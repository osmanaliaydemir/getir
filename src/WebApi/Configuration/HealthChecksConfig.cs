using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Getir.WebApi.HealthChecks;
using System.Diagnostics;

namespace Getir.WebApi.Configuration;

/// <summary>
/// Global-standard health checks configuration for enterprise applications
/// Includes: Database, Redis, SignalR, Memory, Disk, External APIs
/// Supports: Kubernetes liveness/readiness probes, Health Check UI
/// </summary>
public static class HealthChecksConfig
{
    private static readonly DateTime StartupTime = DateTime.UtcNow;
    private static readonly Stopwatch StartupStopwatch = Stopwatch.StartNew();

    public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

        var redisConfiguration = configuration.GetConnectionString("Redis");
        var isRedisEnabled = configuration.GetValue<bool>("Redis:Enabled", false);

        // ============= HEALTH CHECKS REGISTRATION =============
        var healthChecksBuilder = services.AddHealthChecks();

        // 1. Database Health Check (SQL Server Connection)
        if (configuration.GetValue<bool>("HealthChecks:Database:Enabled", true))
        {
            var dbTags = configuration.GetSection("HealthChecks:Database:Tags").Get<string[]>()
                ?? new[] { "db", "sqlserver", "ready" };
            var dbTimeout = TimeSpan.FromSeconds(configuration.GetValue<int>("HealthChecks:Database:TimeoutSeconds", 5));

            healthChecksBuilder.AddSqlServer(
                connectionString,
                name: "sql_server",
                failureStatus: HealthStatus.Unhealthy,
                tags: dbTags,
                timeout: dbTimeout);
        }

        // 2. DbContext Health Check (EF Core + Migrations)
        if (configuration.GetValue<bool>("HealthChecks:DbContext:Enabled", true))
        {
            var dbContextTags = configuration.GetSection("HealthChecks:DbContext:Tags").Get<string[]>()
                ?? new[] { "db", "ef", "ready" };
            var dbContextTimeout = TimeSpan.FromSeconds(configuration.GetValue<int>("HealthChecks:DbContext:TimeoutSeconds", 10));

            healthChecksBuilder.AddCheck<DbContextHealthCheck>(
                "db_context",
                failureStatus: HealthStatus.Unhealthy,
                tags: dbContextTags,
                timeout: dbContextTimeout);
        }

        // 3. Redis Health Check
        if (configuration.GetValue<bool>("HealthChecks:Redis:Enabled", true) && isRedisEnabled && !string.IsNullOrEmpty(redisConfiguration))
        {
            var redisTags = configuration.GetSection("HealthChecks:Redis:Tags").Get<string[]>()
                ?? new[] { "cache", "redis", "ready" };
            var redisTimeout = TimeSpan.FromSeconds(configuration.GetValue<int>("HealthChecks:Redis:TimeoutSeconds", 5));

            // Use official Redis health check
            healthChecksBuilder.AddRedis(
                redisConfiguration,
                name: "redis",
                failureStatus: HealthStatus.Degraded, // Degraded not Unhealthy (we have MemoryCache fallback)
                tags: redisTags,
                timeout: redisTimeout);
        }

        // 4. SignalR Health Check
        if (configuration.GetValue<bool>("HealthChecks:SignalR:Enabled", true))
        {
            var signalRTags = configuration.GetSection("HealthChecks:SignalR:Tags").Get<string[]>()
                ?? new[] { "signalr", "realtime", "ready" };

            healthChecksBuilder.AddCheck<SignalRHealthCheck>(
                "signalr_hubs",
                failureStatus: HealthStatus.Degraded,
                tags: signalRTags);
        }

        // 5. Memory Health Check
        if (configuration.GetValue<bool>("HealthChecks:Memory:Enabled", true))
        {
            var memoryTags = configuration.GetSection("HealthChecks:Memory:Tags").Get<string[]>()
                ?? new[] { "memory", "system" };

            healthChecksBuilder.AddCheck<MemoryHealthCheck>(
                "memory_usage",
                failureStatus: HealthStatus.Degraded,
                tags: memoryTags);
        }

        // 6. Disk Space Health Check
        if (configuration.GetValue<bool>("HealthChecks:DiskSpace:Enabled", true))
        {
            var diskTags = configuration.GetSection("HealthChecks:DiskSpace:Tags").Get<string[]>()
                ?? new[] { "disk", "system" };

            healthChecksBuilder.AddCheck<DiskSpaceHealthCheck>(
                "disk_space",
                failureStatus: HealthStatus.Degraded,
                tags: diskTags);
        }

        // 7. External APIs Health Check
        if (configuration.GetValue<bool>("HealthChecks:ExternalApis:Enabled", true))
        {
            var externalTags = configuration.GetSection("HealthChecks:ExternalApis:Tags").Get<string[]>()
                ?? new[] { "external", "network" };
            var externalTimeout = TimeSpan.FromSeconds(configuration.GetValue<int>("HealthChecks:ExternalApis:TimeoutSeconds", 10));

            healthChecksBuilder.AddCheck<ExternalApiHealthCheck>(
                "external_connectivity",
                failureStatus: HealthStatus.Degraded,
                tags: externalTags,
                timeout: externalTimeout);
        }

        // 8. Startup Time Health Check
        var startupDuration = StartupStopwatch.Elapsed;
        healthChecksBuilder.AddCheck(
            "startup_time",
            () => new StartupTimeHealthCheck(StartupTime, startupDuration).CheckHealthAsync(
                new HealthCheckContext()).Result,
            tags: new[] { "startup", "live" });

        // ============= HEALTH CHECKS UI (Optional Dashboard) =============
        var isHealthCheckUIEnabled = configuration.GetValue<bool>("HealthChecksUI:Enabled", false);
        if (isHealthCheckUIEnabled)
        {
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(configuration.GetValue<int>("HealthChecksUI:EvaluationTimeInSeconds", 30));
                setup.SetMinimumSecondsBetweenFailureNotifications(configuration.GetValue<int>("HealthChecksUI:MinimumSecondsBetweenFailureNotifications", 60));

                // Add health check endpoints to monitor
                var healthCheckEndpoints = configuration.GetSection("HealthChecksUI:HealthChecks").Get<HealthCheckEndpoint[]>();
                if (healthCheckEndpoints != null)
                {
                    foreach (var endpoint in healthCheckEndpoints)
                    {
                        setup.AddHealthCheckEndpoint(endpoint.Name, endpoint.Uri);
                    }
                }
            })
            .AddInMemoryStorage(); // Use SQL Server storage in production for persistence
        }

        return services;
    }

    public static WebApplication UseHealthChecksConfiguration(this WebApplication app)
    {
        var configuration = app.Configuration;

        // ============= HEALTH CHECK ENDPOINTS =============

        // 1. Main Health Check Endpoint (Detailed JSON)
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true, // All health checks
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse, // Beautiful JSON format
            AllowCachingResponses = false
        });

        // 2. Liveness Probe (Kubernetes) - Is the app running?
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            AllowCachingResponses = false
        });

        // 3. Readiness Probe (Kubernetes) - Is the app ready to serve traffic?
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            AllowCachingResponses = false
        });

        // 4. Startup Probe (Kubernetes) - Has the app started successfully?
        app.MapHealthChecks("/health/startup", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("startup") || check.Tags.Contains("live"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            AllowCachingResponses = false
        });

        // 5. Simple Health Check (Just status, no details)
        app.MapHealthChecks("/health/simple", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString().ToLower(),
                    timestamp = DateTime.UtcNow
                });
                await context.Response.WriteAsync(result);
            }
        });

        // 6. Health Check UI (Visual Dashboard)
        var isHealthCheckUIEnabled = configuration.GetValue<bool>("HealthChecksUI:Enabled", false);
        if (isHealthCheckUIEnabled)
        {
            var uiPath = configuration.GetValue<string>("HealthChecksUI:UIPath", "/health-ui");
            var apiPath = configuration.GetValue<string>("HealthChecksUI:ApiPath", "/health-api");

            app.MapHealthChecksUI(options =>
            {
                options.UIPath = uiPath;
                options.ApiPath = apiPath;
            });
        }

        return app;
    }

    // Helper class for configuration binding
    private class HealthCheckEndpoint
    {
        public string Name { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
    }
}

