using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Getir.WebApi.HealthChecks;

/// <summary>
/// Health check for Redis cache availability and performance
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(IConnectionMultiplexer? redis, ILogger<RedisHealthCheck> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // If Redis is not configured or connection is null
            if (_redis == null)
            {
                return HealthCheckResult.Degraded(
                    "Redis is not configured. Using MemoryCache fallback.",
                    data: new Dictionary<string, object>
                    {
                        { "status", "not_configured" },
                        { "fallback", "memory_cache" }
                    });
            }

            // Check if Redis is connected
            if (!_redis.IsConnected)
            {
                return HealthCheckResult.Degraded(
                    "Redis is not connected. Using MemoryCache fallback.",
                    data: new Dictionary<string, object>
                    {
                        { "status", "disconnected" },
                        { "fallback", "memory_cache" }
                    });
            }

            // Try to PING Redis
            var db = _redis.GetDatabase();
            var pingTime = await db.PingAsync();

            // Check ping time
            if (pingTime.TotalMilliseconds > 1000)
            {
                return HealthCheckResult.Degraded(
                    $"Redis is slow. Ping: {pingTime.TotalMilliseconds}ms",
                    data: new Dictionary<string, object>
                    {
                        { "status", "slow" },
                        { "ping_ms", pingTime.TotalMilliseconds },
                        { "threshold_ms", 1000 }
                    });
            }

            // Get server info
            var endpoints = _redis.GetEndPoints();
            var serverInfo = new List<object>();

            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                serverInfo.Add(new
                {
                    endpoint = endpoint.ToString(),
                    is_connected = server.IsConnected,
                    is_replica = server.IsReplica,
                    server_type = server.ServerType.ToString()
                });
            }

            return HealthCheckResult.Healthy(
                $"Redis is healthy. Ping: {pingTime.TotalMilliseconds}ms",
                data: new Dictionary<string, object>
                {
                    { "status", "healthy" },
                    { "ping_ms", pingTime.TotalMilliseconds },
                    { "servers", serverInfo.Count },
                    { "endpoints", serverInfo }
                });
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis health check failed: connection error");

            return HealthCheckResult.Degraded(
                "Redis connection error. Using MemoryCache fallback.",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "status", "connection_error" },
                    { "error", ex.Message },
                    { "fallback", "memory_cache" }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed: unexpected error");

            return HealthCheckResult.Unhealthy(
                "Redis health check failed unexpectedly.",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "status", "error" },
                    { "error", ex.Message }
                });
        }
    }
}

