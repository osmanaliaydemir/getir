using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Getir.Infrastructure.Persistence;

namespace Getir.WebApi.HealthChecks;

/// <summary>
/// Advanced health check for DbContext with query execution and migration status
/// </summary>
public class DbContextHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DbContextHealthCheck> _logger;

    public DbContextHealthCheck(AppDbContext dbContext, ILogger<DbContextHealthCheck> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var startTime = DateTime.UtcNow;

            // Check 1: Can we connect to the database?
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy(
                    "Cannot connect to database",
                    data: new Dictionary<string, object>
                    {
                        { "can_connect", false },
                        { "timestamp", DateTime.UtcNow }
                    });
            }

            // Check 2: Execute a simple query to test read operations
            var queryStartTime = DateTime.UtcNow;
            var userCount = await _dbContext.Users.CountAsync(cancellationToken);
            var queryDuration = (DateTime.UtcNow - queryStartTime).TotalMilliseconds;

            // Check 3: Check pending migrations
            var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            var hasPendingMigrations = pendingMigrations.Any();

            // Check 4: Get applied migrations
            var appliedMigrations = await _dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);

            var totalDuration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            var data = new Dictionary<string, object>
            {
                { "can_connect", true },
                { "user_count", userCount },
                { "query_duration_ms", Math.Round(queryDuration, 2) },
                { "total_duration_ms", Math.Round(totalDuration, 2) },
                { "has_pending_migrations", hasPendingMigrations },
                { "pending_migrations_count", pendingMigrations.Count() },
                { "applied_migrations_count", appliedMigrations.Count() },
                { "database_provider", _dbContext.Database.ProviderName },
                { "timestamp", DateTime.UtcNow }
            };

            // Determine health status
            if (hasPendingMigrations)
            {
                data["pending_migrations"] = pendingMigrations.ToList();
                return HealthCheckResult.Degraded(
                    $"Database has {pendingMigrations.Count()} pending migrations",
                    data: data);
            }

            // Check query performance
            if (queryDuration > 1000) // More than 1 second
            {
                return HealthCheckResult.Degraded(
                    $"Database query is slow: {queryDuration}ms",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"Database is healthy. Query executed in {queryDuration}ms",
                data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy(
                "Database health check failed",
                ex,
                new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { "error_type", ex.GetType().Name },
                    { "timestamp", DateTime.UtcNow }
                });
        }
    }
}

