using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Getir.WebApi.Hubs;

/// <summary>
/// SignalR Hub for real-time courier location tracking
/// </summary>
[Authorize]
public class CourierHub : Hub
{
    private readonly ILogger<CourierHub> _logger;

    public CourierHub(ILogger<CourierHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"courier_{userId}");
            _logger.LogInformation("Courier {UserId} connected to CourierHub", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"courier_{userId}");
            _logger.LogInformation("Courier {UserId} disconnected from CourierHub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Courier sends location update (broadcasted to customers tracking their orders)
    /// </summary>
    public async Task UpdateLocation(double latitude, double longitude, string orderId)
    {
        var userId = GetUserId();
        if (userId != null)
        {
            _logger.LogInformation("Courier {UserId} updated location for order {OrderId}: {Lat}, {Lng}", 
                userId, orderId, latitude, longitude);

            // Broadcast location to order subscribers (customers)
            await Clients.Group($"order_{orderId}")
                .SendAsync("CourierLocationUpdated", new
                {
                    orderId,
                    latitude,
                    longitude,
                    timestamp = DateTime.UtcNow
                });
        }
    }

    /// <summary>
    /// Track specific order's courier
    /// </summary>
    public async Task TrackOrder(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
        _logger.LogInformation("Customer subscribed to courier tracking for order {OrderId}", orderId);
    }

    /// <summary>
    /// Courier joins courier group for broadcasts
    /// </summary>
    public async Task JoinCourierGroup()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "all_couriers");
            _logger.LogInformation("Courier {UserId} joined courier group", userId);
        }
    }

    /// <summary>
    /// Subscribe to courier-specific notifications
    /// </summary>
    public async Task SubscribeToCourierNotifications()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"courier_notifications_{userId}");
            _logger.LogInformation("Courier {UserId} subscribed to notifications", userId);
        }
    }

    /// <summary>
    /// Get courier location history
    /// </summary>
    public async Task GetLocationHistory(string orderId)
    {
        var userId = GetUserId();
        if (userId != null)
        {
            _logger.LogInformation("Courier {UserId} requested location history for order {OrderId}", userId, orderId);
            
            // This would typically fetch location history from a service
            await Clients.Caller.SendAsync("LocationHistory", new
            {
                orderId,
                locations = new List<object>(),
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Send estimated arrival time
    /// </summary>
    public async Task SendEstimatedArrival(string orderId, int estimatedMinutes)
    {
        var userId = GetUserId();
        if (userId != null)
        {
            _logger.LogInformation("Courier {UserId} sent estimated arrival for order {OrderId}: {Minutes} minutes", 
                userId, orderId, estimatedMinutes);

            await Clients.Group($"order_{orderId}")
                .SendAsync("EstimatedArrival", new
                {
                    orderId,
                    estimatedMinutes,
                    timestamp = DateTime.UtcNow
                });
        }
    }

    private Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) 
            ? userId 
            : null;
    }
}
