using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace eBike.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration["Jwt:SecretKey"] ?? configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
        {
            throw new InvalidOperationException("Fatal Security Error: 'Jwt:SecretKey' must be set via environment variables or secure configuration with minimum 256 bits (32 chars).");
        }

        var issuer = configuration["Jwt:Issuer"] ?? "eBikeBackend";
        var audience = configuration["Jwt:Audience"] ?? "eBikeClientPortals";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Set to true in strict production SSL terminations
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
        });

        return services;
    }

    public static IServiceCollection AddSecurityRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global partitioned rate limiter per IP address
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_client";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientIp,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 120, // 120 requests per minute
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10
                    });
            });
        });

        return services;
    }
}

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // 1. Must be authenticated
        if (httpContext.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        // 2. Strict Role Enforcement: Only designated Administrators can access background execution telemetry
        var isAdmin = httpContext.User.IsInRole("Admin") || 
                      httpContext.User.IsInRole("SuperAdmin") ||
                      httpContext.User.IsInRole("superadmin");

        return isAdmin;
    }
}

