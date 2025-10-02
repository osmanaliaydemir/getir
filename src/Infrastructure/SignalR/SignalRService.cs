using Getir.Application.Abstractions;
using Getir.Application.DTO;
using Microsoft.AspNetCore.SignalR;

namespace Getir.Infrastructure.SignalR;

public class SignalRService : ISignalRService
{
    private readonly ISignalRNotificationSender _notificationSender;
    private readonly ISignalROrderSender _orderSender;
    private readonly ISignalRCourierSender _courierSender;

    public SignalRService(
        ISignalRNotificationSender notificationSender,
        ISignalROrderSender orderSender,
        ISignalRCourierSender courierSender)
    {
        _notificationSender = notificationSender;
        _orderSender = orderSender;
        _courierSender = courierSender;
    }

    public async Task SendNotificationToUserAsync(Guid userId, string title, string message, string type)
    {
        await _notificationSender.SendToUserAsync(userId, title, message, type);
    }

    public async Task SendOrderStatusUpdateAsync(Guid orderId, Guid userId, string status, string message)
    {
        await _orderSender.SendStatusUpdateAsync(orderId, userId, status, message);
    }

    public async Task SendCourierLocationUpdateAsync(Guid orderId, decimal latitude, decimal longitude)
    {
        await _courierSender.SendLocationUpdateAsync(orderId, latitude, longitude);
    }

    // Enhanced Order Tracking
    public async Task SendOrderStatusUpdateEventAsync(OrderStatusUpdateEvent orderEvent)
    {
        await _orderSender.SendStatusUpdateAsync(orderEvent.OrderId, Guid.Empty, orderEvent.Status, orderEvent.Message);
    }

    public async Task SendOrderTrackingUpdateAsync(Guid orderId, OrderTrackingResponse tracking)
    {
        await _orderSender.SendStatusUpdateAsync(orderId, Guid.Empty, tracking.Status, "Order tracking updated");
    }

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
    public async Task SendCourierLocationEventAsync(CourierLocationEvent locationEvent)
    {
        await _courierSender.SendLocationUpdateAsync(Guid.Empty, locationEvent.Latitude, locationEvent.Longitude);
    }

    public async Task SendCourierTrackingUpdateAsync(Guid courierId, CourierTrackingResponse tracking)
    {
        await _courierSender.SendLocationUpdateAsync(Guid.Empty, 0, 0);
    }

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
    public async Task SendRealtimeNotificationAsync(RealtimeNotificationEvent notification)
    {
        await _notificationSender.SendToUserAsync(Guid.Empty, notification.Title, notification.Message, notification.Type);
    }

    public async Task BroadcastNotificationToGroupAsync(string groupName, RealtimeNotificationEvent notification)
    {
        await _notificationSender.SendToUserAsync(Guid.Empty, notification.Title, notification.Message, notification.Type);
    }

    public async Task SendNotificationToRoleAsync(string role, RealtimeNotificationEvent notification)
    {
        await _notificationSender.SendToUserAsync(Guid.Empty, notification.Title, notification.Message, notification.Type);
    }

    // Dashboard Updates
    public async Task SendDashboardUpdateAsync(string eventType, object data)
    {
        await _notificationSender.SendToUserAsync(Guid.Empty, "Dashboard Update", $"Dashboard updated: {eventType}", "info");
    }

    public async Task SendOrderStatsUpdateAsync(OrderRealtimeStats stats)
    {
        await SendDashboardUpdateAsync("OrderStats", stats);
    }

    public async Task SendCourierStatsUpdateAsync(CourierRealtimeStats stats)
    {
        await SendDashboardUpdateAsync("CourierStats", stats);
    }

    // Connection Management
    public async Task<ConnectionStats> GetConnectionStatsAsync()
    {
        // This would require a connection tracking service
        // For now, return basic stats
        return new ConnectionStats(
            TotalConnections: 0,
            UserConnections: 0,
            CourierConnections: 0,
            AdminConnections: 0,
            GroupSubscriptions: new Dictionary<string, int>());
    }

    public async Task<List<ConnectionInfo>> GetActiveConnectionsAsync()
    {
        // This would require a connection tracking service
        // For now, return empty list
        return new List<ConnectionInfo>();
    }

    public async Task<bool> IsUserConnectedAsync(Guid userId)
    {
        // This would require a connection tracking service
        // For now, return false
        return false;
    }

    public async Task DisconnectUserAsync(Guid userId, string reason = "Manual disconnect")
    {
        await _notificationSender.SendToUserAsync(userId, "Disconnect", reason, "system");
    }
}
