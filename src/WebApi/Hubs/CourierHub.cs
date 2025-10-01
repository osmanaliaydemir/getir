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

    private Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) 
            ? userId 
            : null;
    }
}
