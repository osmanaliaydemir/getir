using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.NetworkInformation;
using Getir.WebApi.HealthChecks;

namespace Getir.WebApi.Configuration;

public static class HealthChecksConfig
{
    public static IServiceCollection AddHealthChecksConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

        services.AddHealthChecks()
            .AddSqlServer(connectionString, name: "database", tags: new[] { "db", "sqlserver" })
            .AddCheck<MemoryHealthCheck>("memory", tags: new[] { "memory" })
            .AddCheck<DiskSpaceHealthCheck>("disk_space", tags: new[] { "disk" })
            .AddCheck<ExternalApiHealthCheck>("external_apis", tags: new[] { "external" })
            .AddCheck<RedisHealthCheck>("redis_cache", 
                failureStatus: HealthStatus.Degraded, // Degraded instead of Unhealthy (fallback to MemoryCache)
                tags: new[] { "cache", "redis" });

        return services;
    }

    public static WebApplication UseHealthChecksConfiguration(this WebApplication app)
    {
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        duration = e.Value.Duration.ToString()
                    }),
                    totalDuration = report.TotalDuration.ToString()
                });
                await context.Response.WriteAsync(result);
            }
        });

        return app;
    }
}
