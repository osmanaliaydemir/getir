namespace Getir.Application.DTO;

public record CreateNotificationRequest(
    Guid UserId,
    string Title,
    string Message,
    string Type,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    string? ImageUrl,
    string? ActionUrl);

public record NotificationResponse(
    Guid Id,
    string Title,
    string Message,
    string Type,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    bool IsRead,
    string? ImageUrl,
    string? ActionUrl,
    DateTime CreatedAt);

public record MarkAsReadRequest(
    List<Guid> NotificationIds);

/// <summary>
/// Mark single notification as read
/// </summary>
public record MarkSingleNotificationAsReadRequest(
    Guid NotificationId,
    Guid UserId);

/// <summary>
/// Get user notifications request
/// </summary>
public record GetUserNotificationsRequest(
    Guid UserId,
    int Count = 20,
    bool? UnreadOnly = null);

/// <summary>
/// Delete notification request
/// </summary>
public record DeleteNotificationRequest(
    Guid NotificationId,
    Guid UserId);

/// <summary>
/// Notification list response
/// </summary>
public record NotificationListResponse(
    List<NotificationResponse> Notifications,
    int TotalCount,
    int UnreadCount);