using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.IO;

namespace Getir.WebApi.HealthChecks;

/// <summary>
/// Health check for disk space monitoring with configurable thresholds
/// </summary>
public class DiskSpaceHealthCheck : IHealthCheck
{
    private readonly long _threshold;
    private readonly ILogger<DiskSpaceHealthCheck> _logger;

    public DiskSpaceHealthCheck(IConfiguration configuration, ILogger<DiskSpaceHealthCheck> logger)
    {
        _threshold = configuration.GetValue<long>("HealthChecks:DiskSpace:ThresholdBytes", 1024L * 1024L * 1024L); // 1GB default
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var drive = new DriveInfo(Directory.GetCurrentDirectory());
            var freeSpace = drive.AvailableFreeSpace;
            var totalSpace = drive.TotalSize;
            var usedSpace = totalSpace - freeSpace;
            var usagePercentage = (double)usedSpace / totalSpace * 100;
            var freePercentage = 100 - usagePercentage;

            var data = new Dictionary<string, object>
            {
                { "free_space_bytes", freeSpace },
                { "free_space_gb", Math.Round(freeSpace / (1024.0 * 1024.0 * 1024.0), 2) },
                { "total_space_bytes", totalSpace },
                { "total_space_gb", Math.Round(totalSpace / (1024.0 * 1024.0 * 1024.0), 2) },
                { "used_space_bytes", usedSpace },
                { "used_space_gb", Math.Round(usedSpace / (1024.0 * 1024.0 * 1024.0), 2) },
                { "usage_percentage", Math.Round(usagePercentage, 2) },
                { "free_percentage", Math.Round(freePercentage, 2) },
                { "threshold_bytes", _threshold },
                { "threshold_gb", Math.Round(_threshold / (1024.0 * 1024.0 * 1024.0), 2) },
                { "drive_name", drive.Name },
                { "drive_format", drive.DriveFormat },
                { "drive_type", drive.DriveType.ToString() },
                { "volume_label", drive.VolumeLabel },
                { "timestamp", DateTime.UtcNow }
            };

            // Determine health status
            HealthStatus status;
            string description;

            if (freeSpace > _threshold * 2) // More than 2x threshold
            {
                status = HealthStatus.Healthy;
                description = $"Disk space is healthy: {Math.Round(freeSpace / (1024.0 * 1024.0 * 1024.0), 2)}GB free ({Math.Round(freePercentage, 2)}%)";
            }
            else if (freeSpace > _threshold) // Between 1x and 2x threshold
            {
                status = HealthStatus.Degraded;
                description = $"Disk space is getting low: {Math.Round(freeSpace / (1024.0 * 1024.0 * 1024.0), 2)}GB free ({Math.Round(freePercentage, 2)}%)";
                _logger.LogWarning("Disk space is getting low: {FreeSpaceGB}GB free", Math.Round(freeSpace / (1024.0 * 1024.0 * 1024.0), 2));
            }
            else // Below threshold
            {
                status = HealthStatus.Unhealthy;
                description = $"Disk space is critically low: {Math.Round(freeSpace / (1024.0 * 1024.0 * 1024.0), 2)}GB free ({Math.Round(freePercentage, 2)}%)";
                _logger.LogError("Disk space is critically low: {FreeSpaceGB}GB free", Math.Round(freeSpace / (1024.0 * 1024.0 * 1024.0), 2));
            }

            return Task.FromResult(new HealthCheckResult(status, description, null, data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Disk space health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Unable to check disk space",
                ex,
                new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { "timestamp", DateTime.UtcNow }
                }));
        }
    }
}
