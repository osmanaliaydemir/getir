using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Getir.WebApi.HealthChecks;

public class MemoryHealthCheck : IHealthCheck
{
    private readonly long _threshold;

    public MemoryHealthCheck(long threshold = 1024L * 1024L * 1024L) // 1GB default
    {
        _threshold = threshold;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var allocated = GC.GetTotalMemory(false);
        var data = new Dictionary<string, object>
        {
            { "allocated", allocated },
            { "gen0_collections", GC.CollectionCount(0) },
            { "gen1_collections", GC.CollectionCount(1) },
            { "gen2_collections", GC.CollectionCount(2) }
        };

        var status = allocated < _threshold ? HealthStatus.Healthy : HealthStatus.Degraded;

        return Task.FromResult(new HealthCheckResult(status, 
            status == HealthStatus.Healthy 
                ? "Memory usage is within acceptable limits" 
                : "Memory usage is high", 
            null, 
            data));
    }
}
