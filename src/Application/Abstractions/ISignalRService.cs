using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Abstractions;

public interface ISignalRService
{
    // Existing methods
    Task SendNotificationToUserAsync(Guid userId, string title, string message, string type);
    Task SendOrderStatusUpdateAsync(Guid orderId, Guid userId, string status, string message);
    Task SendCourierLocationUpdateAsync(Guid orderId, decimal latitude, decimal longitude);
    
    // Enhanced Order Tracking
    Task SendOrderStatusUpdateEventAsync(OrderStatusUpdateEvent orderEvent);
    Task SendOrderTrackingUpdateAsync(Guid orderId, OrderTrackingResponse tracking);
    Task BroadcastOrderStatusChangeAsync(Guid orderId, string status, string message);
    
    // Enhanced Courier Tracking
    Task SendCourierLocationEventAsync(CourierLocationEvent locationEvent);
    Task SendCourierTrackingUpdateAsync(Guid courierId, CourierTrackingResponse tracking);
    Task BroadcastCourierLocationAsync(Guid courierId, decimal latitude, decimal longitude);
    
    // Enhanced Notifications
    Task SendRealtimeNotificationAsync(RealtimeNotificationEvent notification);
    Task BroadcastNotificationToGroupAsync(string groupName, RealtimeNotificationEvent notification);
    Task SendNotificationToRoleAsync(string role, RealtimeNotificationEvent notification);
    
    // Dashboard Updates
    Task SendDashboardUpdateAsync(string eventType, object data);
    Task SendOrderStatsUpdateAsync(OrderRealtimeStats stats);
    Task SendCourierStatsUpdateAsync(CourierRealtimeStats stats);
    
    // Connection Management
    Task<ConnectionStats> GetConnectionStatsAsync();
    Task<List<ConnectionInfo>> GetActiveConnectionsAsync();
    Task<bool> IsUserConnectedAsync(Guid userId);
    Task DisconnectUserAsync(Guid userId, string reason = "Manual disconnect");
    
    // Payment Notifications
    Task SendPaymentStatusUpdateAsync(Guid paymentId, Guid orderId, Guid userId, PaymentStatus status, string message);
    Task SendPaymentCreatedNotificationAsync(Guid paymentId, Guid orderId, Guid userId, PaymentMethod method, decimal amount);
    Task SendPaymentCollectedNotificationAsync(Guid paymentId, Guid orderId, Guid userId, decimal amount, string courierName);
    Task SendPaymentFailedNotificationAsync(Guid paymentId, Guid orderId, Guid userId, string reason);
    Task SendCourierPaymentNotificationAsync(Guid courierId, Guid orderId, decimal amount, string customerName);
    Task SendMerchantPaymentNotificationAsync(Guid merchantId, Guid orderId, decimal amount, string status);
    Task SendSettlementNotificationAsync(Guid merchantId, decimal totalAmount, decimal netAmount, string status);
}
