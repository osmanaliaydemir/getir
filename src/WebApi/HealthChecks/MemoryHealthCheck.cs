using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Getir.WebApi.HealthChecks;

/// <summary>
/// Health check for memory usage monitoring with configurable thresholds
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly long _threshold;
    private readonly ILogger<MemoryHealthCheck> _logger;

    public MemoryHealthCheck(IConfiguration configuration, ILogger<MemoryHealthCheck> logger)
    {
        _threshold = configuration.GetValue<long>("HealthChecks:Memory:ThresholdBytes", 1024L * 1024L * 1024L); // 1GB default
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get GC memory info
            var gcInfo = GC.GetGCMemoryInfo();
            var allocated = GC.GetTotalMemory(false);
            var totalMemory = gcInfo.TotalAvailableMemoryBytes;
            var heapSize = gcInfo.HeapSizeBytes;
            var fragmentedBytes = gcInfo.FragmentedBytes;

            // Calculate percentages
            var usagePercentage = totalMemory > 0
                ? (double)allocated / totalMemory * 100
                : 0;

            var data = new Dictionary<string, object>
            {
                { "allocated_bytes", allocated },
                { "allocated_mb", Math.Round(allocated / 1024.0 / 1024.0, 2) },
                { "threshold_bytes", _threshold },
                { "threshold_mb", Math.Round(_threshold / 1024.0 / 1024.0, 2) },
                { "usage_percentage", Math.Round(usagePercentage, 2) },
                { "total_available_memory_bytes", totalMemory },
                { "heap_size_bytes", heapSize },
                { "heap_size_mb", Math.Round(heapSize / 1024.0 / 1024.0, 2) },
                { "fragmented_bytes", fragmentedBytes },
                { "gen0_collections", GC.CollectionCount(0) },
                { "gen1_collections", GC.CollectionCount(1) },
                { "gen2_collections", GC.CollectionCount(2) },
                { "timestamp", DateTime.UtcNow }
            };

            // Determine status based on allocated memory
            HealthStatus status;
            string description;

            if (allocated < _threshold * 0.7) // Below 70% threshold
            {
                status = HealthStatus.Healthy;
                description = $"Memory usage is healthy: {Math.Round(allocated / 1024.0 / 1024.0, 2)}MB ({Math.Round(usagePercentage, 2)}%)";
            }
            else if (allocated < _threshold) // Between 70% and 100% threshold
            {
                status = HealthStatus.Degraded;
                description = $"Memory usage is elevated: {Math.Round(allocated / 1024.0 / 1024.0, 2)}MB ({Math.Round(usagePercentage, 2)}%)";
                _logger.LogWarning("Memory usage is elevated: {AllocatedMB}MB", Math.Round(allocated / 1024.0 / 1024.0, 2));
            }
            else // Above threshold
            {
                status = HealthStatus.Unhealthy;
                description = $"Memory usage is critical: {Math.Round(allocated / 1024.0 / 1024.0, 2)}MB ({Math.Round(usagePercentage, 2)}%)";
                _logger.LogError("Memory usage is critical: {AllocatedMB}MB", Math.Round(allocated / 1024.0 / 1024.0, 2));
            }

            return Task.FromResult(new HealthCheckResult(status, description, null, data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Memory health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Failed to check memory status",
                ex,
                new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { "timestamp", DateTime.UtcNow }
                }));
        }
    }
}
