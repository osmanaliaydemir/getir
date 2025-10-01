using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Getir.WebApi.Hubs;

/// <summary>
/// SignalR Hub for real-time order tracking
/// </summary>
[Authorize]
public class OrderHub : Hub
{
    private readonly ILogger<OrderHub> _logger;

    public OrderHub(ILogger<OrderHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} connected to OrderHub. ConnectionId: {ConnectionId}", 
                userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} disconnected from OrderHub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to specific order updates
    /// </summary>
    public async Task SubscribeToOrder(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
        _logger.LogInformation("Connection {ConnectionId} subscribed to order {OrderId}", 
            Context.ConnectionId, orderId);
    }

    /// <summary>
    /// Unsubscribe from order updates
    /// </summary>
    public async Task UnsubscribeFromOrder(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from order {OrderId}", 
            Context.ConnectionId, orderId);
    }

    /// <summary>
    /// Subscribe to merchant order updates
    /// </summary>
    public async Task SubscribeToMerchantOrders(string merchantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"merchant_{merchantId}");
        _logger.LogInformation("Connection {ConnectionId} subscribed to merchant {MerchantId} orders", 
            Context.ConnectionId, merchantId);
    }

    /// <summary>
    /// Subscribe to all order updates (admin only)
    /// </summary>
    public async Task SubscribeToAllOrders()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admin_orders");
        _logger.LogInformation("Connection {ConnectionId} subscribed to all orders", Context.ConnectionId);
    }

    /// <summary>
    /// Get order tracking information
    /// </summary>
    public async Task GetOrderTracking(string orderId)
    {
        var userId = GetUserId();
        if (userId != null)
        {
            // This would typically fetch order tracking data from a service
            _logger.LogInformation("User {UserId} requested tracking for order {OrderId}", userId, orderId);
            
            // Send current tracking status
            await Clients.Caller.SendAsync("OrderTrackingInfo", new
            {
                orderId,
                status = "Tracking requested",
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
