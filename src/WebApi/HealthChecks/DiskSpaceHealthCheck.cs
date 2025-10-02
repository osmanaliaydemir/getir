using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.IO;

namespace Getir.WebApi.HealthChecks;

public class DiskSpaceHealthCheck : IHealthCheck
{
    private readonly long _threshold;

    public DiskSpaceHealthCheck(long threshold = 1024L * 1024L * 1024L) // 1GB default
    {
        _threshold = threshold;
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

            var data = new Dictionary<string, object>
            {
                { "free_space", freeSpace },
                { "total_space", totalSpace },
                { "used_space", usedSpace },
                { "usage_percentage", Math.Round(usagePercentage, 2) },
                { "drive_name", drive.Name }
            };

            var status = freeSpace > _threshold ? HealthStatus.Healthy : HealthStatus.Degraded;

            return Task.FromResult(new HealthCheckResult(status,
                status == HealthStatus.Healthy
                    ? $"Disk space is sufficient. {freeSpace / (1024 * 1024 * 1024)}GB available"
                    : $"Disk space is low. {freeSpace / (1024 * 1024 * 1024)}GB available",
                null,
                data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(HealthStatus.Unhealthy,
                "Unable to check disk space",
                ex));
        }
    }
}
