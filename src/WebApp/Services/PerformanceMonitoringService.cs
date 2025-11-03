using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Diagnostics;

namespace WebApp.Services;

public interface IPerformanceMonitoringService
{
    void TrackPageView(string pageName, string? userId = null, Dictionary<string, string>? properties = null);
    void TrackEvent(string eventName, Dictionary<string, string>? properties = null, Dictionary<string, double>? metrics = null);
    void TrackException(Exception exception, Dictionary<string, string>? properties = null);
    void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success);
    void TrackMetric(string metricName, double value, Dictionary<string, string>? properties = null);
    void StartTimer(string timerName);
    TimeSpan StopTimer(string timerName, bool trackAsMetric = true);
    void TrackApiCall(string apiName, string endpoint, TimeSpan duration, bool success, int? statusCode = null);
    void TrackUserAction(string action, string? userId = null, Dictionary<string, string>? properties = null);
    void TrackBusinessMetric(string metricName, double value, string? category = null);
    void Flush();
}

public class PerformanceMonitoringService : IPerformanceMonitoringService
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly Dictionary<string, Stopwatch> _activeTimers;

    public PerformanceMonitoringService(TelemetryClient telemetryClient, ILogger<PerformanceMonitoringService> logger)
    {
        _telemetryClient = telemetryClient;
        _logger = logger;
        _activeTimers = new Dictionary<string, Stopwatch>();
    }

    public void TrackPageView(string pageName, string? userId = null, Dictionary<string, string>? properties = null)
    {
        try
        {
            var pageView = new PageViewTelemetry(pageName)
            {
                Timestamp = DateTimeOffset.UtcNow
            };

            if (!string.IsNullOrEmpty(userId))
            {
                pageView.Context.User.Id = userId;
            }

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    pageView.Properties[prop.Key] = prop.Value;
                }
            }

            _telemetryClient.TrackPageView(pageView);
            _logger.LogDebug("Tracked page view: {PageName}", pageName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking page view: {PageName}", pageName);
        }
    }

    public void TrackEvent(string eventName, Dictionary<string, string>? properties = null, Dictionary<string, double>? metrics = null)
    {
        try
        {
            var eventTelemetry = new EventTelemetry(eventName)
            {
                Timestamp = DateTimeOffset.UtcNow
            };

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    eventTelemetry.Properties[prop.Key] = prop.Value;
                }
            }

            if (metrics != null)
            {
                foreach (var metric in metrics)
                {
                    eventTelemetry.Metrics[metric.Key] = metric.Value;
                }
            }

            _telemetryClient.TrackEvent(eventTelemetry);
            _logger.LogDebug("Tracked event: {EventName}", eventName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking event: {EventName}", eventName);
        }
    }

    public void TrackException(Exception exception, Dictionary<string, string>? properties = null)
    {
        try
        {
            var exceptionTelemetry = new ExceptionTelemetry(exception)
            {
                Timestamp = DateTimeOffset.UtcNow
            };

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    exceptionTelemetry.Properties[prop.Key] = prop.Value;
                }
            }

            _telemetryClient.TrackException(exceptionTelemetry);
            _logger.LogError(exception, "Tracked exception: {ExceptionType}", exception.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking exception");
        }
    }

    public void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success)
    {
        try
        {
            var dependencyTelemetry = new DependencyTelemetry(dependencyName, commandName, startTime, duration, success)
            {
                Timestamp = startTime
            };

            _telemetryClient.TrackDependency(dependencyTelemetry);
            _logger.LogDebug("Tracked dependency: {DependencyName}, Duration: {Duration}ms", dependencyName, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking dependency: {DependencyName}", dependencyName);
        }
    }

    public void TrackMetric(string metricName, double value, Dictionary<string, string>? properties = null)
    {
        try
        {
            var metricTelemetry = new MetricTelemetry(metricName, value)
            {
                Timestamp = DateTimeOffset.UtcNow
            };

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    metricTelemetry.Properties[prop.Key] = prop.Value;
                }
            }

            _telemetryClient.TrackMetric(metricTelemetry);
            _logger.LogDebug("Tracked metric: {MetricName} = {Value}", metricName, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking metric: {MetricName}", metricName);
        }
    }

    public void StartTimer(string timerName)
    {
        try
        {
            if (_activeTimers.ContainsKey(timerName))
            {
                _logger.LogWarning("Timer {TimerName} is already running", timerName);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            _activeTimers[timerName] = stopwatch;
            _logger.LogDebug("Started timer: {TimerName}", timerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting timer: {TimerName}", timerName);
        }
    }

    public TimeSpan StopTimer(string timerName, bool trackAsMetric = true)
    {
        try
        {
            if (!_activeTimers.TryGetValue(timerName, out var stopwatch))
            {
                _logger.LogWarning("Timer {TimerName} not found", timerName);
                return TimeSpan.Zero;
            }

            stopwatch.Stop();
            var duration = stopwatch.Elapsed;
            _activeTimers.Remove(timerName);

            if (trackAsMetric)
            {
                TrackMetric($"Timer.{timerName}", duration.TotalMilliseconds);
            }

            _logger.LogDebug("Stopped timer: {TimerName}, Duration: {Duration}ms", timerName, duration.TotalMilliseconds);
            return duration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping timer: {TimerName}", timerName);
            return TimeSpan.Zero;
        }
    }

    public void TrackApiCall(string apiName, string endpoint, TimeSpan duration, bool success, int? statusCode = null)
    {
        try
        {
            var properties = new Dictionary<string, string>
            {
                ["ApiName"] = apiName,
                ["Endpoint"] = endpoint,
                ["Success"] = success.ToString()
            };

            if (statusCode.HasValue)
            {
                properties["StatusCode"] = statusCode.Value.ToString();
            }

            TrackEvent("ApiCall", properties, new Dictionary<string, double>
            {
                ["Duration"] = duration.TotalMilliseconds
            });

            _logger.LogDebug("Tracked API call: {ApiName} - {Endpoint}, Duration: {Duration}ms, Success: {Success}", 
                apiName, endpoint, duration.TotalMilliseconds, success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking API call: {ApiName}", apiName);
        }
    }

    public void TrackUserAction(string action, string? userId = null, Dictionary<string, string>? properties = null)
    {
        try
        {
            var eventProperties = new Dictionary<string, string>
            {
                ["Action"] = action,
                ["Timestamp"] = DateTimeOffset.UtcNow.ToString("O")
            };

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    eventProperties[prop.Key] = prop.Value;
                }
            }

            TrackEvent("UserAction", eventProperties);

            if (!string.IsNullOrEmpty(userId))
            {
                _telemetryClient.Context.User.Id = userId;
            }

            _logger.LogDebug("Tracked user action: {Action}", action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking user action: {Action}", action);
        }
    }

    public void TrackBusinessMetric(string metricName, double value, string? category = null)
    {
        try
        {
            var properties = new Dictionary<string, string>();
            
            if (!string.IsNullOrEmpty(category))
            {
                properties["Category"] = category;
            }

            properties["MetricType"] = "Business";
            properties["Timestamp"] = DateTimeOffset.UtcNow.ToString("O");

            TrackMetric($"Business.{metricName}", value, properties);
            _logger.LogDebug("Tracked business metric: {MetricName} = {Value}", metricName, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking business metric: {MetricName}", metricName);
        }
    }

    public void Flush()
    {
        try
        {
            _telemetryClient.Flush();
            _logger.LogDebug("Telemetry data flushed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flushing telemetry data");
        }
    }
}
