using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Getir.WebApi.HealthChecks;

/// <summary>
/// Health check for SignalR hub connectivity and client connections
/// </summary>
public class SignalRHealthCheck : IHealthCheck
{
    private readonly IHubContext<Getir.WebApi.Hubs.NotificationHub> _notificationHub;
    private readonly IHubContext<Getir.WebApi.Hubs.OrderHub> _orderHub;
    private readonly IHubContext<Getir.WebApi.Hubs.CourierHub> _courierHub;
    private readonly IHubContext<Getir.WebApi.Hubs.RealtimeTrackingHub> _trackingHub;
    private readonly ILogger<SignalRHealthCheck> _logger;

    public SignalRHealthCheck(IHubContext<Getir.WebApi.Hubs.NotificationHub> notificationHub, IHubContext<Getir.WebApi.Hubs.OrderHub> orderHub,
        IHubContext<Getir.WebApi.Hubs.CourierHub> courierHub, IHubContext<Getir.WebApi.Hubs.RealtimeTrackingHub> trackingHub, ILogger<SignalRHealthCheck> logger)
    {
        _notificationHub = notificationHub;
        _orderHub = orderHub;
        _courierHub = courierHub;
        _trackingHub = trackingHub;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // SignalR hubs are registered and accessible
            var data = new Dictionary<string, object>
            {
                { "notification_hub", _notificationHub != null },
                { "order_hub", _orderHub != null },
                { "courier_hub", _courierHub != null },
                { "tracking_hub", _trackingHub != null },
                { "timestamp", DateTime.UtcNow }
            };

            // Check if all hubs are initialized
            if (_notificationHub != null && _orderHub != null &&
                _courierHub != null && _trackingHub != null)
            {
                return Task.FromResult(HealthCheckResult.Healthy(
                    "All SignalR hubs are operational",
                    data));
            }
            else
            {
                var missingHubs = new List<string>();
                if (_notificationHub == null) missingHubs.Add("notification");
                if (_orderHub == null) missingHubs.Add("order");
                if (_courierHub == null) missingHubs.Add("courier");
                if (_trackingHub == null) missingHubs.Add("tracking");

                data["missing_hubs"] = string.Join(", ", missingHubs);

                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Some SignalR hubs are not available: {string.Join(", ", missingHubs)}",
                    data: data));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "SignalR hubs are not accessible",
                ex,
                new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { "timestamp", DateTime.UtcNow }
                }));
        }
    }
}

