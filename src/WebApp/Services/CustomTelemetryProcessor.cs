using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace WebApp.Services;

public class CustomTelemetryProcessor : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;
    private readonly ILogger<CustomTelemetryProcessor> _logger;

    public CustomTelemetryProcessor(ITelemetryProcessor next, ILogger<CustomTelemetryProcessor> logger)
    {
        _next = next;
        _logger = logger;
    }

    public void Process(ITelemetry item)
    {
        try
        {
            // Filter out health check requests
            if (item is RequestTelemetry request)
            {
                if (request.Url?.ToString().Contains("/health", StringComparison.OrdinalIgnoreCase) == true)
                {
                    _logger.LogDebug("Filtering out health check request from telemetry");
                    return;
                }

                // Add custom properties
                request.Properties["Application"] = "Getir-WebApp";
                request.Properties["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
                
                // Add user context if available
                if (!string.IsNullOrEmpty(request.Context.User.Id))
                {
                    request.Properties["UserId"] = request.Context.User.Id;
                }

                // Add session context
                if (!string.IsNullOrEmpty(request.Context.Session.Id))
                {
                    request.Properties["SessionId"] = request.Context.Session.Id;
                }
            }

            // Filter out dependency calls to health checks
            if (item is DependencyTelemetry dependency)
            {
                if (dependency.Name?.Contains("health", StringComparison.OrdinalIgnoreCase) == true)
                {
                    _logger.LogDebug("Filtering out health check dependency from telemetry");
                    return;
                }

                // Add custom properties to dependencies
                dependency.Properties["Application"] = "Getir-WebApp";
                dependency.Properties["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            }

            // Filter out excessive logging
            if (item is TraceTelemetry trace)
            {
                // Filter out debug level traces in production
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" && 
                    trace.SeverityLevel == SeverityLevel.Verbose)
                {
                    return;
                }

                // Add custom properties
                trace.Properties["Application"] = "Getir-WebApp";
                trace.Properties["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            }

            // Filter out excessive exceptions
            if (item is ExceptionTelemetry exception)
            {
                // Add custom properties
                exception.Properties["Application"] = "Getir-WebApp";
                exception.Properties["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
                
                // Add additional context
                if (exception.Context.User != null && !string.IsNullOrEmpty(exception.Context.User.Id))
                {
                    exception.Properties["UserId"] = exception.Context.User.Id;
                }
            }

            // Filter out excessive metrics
            if (item is MetricTelemetry metric)
            {
                // Add custom properties
                metric.Properties["Application"] = "Getir-WebApp";
                metric.Properties["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            }

            // Process the telemetry item
            _next.Process(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing telemetry item");
            // Continue processing even if there's an error
            _next.Process(item);
        }
    }
}
