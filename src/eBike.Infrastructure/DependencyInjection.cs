using System;
using eBike.Application.Common.Interfaces;
using eBike.Infrastructure.Configuration;
using eBike.Infrastructure.Persistence;
using eBike.Infrastructure.Persistence.Behaviors;
using eBike.Infrastructure.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eBike.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = DbConnectionResolver.ResolveConnectionString(configuration);

        // Persistence / EF Core mapping to ebikeimsweb
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            }));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<ISqlConnectionFactory>(new SqlConnectionFactory(connectionString));

        // Transaction Pipeline Behavior
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // Current User Accessor
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        // Caching
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddScoped<IRolePermissionCacheService, RolePermissionCacheService>();

        // Security & Cryptography
        services.AddTransient<ICryptographyService, CryptographyService>();
        services.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();

        // File Storage & Email
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.AddTransient<IEmailService, EmailService>();

        // Financial & Banking Services
        services.AddScoped<IBankBalanceService, BankBalanceService>();
        services.AddScoped<ICashDueService, CashDueService>();
        services.AddScoped<IPurchaseOrderExecutionService, PurchaseOrderExecutionService>();
        services.AddScoped<ISalesOrderExecutionService, SalesOrderExecutionService>();
        services.AddScoped<ICreditSalesOrderExecutionService, CreditSalesOrderExecutionService>();

        // In-Memory Asynchronous Event Bus & Background Worker
        services.AddSingleton<IEventBus, Messaging.ChannelEventBus>();
        services.AddHostedService<Messaging.EventBusBackgroundService>();

        // Hangfire Background Job Processing
        services.AddHangfire(cfg =>
        {
            cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
               .UseSimpleAssemblyNameTypeSerializer()
               .UseRecommendedSerializerSettings();

            string infraMode = configuration["INFRASTRUCTURE_MODE"] ?? "InMemory";
            if (infraMode.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                cfg.UseMemoryStorage();
            }
            else
            {
                cfg.UseSqlServerStorage(connectionString);
            }
        });

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Math.Max(Environment.ProcessorCount * 2, 4);
        });

        return services;
    }
}
