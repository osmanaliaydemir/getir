using Getir.Application.Abstractions;
using Getir.Application.DTO;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Getir.Infrastructure.SignalR;

/// <summary>
/// SignalR service implementation
/// </summary>
public class SignalRService : ISignalRService
{
    private readonly ISignalRNotificationSender _notificationSender;
    private readonly ISignalROrderSender _orderSender;
    private readonly ISignalRCourierSender _courierSender;

    /// <summary>
    /// SignalR service constructor
    /// </summary>
    /// <param name="notificationSender">Notification sender</param>
    /// <param name="orderSender">Order sender</param>
    /// <param name="courierSender">Courier sender</param>
    public SignalRService(
        ISignalRNotificationSender notificationSender,
        ISignalROrderSender orderSender,
        ISignalRCourierSender courierSender)
    {
        _notificationSender = notificationSender;
        _orderSender = orderSender;
        _courierSender = courierSender;
    }

    /// <summary>
    /// Send notification to user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="type">Notification type</param>
    public async Task SendNotificationToUserAsync(Guid userId, string title, string message, string type)
    {
        await _notificationSender.SendToUserAsync(userId, title, message, type);
    }

    /// <summary>
    /// Send order status update
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="status">Order status</param>
    /// <param name="message">Order message</param>
    public async Task SendOrderStatusUpdateAsync(Guid orderId, Guid userId, string status, string message)
    {
        await _orderSender.SendStatusUpdateAsync(orderId, userId, status, message);
    }

    /// <summary>
    /// Send courier location update
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    public async Task SendCourierLocationUpdateAsync(Guid orderId, decimal latitude, decimal longitude)
    {
        await _courierSender.SendLocationUpdateAsync(orderId, latitude, longitude);
    }

    // Enhanced Order Tracking
    /// <summary>
    /// Send order status update event
    /// </summary>
    /// <param name="orderEvent">Order event</param>
    public async Task SendOrderStatusUpdateEventAsync(OrderStatusUpdateEvent orderEvent)
    {
        await _orderSender.SendStatusUpdateAsync(orderEvent.OrderId, Guid.Empty, orderEvent.Status, orderEvent.Message);
    }

    /// <summary>
    /// Send order tracking update
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="tracking">Order tracking</param>
    public async Task SendOrderTrackingUpdateAsync(Guid orderId, OrderTrackingResponse tracking)
    {
        await _orderSender.SendStatusUpdateAsync(orderId, Guid.Empty, tracking.Status, "Order tracking updated");
    }

    /// <summary>
    /// Broadcast order status change
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="status">Order status</param>
    /// <param name="message">Order message</param>
    public async Task BroadcastOrderStatusChangeAsync(Guid orderId, string status, string message)
    {
        var statusEvent = new OrderStatusUpdateEvent(
            orderId,
            "",
            status,
            message,
            DateTime.UtcNow);

        await SendOrderStatusUpdateEventAsync(statusEvent);
    }

    // Enhanced Courier Tracking
    /// <summary>
    /// Send courier location event
    /// </summary>
    /// <param name="locationEvent">Courier location event</param>
    public async Task SendCourierLocationEventAsync(CourierLocationEvent locationEvent)
    {
        await _courierSender.SendLocationUpdateAsync(Guid.Empty, locationEvent.Latitude, locationEvent.Longitude);
    }

    /// <summary>
    /// Send courier tracking update
    /// </summary>
    /// <param name="courierId">Courier ID</param>
    /// <param name="tracking">Courier tracking</param>
    public async Task SendCourierTrackingUpdateAsync(Guid courierId, CourierTrackingResponse tracking)
    {
        await _courierSender.SendLocationUpdateAsync(Guid.Empty, 0, 0);
    }

    /// <summary>
    /// Broadcast courier location
    /// </summary>
    /// <param name="courierId">Courier ID</param>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    public async Task BroadcastCourierLocationAsync(Guid courierId, decimal latitude, decimal longitude)
    {
        var locationEvent = new CourierLocationEvent(
            courierId,
            "",
            latitude,
            longitude,
            DateTime.UtcNow);

        await SendCourierLocationEventAsync(locationEvent);
    }

    // Enhanced Notifications
    /// <summary>
    /// Send realtime notification
    /// </summary>
    /// <param name="notification">Realtime notification event</param>
    public async Task SendRealtimeNotificationAsync(RealtimeNotificationEvent notification)
    {
        await _notificationSender.SendToUserAsync(Guid.Empty, notification.Title, notification.Message, notification.Type);
    }

    /// <summary>
    /// Broadcast notification to group
    /// </summary>
    /// <param name="groupName">Group name</param>
    /// <param name="notification">Realtime notification event</param>
    public async Task BroadcastNotificationToGroupAsync(string groupName, RealtimeNotificationEvent notification)
    {
        await _notificationSender.SendToUserAsync(Guid.Empty, notification.Title, notification.Message, notification.Type);
    }

    /// <summary>
    /// Send notification to role
    /// </summary>
    /// <param name="role">Role</param>
    /// <param name="notification">Realtime notification event</param>
    public async Task SendNotificationToRoleAsync(string role, RealtimeNotificationEvent notification)
    {
        await _notificationSender.SendToUserAsync(Guid.Empty, notification.Title, notification.Message, notification.Type);
    }

    // Dashboard Updates
    /// <summary>
    /// Send dashboard update
    /// </summary>
    /// <param name="eventType">Event type</param>
    /// <param name="data">Data</param>
    public async Task SendDashboardUpdateAsync(string eventType, object data)
    {
        await _notificationSender.SendToUserAsync(Guid.Empty, "Dashboard Update", $"Dashboard updated: {eventType}", "info");
    }

    /// <summary>
    /// Send order stats update
    /// </summary>
    /// <param name="stats">Order realtime stats</param>
    public async Task SendOrderStatsUpdateAsync(OrderRealtimeStats stats)
    {
        await SendDashboardUpdateAsync("OrderStats", stats);
    }

    /// <summary>
    /// Send courier stats update
    /// </summary>
    /// <param name="stats">Courier realtime stats</param>
    public async Task SendCourierStatsUpdateAsync(CourierRealtimeStats stats)
    {
        await SendDashboardUpdateAsync("CourierStats", stats);
    }

    // Connection Management
    /// <summary>
    /// Get connection stats
    /// </summary>
    /// <returns>Connection stats</returns>
    public Task<ConnectionStats> GetConnectionStatsAsync()
    {
        // This would require a connection tracking service
        // For now, return basic stats
        return Task.FromResult(new ConnectionStats(
            TotalConnections: 0,
            UserConnections: 0,
            CourierConnections: 0,
            AdminConnections: 0,
            GroupSubscriptions: new Dictionary<string, int>()));
    }

    /// <summary>
    /// Get active connections
    /// </summary>
    /// <returns>Active connections</returns>
    public Task<List<ConnectionInfo>> GetActiveConnectionsAsync()
    {
        // This would require a connection tracking service
        // For now, return empty list
        return Task.FromResult(new List<ConnectionInfo>());
    }

    /// <summary>
    /// Check if user is connected
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if user is connected, false otherwise</returns>
    public Task<bool> IsUserConnectedAsync(Guid userId)
    {
        // This would require a connection tracking service
        // For now, return false
        return Task.FromResult(false);
    }

    /// <summary>
    /// Disconnect user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="reason">Reason for disconnect</param>
    public async Task DisconnectUserAsync(Guid userId, string reason = "Manual disconnect")
    {
        await _notificationSender.SendToUserAsync(userId, "Disconnect", reason, "system");
    }

    #region Payment Notifications
    /// <summary>
    /// Send payment status update
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="orderId">Order ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="status">Payment status</param>
    /// <param name="message">Payment message</param>
    public async Task SendPaymentStatusUpdateAsync(Guid paymentId, Guid orderId, Guid userId, PaymentStatus status, string message)
    {
        await _notificationSender.SendToUserAsync(userId, "Payment Update", message, "payment");
        
        // Order status'u da güncelle
        await _orderSender.SendStatusUpdateAsync(orderId, userId, status.ToString(), message);
    }

    /// <summary>
    /// Send payment created notification
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="orderId">Order ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="method">Payment method</param>
    /// <param name="amount">Payment amount</param>
    public async Task SendPaymentCreatedNotificationAsync(Guid paymentId, Guid orderId, Guid userId, PaymentMethod method, decimal amount)
    {
        var message = $"Payment of {amount:C} created for your order using {method.GetDisplayName()}";
        await _notificationSender.SendToUserAsync(userId, "Payment Created", message, "payment");
    }

    /// <summary>
    /// Send payment collected notification
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="orderId">Order ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="amount">Payment amount</param>
    /// <param name="courierName">Courier name</param>
    public async Task SendPaymentCollectedNotificationAsync(Guid paymentId, Guid orderId, Guid userId, decimal amount, string courierName)
    {
        var message = $"Payment of {amount:C} has been collected by courier {courierName}";
        await _notificationSender.SendToUserAsync(userId, "Payment Collected", message, "payment");
        
        // Order delivered status'a güncelle
        await _orderSender.SendStatusUpdateAsync(orderId, userId, "Delivered", message);
    }

    /// <summary>
    /// Send payment failed notification
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="orderId">Order ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="reason">Reason for payment failure</param>
    public async Task SendPaymentFailedNotificationAsync(Guid paymentId, Guid orderId, Guid userId, string reason)
    {
        var message = $"Payment failed: {reason}";
        await _notificationSender.SendToUserAsync(userId, "Payment Failed", message, "payment");
        
        // Order cancelled status'a güncelle
        await _orderSender.SendStatusUpdateAsync(orderId, userId, "Cancelled", message);
    }

    /// <summary>
    /// Send courier payment notification
    /// </summary>
    /// <param name="courierId">Courier ID</param>
    /// <param name="orderId">Order ID</param>
    /// <param name="amount">Payment amount</param>
    /// <param name="customerName">Customer name</param>
    public async Task SendCourierPaymentNotificationAsync(Guid courierId, Guid orderId, decimal amount, string customerName)
    {
        var message = $"Collect {amount:C} cash payment from {customerName} for order #{orderId}";
        await _notificationSender.SendToUserAsync(courierId, "Cash Collection Required", message, "payment");
    }

    /// <summary>
    /// Send merchant payment notification
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="orderId">Order ID</param>
    /// <param name="amount">Payment amount</param>
    /// <param name="status">Payment status</param>
    public async Task SendMerchantPaymentNotificationAsync(Guid merchantId, Guid orderId, decimal amount, string status)
    {
        var message = $"Payment of {amount:C} for order #{orderId} status: {status}";
        await _notificationSender.SendToUserAsync(merchantId, "Payment Update", message, "payment");
    }

    /// <summary>
    /// Send settlement notification
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="totalAmount">Total amount</param>
    /// <param name="netAmount">Net amount</param>
    /// <param name="status">Settlement status</param>
    public async Task SendSettlementNotificationAsync(Guid merchantId, decimal totalAmount, decimal netAmount, string status)
    {
        var message = $"Settlement {status}: Total {totalAmount:C}, Net {netAmount:C}";
        await _notificationSender.SendToUserAsync(merchantId, "Settlement Update", message, "settlement");
    }

    #endregion
}
