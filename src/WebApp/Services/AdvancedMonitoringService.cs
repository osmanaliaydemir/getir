using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace WebApp.Services;

public interface IAdvancedMonitoringService
{
    void IncrementCounter(string name, string[]? labels = null, double value = 1);
    void SetGauge(string name, double value, string[]? labels = null);
    void RecordHistogram(string name, double value, string[]? labels = null);
    void RecordSummary(string name, double value, string[]? labels = null);
    IDisposable StartTimer(string name, string[]? labels = null);
    void TrackCustomMetric(string metricName, double value, Dictionary<string, string>? tags = null);
    void TrackBusinessMetric(string metricName, double value, string? category = null);
    void TrackUserAction(string action, string? userId = null, Dictionary<string, string>? properties = null);
    void TrackApiPerformance(string endpoint, string method, int statusCode, double duration);
    void TrackDatabasePerformance(string operation, string table, double duration, bool success);
    void TrackCachePerformance(string operation, string key, double duration, bool hit);
    void TrackError(string errorType, string message, string? userId = null, Dictionary<string, string>? properties = null);
    void TrackSecurityEvent(string eventType, string details, string? userId = null);
    HealthCheckResult CheckSystemHealth();
    Dictionary<string, object> GetSystemMetrics();
    void SendAlert(string alertType, string message, string severity = "warning");
}

public class AdvancedMonitoringService : IAdvancedMonitoringService
{
    private readonly ILogger<AdvancedMonitoringService> _logger;
    private readonly PerformanceCounter? _cpuCounter;
    private readonly PerformanceCounter? _memoryCounter;
    private readonly PerformanceCounter? _diskCounter;

    // In-memory metrics storage (Prometheus alternative)
    private static readonly Dictionary<string, double> _counters = new();
    private static readonly Dictionary<string, double> _gauges = new();
    private static readonly Dictionary<string, List<double>> _histograms = new();
    private static readonly Dictionary<string, List<double>> _summaries = new();

    public AdvancedMonitoringService(ILogger<AdvancedMonitoringService> logger)
    {
        _logger = logger;
        
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
            _diskCounter = new PerformanceCounter("LogicalDisk", "% Disk Time", "C:");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize performance counters");
        }
    }

    public void IncrementCounter(string name, string[]? labels = null, double value = 1)
    {
        try
        {
            var key = labels != null ? $"{name}[{string.Join(",", labels)}]" : name;
            lock (_counters)
            {
                _counters[key] = _counters.GetValueOrDefault(key, 0) + value;
            }
            _logger.LogDebug("Incremented counter: {Name} by {Value}", name, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing counter: {Name}", name);
        }
    }

    public void SetGauge(string name, double value, string[]? labels = null)
    {
        try
        {
            var key = labels != null ? $"{name}[{string.Join(",", labels)}]" : name;
            lock (_gauges)
            {
                _gauges[key] = value;
            }
            _logger.LogDebug("Set gauge: {Name} to {Value}", name, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting gauge: {Name}", name);
        }
    }

    public void RecordHistogram(string name, double value, string[]? labels = null)
    {
        try
        {
            var key = labels != null ? $"{name}[{string.Join(",", labels)}]" : name;
            lock (_histograms)
            {
                if (!_histograms.ContainsKey(key))
                    _histograms[key] = new List<double>();
                _histograms[key].Add(value);
            }
            _logger.LogDebug("Recorded histogram: {Name} with value {Value}", name, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording histogram: {Name}", name);
        }
    }

    public void RecordSummary(string name, double value, string[]? labels = null)
    {
        try
        {
            var key = labels != null ? $"{name}[{string.Join(",", labels)}]" : name;
            lock (_summaries)
            {
                if (!_summaries.ContainsKey(key))
                    _summaries[key] = new List<double>();
                _summaries[key].Add(value);
            }
            _logger.LogDebug("Recorded summary: {Name} with value {Value}", name, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording summary: {Name}", name);
        }
    }

    public IDisposable StartTimer(string name, string[]? labels = null)
    {
        return new TimerScope(this, name, labels);
    }

    public void TrackCustomMetric(string metricName, double value, Dictionary<string, string>? tags = null)
    {
        try
        {
            var labels = tags?.Values.ToArray() ?? new string[0];
            SetGauge(metricName, value, labels);
            _logger.LogDebug("Tracked custom metric: {MetricName} = {Value}", metricName, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking custom metric: {MetricName}", metricName);
        }
    }

    public void TrackBusinessMetric(string metricName, double value, string? category = null)
    {
        try
        {
            SetGauge(metricName, value, new[] { category ?? "default" });
            _logger.LogDebug("Tracked business metric: {MetricName} = {Value} (Category: {Category})", 
                metricName, value, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking business metric: {MetricName}", metricName);
        }
    }

    public void TrackUserAction(string action, string? userId = null, Dictionary<string, string>? properties = null)
    {
        try
        {
            IncrementCounter("business_counter_total", new[] { action, "user_action" });
            _logger.LogDebug("Tracked user action: {Action} by user {UserId}", action, userId ?? "anonymous");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking user action: {Action}", action);
        }
    }

    public void TrackApiPerformance(string endpoint, string method, int statusCode, double duration)
    {
        try
        {
            IncrementCounter("api_requests_total", new[] { endpoint, method, statusCode.ToString() });
            RecordHistogram("api_request_duration_seconds", duration, new[] { endpoint, method });
            _logger.LogDebug("Tracked API performance: {Method} {Endpoint} - {StatusCode} in {Duration}ms", 
                method, endpoint, statusCode, duration * 1000);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking API performance: {Endpoint}", endpoint);
        }
    }

    public void TrackDatabasePerformance(string operation, string table, double duration, bool success)
    {
        try
        {
            IncrementCounter("database_operations_total", new[] { operation, table, success.ToString() });
            RecordHistogram("database_operation_duration_seconds", duration, new[] { operation, table });
            _logger.LogDebug("Tracked database performance: {Operation} on {Table} - {Success} in {Duration}ms", 
                operation, table, success, duration * 1000);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking database performance: {Operation}", operation);
        }
    }

    public void TrackCachePerformance(string operation, string key, double duration, bool hit)
    {
        try
        {
            IncrementCounter("cache_operations_total", new[] { operation, hit.ToString() });
            RecordHistogram("cache_operation_duration_seconds", duration, new[] { operation });
            _logger.LogDebug("Tracked cache performance: {Operation} on {Key} - {Hit} in {Duration}ms", 
                operation, key, hit, duration * 1000);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking cache performance: {Operation}", operation);
        }
    }

    public void TrackError(string errorType, string message, string? userId = null, Dictionary<string, string>? properties = null)
    {
        try
        {
            IncrementCounter("errors_total", new[] { errorType, userId ?? "anonymous" });
            _logger.LogError("Tracked error: {ErrorType} - {Message} (User: {UserId})", 
                errorType, message, userId ?? "anonymous");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking error: {ErrorType}", errorType);
        }
    }

    public void TrackSecurityEvent(string eventType, string details, string? userId = null)
    {
        try
        {
            IncrementCounter("security_events_total", new[] { eventType, userId ?? "anonymous" });
            _logger.LogWarning("Tracked security event: {EventType} - {Details} (User: {UserId})", 
                eventType, details, userId ?? "anonymous");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking security event: {EventType}", eventType);
        }
    }

    public HealthCheckResult CheckSystemHealth()
    {
        try
        {
            var issues = new List<string>();

            // Check CPU usage
            var cpuUsage = GetCpuUsage();
            if (cpuUsage > 90)
            {
                issues.Add($"High CPU usage: {cpuUsage:F1}%");
            }

            // Check memory usage
            var memoryUsage = GetMemoryUsage();
            if (memoryUsage > 90)
            {
                issues.Add($"High memory usage: {memoryUsage:F1}%");
            }

            // Check disk usage
            var diskUsage = GetDiskUsage();
            if (diskUsage > 90)
            {
                issues.Add($"High disk usage: {diskUsage:F1}%");
            }

            if (issues.Any())
            {
                return HealthCheckResult.Unhealthy($"System health issues: {string.Join(", ", issues)}");
            }

            return HealthCheckResult.Healthy("System is healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking system health");
            return HealthCheckResult.Unhealthy("Failed to check system health", ex);
        }
    }

    public Dictionary<string, object> GetSystemMetrics()
    {
        var metrics = new Dictionary<string, object>
        {
            ["cpu_usage_percent"] = GetCpuUsage(),
            ["memory_usage_percent"] = GetMemoryUsage(),
            ["disk_usage_percent"] = GetDiskUsage(),
            ["timestamp"] = DateTime.UtcNow
        };

        return metrics;
    }

    public void SendAlert(string alertType, string message, string severity = "warning")
    {
        try
        {
            _logger.LogWarning("ALERT - Type: {AlertType}, Severity: {Severity}, Message: {Message}", 
                alertType, severity, message);

            // Here you could integrate with external alerting systems
            // e.g., PagerDuty, Slack, Teams, Email, etc.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert: {AlertType}", alertType);
        }
    }

    private double GetCpuUsage()
    {
        try
        {
            return _cpuCounter?.NextValue() ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private double GetMemoryUsage()
    {
        try
        {
            var availableMemory = _memoryCounter?.NextValue() ?? 0;
            var totalMemory = GC.GetTotalMemory(false) / (1024 * 1024); // Convert to MB
            return totalMemory / (totalMemory + availableMemory) * 100;
        }
        catch
        {
            return 0;
        }
    }

    private double GetDiskUsage()
    {
        try
        {
            return _diskCounter?.NextValue() ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private class TimerScope : IDisposable
    {
        private readonly AdvancedMonitoringService _monitoringService;
        private readonly string _name;
        private readonly string[]? _labels;
        private readonly Stopwatch _stopwatch;

        public TimerScope(AdvancedMonitoringService monitoringService, string name, string[]? labels)
        {
            _monitoringService = monitoringService;
            _name = name;
            _labels = labels;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            var duration = _stopwatch.Elapsed.TotalSeconds;
            _monitoringService.RecordHistogram(_name, duration, _labels);
        }
    }
}
