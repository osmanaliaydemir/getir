using Getir.Application.Common;

namespace Getir.Application.DTO;

// Real-time Order Tracking DTOs
public record OrderStatusUpdateEvent(
    Guid OrderId,
    string OrderNumber,
    string Status,
    string Message,
    DateTime Timestamp,
    OrderLocationEvent? Location = null);

public record OrderLocationEvent(
    decimal Latitude,
    decimal Longitude,
    string? Address = null,
    DateTime Timestamp = default);

public record OrderTrackingResponse(
    Guid OrderId,
    string OrderNumber,
    string Status,
    List<OrderStatusEvent> StatusHistory,
    OrderLocationEvent? CurrentLocation,
    string? CourierName,
    string? CourierPhone,
    int EstimatedMinutes);

public record OrderStatusEvent(
    string Status,
    string Message,
    DateTime Timestamp);

// Real-time Courier Tracking DTOs
public record CourierLocationEvent(
    Guid CourierId,
    string CourierName,
    decimal Latitude,
    decimal Longitude,
    DateTime Timestamp,
    string? VehicleType = null);

public record CourierTrackingResponse(
    Guid CourierId,
    string CourierName,
    string VehicleType,
    List<CourierLocationEvent> LocationHistory,
    bool IsActive,
    int EstimatedArrivalMinutes);

// Real-time Notification DTOs
public record RealtimeNotificationEvent(
    Guid NotificationId,
    string Title,
    string Message,
    string Type,
    DateTime Timestamp,
    bool IsRead,
    Dictionary<string, object>? Data = null);

public record NotificationSubscriptionRequest(
    List<string> NotificationTypes);

public record NotificationSubscriptionResponse(
    List<string> SubscribedTypes,
    DateTime SubscribedAt);

// Real-time Dashboard DTOs
public record RealtimeDashboardEvent(
    string EventType,
    object Data,
    DateTime Timestamp);

public record OrderRealtimeStats(
    int ActiveOrders,
    int CompletedToday,
    decimal TodayRevenue,
    List<OrderStatusUpdateEvent> RecentUpdates);

public record CourierRealtimeStats(
    int ActiveCouriers,
    int AvailableCouriers,
    List<CourierLocationEvent> RecentLocations);

// SignalR Connection Management DTOs
public record ConnectionInfo(
    string ConnectionId,
    Guid? UserId,
    string? UserRole,
    DateTime ConnectedAt,
    List<string> SubscribedGroups);

public record ConnectionStats(
    int TotalConnections,
    int UserConnections,
    int CourierConnections,
    int AdminConnections,
    Dictionary<string, int> GroupSubscriptions);
