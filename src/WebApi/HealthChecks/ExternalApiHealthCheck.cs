using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace Getir.WebApi.HealthChecks;

/// <summary>
/// Health check for external API connectivity and network status
/// </summary>
public class ExternalApiHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExternalApiHealthCheck> _logger;

    public ExternalApiHealthCheck(HttpClient httpClient, IConfiguration configuration, ILogger<ExternalApiHealthCheck> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Check internet connectivity with multiple DNS servers
            var pingTargets = new[]
            {
                ("Google DNS", "8.8.8.8"),
                ("Cloudflare DNS", "1.1.1.1")
            };

            var pingResults = new Dictionary<string, object>();
            var successfulPings = 0;
            var totalRoundtripTime = 0L;

            using var ping = new Ping();
            foreach (var (name, ip) in pingTargets)
            {
                try
                {
                    var pingResult = await ping.SendPingAsync(ip, 3000);
                    var isSuccess = pingResult.Status == IPStatus.Success;

                    pingResults[$"{name}_status"] = pingResult.Status.ToString();
                    pingResults[$"{name}_roundtrip_ms"] = pingResult.RoundtripTime;

                    if (isSuccess)
                    {
                        successfulPings++;
                        totalRoundtripTime += pingResult.RoundtripTime;
                    }
                }
                catch (Exception ex)
                {
                    pingResults[$"{name}_error"] = ex.Message;
                    _logger.LogWarning(ex, "Failed to ping {Name} ({IP})", name, ip);
                }
            }

            stopwatch.Stop();

            var averageRoundtripTime = successfulPings > 0
                ? totalRoundtripTime / successfulPings
                : 0;

            var data = new Dictionary<string, object>
            {
                { "internet_connectivity", successfulPings > 0 },
                { "successful_pings", successfulPings },
                { "total_ping_targets", pingTargets.Length },
                { "average_roundtrip_ms", averageRoundtripTime },
                { "check_duration_ms", stopwatch.ElapsedMilliseconds },
                { "timestamp", DateTime.UtcNow }
            };

            // Add ping results
            foreach (var kvp in pingResults)
            {
                data[kvp.Key] = kvp.Value;
            }

            // Determine health status
            if (successfulPings == pingTargets.Length)
            {
                return HealthCheckResult.Healthy(
                    $"External connectivity is excellent. Average latency: {averageRoundtripTime}ms",
                    data);
            }
            else if (successfulPings > 0)
            {
                return HealthCheckResult.Degraded(
                    $"Partial external connectivity. {successfulPings}/{pingTargets.Length} targets reachable",
                    data: data);
            }
            else
            {
                return HealthCheckResult.Unhealthy(
                    "No external connectivity detected",
                    data: data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External API health check failed");
            return HealthCheckResult.Unhealthy(
                "Unable to check external connectivity",
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
