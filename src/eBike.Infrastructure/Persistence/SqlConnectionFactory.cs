using System.Data;
using eBike.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace eBike.Infrastructure.Persistence;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = DbConnectionResolver.ResolveConnectionString(configuration);
    }

    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
