using Getir.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Getir.Infrastructure.SignalR;

/// <summary>
/// SignalR notification sender implementation
/// </summary>
public class SignalRNotificationSender : ISignalRNotificationSender
{
    private readonly IHubContext<Hub> _hubContext;

    /// <summary>
    /// SignalR notification sender constructor
    /// </summary>
    /// <param name="hubContext">Hub context</param>
    public SignalRNotificationSender(IHubContext<Hub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Send notification to user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="type">Notification type</param>
    public async Task SendToUserAsync(Guid userId, string title, string message, string type)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("ReceiveNotification", new
            {
                title,
                message,
                type,
                timestamp = DateTime.UtcNow
            });
    }
}

/// <summary>
/// SignalR order sender implementation
/// </summary>
public class SignalROrderSender : ISignalROrderSender
{
    private readonly IHubContext<Hub> _hubContext;

    public SignalROrderSender(IHubContext<Hub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendStatusUpdateAsync(Guid orderId, Guid userId, string status, string message)
    {
        // Send to specific order subscribers
        await _hubContext.Clients.Group($"order_{orderId}")
            .SendAsync("OrderStatusChanged", new
            {
                orderId,
                status,
                message,
                timestamp = DateTime.UtcNow
            });

        // Also send to user's group
        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("OrderStatusChanged", new
            {
                orderId,
                status,
                message,
                timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Send new order to merchant
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="orderData">Order data</param>
    public async Task SendNewOrderToMerchantAsync(Guid merchantId, object orderData)
    {
        // Send to merchant group
        await _hubContext.Clients
            .Group($"merchant_{merchantId}")
            .SendAsync("NewOrderReceived", orderData);
    }

    /// <summary>
    /// Send order status changed to merchant
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="orderId">Order ID</param>
    /// <param name="orderNumber">Order number</param>
    /// <param name="status">Order status</param>
    public async Task SendOrderStatusChangedToMerchantAsync(Guid merchantId, Guid orderId, string orderNumber, string status)
    {
        // Send to merchant group
        await _hubContext.Clients
            .Group($"merchant_{merchantId}")
            .SendAsync("OrderStatusChanged", new
            {
                orderId,
                orderNumber,
                status,
                timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Send order cancelled to merchant
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="orderId">Order ID</param>
    /// <param name="orderNumber">Order number</param>
    /// <param name="reason">Order cancellation reason</param>
    public async Task SendOrderCancelledToMerchantAsync(Guid merchantId, Guid orderId, string orderNumber, string? reason)
    {
        // Send to merchant group
        await _hubContext.Clients
            .Group($"merchant_{merchantId}")
            .SendAsync("OrderCancelled", new
            {
                orderId,
                orderNumber,
                reason,
                timestamp = DateTime.UtcNow
            });
    }
}

/// <summary>
/// SignalR courier sender implementation
/// </summary>
public class SignalRCourierSender : ISignalRCourierSender
{
    private readonly IHubContext<Hub> _hubContext;

    /// <summary>
    /// SignalR courier sender constructor
    /// </summary>
    /// <param name="hubContext">Hub context</param>
    public SignalRCourierSender(IHubContext<Hub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Send location update
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    public async Task SendLocationUpdateAsync(Guid orderId, decimal latitude, decimal longitude)
    {
        await _hubContext.Clients
            .Group($"order_{orderId}")
            .SendAsync("CourierLocationUpdated", new
            {
                orderId,
                latitude,
                longitude,
                timestamp = DateTime.UtcNow
            });
    }
}
