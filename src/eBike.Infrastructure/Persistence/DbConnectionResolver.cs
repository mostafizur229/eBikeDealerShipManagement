using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace eBike.Infrastructure.Persistence;

/// <summary>
/// Intelligent SQL Server Connection Resolver.
/// Automatically detects active local SQL Server instances (., .\SQLEXPRESS, (localdb)\mssqllocaldb, localhost, etc.)
/// and ensures automatic connectivity on any developer's PC without manual connection string changes.
/// </summary>
public static class DbConnectionResolver
{
    private static string? _resolvedConnectionString;
    private static readonly object _lock = new();

    public static string ResolveConnectionString(IConfiguration configuration, ILogger? logger = null)
    {
        if (!string.IsNullOrEmpty(_resolvedConnectionString))
        {
            return _resolvedConnectionString;
        }

        lock (_lock)
        {
            if (!string.IsNullOrEmpty(_resolvedConnectionString))
            {
                return _resolvedConnectionString;
            }

            var configuredConn = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=.;Database=ebikeimswebcore;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

            SqlConnectionStringBuilder configuredBuilder;
            try
            {
                configuredBuilder = new SqlConnectionStringBuilder(configuredConn);
            }
            catch
            {
                configuredBuilder = new SqlConnectionStringBuilder("Server=.;Database=ebikeimswebcore;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;");
            }

            string dbName = string.IsNullOrWhiteSpace(configuredBuilder.InitialCatalog) ? "ebikeimswebcore" : configuredBuilder.InitialCatalog;

            // If the connection string specifies explicit SQL credentials (User ID / Password), use it directly!
            if (!configuredBuilder.IntegratedSecurity && !string.IsNullOrWhiteSpace(configuredBuilder.UserID))
            {
                _resolvedConnectionString = configuredBuilder.ConnectionString;
                return _resolvedConnectionString;
            }

            // 1. First test if the configured server connects directly
            if (TryConnect(configuredBuilder.ConnectionString, 2))
            {
                _resolvedConnectionString = configuredBuilder.ConnectionString;
                logger?.LogInformation("[DbConnectionResolver] Connected using configured connection string on server '{Server}'.", configuredBuilder.DataSource);
                EnsureDatabaseExists(_resolvedConnectionString, dbName, logger);
                return _resolvedConnectionString;
            }

            // Also test if configured server connects to master (in case database doesn't exist yet)
            var configuredMaster = new SqlConnectionStringBuilder(configuredBuilder.ConnectionString) { InitialCatalog = "master", ConnectTimeout = 2 };
            if (TryConnect(configuredMaster.ConnectionString, 2))
            {
                _resolvedConnectionString = configuredBuilder.ConnectionString;
                logger?.LogInformation("[DbConnectionResolver] Connected to server '{Server}'. Ensuring database '{DbName}' exists.", configuredBuilder.DataSource, dbName);
                EnsureDatabaseExists(_resolvedConnectionString, dbName, logger);
                return _resolvedConnectionString;
            }

            // 2. Configured server failed (e.g. NAHIDEMON or other machine-specific name on another PC).
            // Probe common local SQL Server instances in order of prevalence:
            var candidateServers = new List<string>
            {
                @".\SQLEXPRESS",
                ".",
                "localhost",
                @"localhost\SQLEXPRESS",
                @"(localdb)\mssqllocaldb",
                "(local)",
                Environment.MachineName,
                $@"{Environment.MachineName}\SQLEXPRESS"
            };

            // Environment variable override if specified by developer or docker
            var envServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? Environment.GetEnvironmentVariable("SQL_SERVER");
            if (!string.IsNullOrWhiteSpace(envServer) && !candidateServers.Contains(envServer))
            {
                candidateServers.Insert(0, envServer);
            }

            foreach (var server in candidateServers)
            {
                var testBuilder = new SqlConnectionStringBuilder
                {
                    DataSource = server,
                    InitialCatalog = "master",
                    IntegratedSecurity = true,
                    TrustServerCertificate = true,
                    MultipleActiveResultSets = true,
                    ConnectTimeout = 2
                };

                if (TryConnect(testBuilder.ConnectionString, 2))
                {
                    var finalBuilder = new SqlConnectionStringBuilder(testBuilder.ConnectionString)
                    {
                        InitialCatalog = dbName,
                        ConnectTimeout = 15
                    };

                    _resolvedConnectionString = finalBuilder.ConnectionString;
                    logger?.LogInformation("[DbConnectionResolver] Auto-detected local SQL Server instance: '{Server}'. Target database: '{DbName}'.", server, dbName);
                    
                    EnsureDatabaseExists(_resolvedConnectionString, dbName, logger);
                    return _resolvedConnectionString;
                }
            }

            // 3. Fallback to standard local connection string if probing couldn't establish a ping
            var fallback = new SqlConnectionStringBuilder
            {
                DataSource = ".",
                InitialCatalog = dbName,
                IntegratedSecurity = true,
                TrustServerCertificate = true,
                MultipleActiveResultSets = true
            }.ConnectionString;

            _resolvedConnectionString = fallback;
            logger?.LogWarning("[DbConnectionResolver] Auto-detection could not find an active instance. Falling back to default: '{Fallback}'.", fallback);
            return _resolvedConnectionString;
        }
    }

    private static bool TryConnect(string connectionString, int timeoutSeconds)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                ConnectTimeout = timeoutSeconds
            };

            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void EnsureDatabaseExists(string connectionString, string databaseName, ILogger? logger)
    {
        try
        {
            var masterBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master",
                ConnectTimeout = 3
            };

            using var connection = new SqlConnection(masterBuilder.ConnectionString);
            connection.Open();

            using var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(1) FROM sys.databases WHERE name = @name";
            checkCmd.Parameters.AddWithValue("@name", databaseName);

            var count = Convert.ToInt32(checkCmd.ExecuteScalar());
            if (count == 0)
            {
                using var createCmd = connection.CreateCommand();
                createCmd.CommandText = $"CREATE DATABASE [{databaseName}]";
                createCmd.ExecuteNonQuery();
                logger?.LogInformation("[DbConnectionResolver] Database '{DbName}' did not exist. Successfully created automatically.", databaseName);
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "[DbConnectionResolver] Note: Could not auto-verify/create database '{DbName}'.", databaseName);
        }
    }
}
