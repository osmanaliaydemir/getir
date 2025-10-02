using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.NetworkInformation;

namespace Getir.WebApi.HealthChecks;

public class ExternalApiHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;

    public ExternalApiHealthCheck(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check internet connectivity
            using var ping = new Ping();
            var pingResult = await ping.SendPingAsync("8.8.8.8", 5000);
            
            var data = new Dictionary<string, object>
            {
                { "internet_connectivity", pingResult.Status == IPStatus.Success },
                { "ping_roundtrip_time", pingResult.RoundtripTime }
            };

            if (pingResult.Status == IPStatus.Success)
            {
                return new HealthCheckResult(HealthStatus.Healthy,
                    "External connectivity is working",
                    null,
                    data);
            }
            else
            {
                return new HealthCheckResult(HealthStatus.Degraded,
                    "External connectivity issues detected",
                    null,
                    data);
            }
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(HealthStatus.Unhealthy,
                "Unable to check external connectivity",
                ex);
        }
    }
}
