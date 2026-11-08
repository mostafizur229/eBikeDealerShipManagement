using eBike.API.Converters;
using eBike.API.Extensions;
using eBike.API.Filters;
using eBike.API.Hubs;
using eBike.API.Middlewares;
using eBike.Application;
using eBike.Application.Common.Interfaces;
using eBike.Infrastructure;
using eBike.Infrastructure.Persistence;
using Hangfire;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Global Response Formatting
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AutoWrapApiResponseFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateTimeUtcJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new NullableDateTimeUtcJsonConverter());
});

builder.Services.AddSignalR();
builder.Services.AddHealthChecks();

// Security, JWT Auth & Partitioned Rate Limiting
builder.Services.AddSecurityRateLimiter();
builder.Services.AddJwtAuthentication(builder.Configuration);

// Swagger / OpenAPI Configuration with Bearer Authorization Security Scheme
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "eBike Management Enterprise API",
        Version = "v1",
        Description = "Enterprise Clean Architecture Backend for e-Bike Inventory & Service Management."
    });

    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token format: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Cross-Origin Resource Sharing (CORS)
var allowedCorsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientPortals", policy =>
    {
        policy.WithOrigins(allowedCorsOrigins ?? ["http://localhost:3000", "http://localhost:5173"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Layer Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger / OpenAPI documentation UI
app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "eBike Management Enterprise API v1");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
});

// Redirect root URL directly to Swagger UI (relative to application base path)
app.MapGet("/", () => Results.Redirect("swagger"));

// Only enforce HTTPS redirection if explicitly enabled in configuration
if (builder.Configuration.GetValue<bool>("EnableHttpsRedirection", false))
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowClientPortals");
app.UseStaticFiles();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Hangfire Background Event Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "eBike Enterprise Jobs Dashboard",
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.MapHealthChecks("/health");
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

// Verify Database connection on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (await context.Database.CanConnectAsync())
        {
            var userCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(context.Users);
            logger.LogInformation("Connected successfully to ebikeimsweb database. Total existing users in database: {UserCount}", userCount);

            try
            {
                await context.Database.ExecuteSqlRawAsync(@"
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ServiceTickets_ServiceTechnicians')
                    BEGIN
                        ALTER TABLE [dbo].[ServiceTickets] DROP CONSTRAINT [FK_ServiceTickets_ServiceTechnicians];
                    END");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Note: Unable to auto-drop FK_ServiceTickets_ServiceTechnicians constraint.");
            }
        }
        else
        {
            logger.LogWarning("Could not establish initial connection to ebikeimsweb database. Please verify SQL Server is running.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while verifying the database connection on startup.");
    }
}

app.Run();
