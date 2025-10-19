using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Getir.WebApi.HealthChecks;

/// <summary>
/// Health check for application startup time and readiness
/// </summary>
public class StartupTimeHealthCheck : IHealthCheck
{
    private readonly DateTime _startupTime;
    private readonly TimeSpan _startupDuration;
    private static readonly TimeSpan MaxStartupTime = TimeSpan.FromMinutes(2);

    public StartupTimeHealthCheck(DateTime startupTime, TimeSpan startupDuration)
    {
        _startupTime = startupTime;
        _startupDuration = startupDuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var uptime = DateTime.UtcNow - _startupTime;

        var data = new Dictionary<string, object>
        {
            { "startup_time", _startupTime.ToString("o") },
            { "startup_duration_seconds", Math.Round(_startupDuration.TotalSeconds, 2) },
            { "uptime_seconds", Math.Round(uptime.TotalSeconds, 2) },
            { "uptime_minutes", Math.Round(uptime.TotalMinutes, 2) },
            { "uptime_hours", Math.Round(uptime.TotalHours, 2) },
            { "uptime_days", Math.Round(uptime.TotalDays, 2) },
            { "is_ready", uptime.TotalSeconds >= 10 }, // Ready after 10 seconds
            { "timestamp", DateTime.UtcNow }
        };

        // Check if startup took too long
        if (_startupDuration > MaxStartupTime)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Application startup was slow: {_startupDuration.TotalSeconds}s",
                data: data));
        }

        // Check if just started (might still be initializing)
        if (uptime.TotalSeconds < 10)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                "Application is still starting up",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Application has been running for {Math.Round(uptime.TotalMinutes, 2)} minutes",
            data));
    }
}

