using Getir.Domain.Entities;
using Getir.Domain.Enums;

namespace Getir.Application.DTO;

/// <summary>
/// Notification status enum for DTOs
/// </summary>
public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Failed = 3,
    Cancelled = 4,
    Scheduled = 5
}

/// <summary>
/// Log notification request
/// </summary>
public record LogNotificationRequest(
    Guid UserId,
    string Title,
    string Message,
    NotificationType Type,
    NotificationChannel Channel,
    NotificationStatus Status,
    Guid? RelatedEntityId = null,
    string? RelatedEntityType = null,
    string? ExternalMessageId = null,
    string? ErrorMessage = null,
    string? ErrorCode = null,
    int RetryCount = 0,
    DateTime? ScheduledAt = null,
    DateTime? SentAt = null,
    DateTime? DeliveredAt = null,
    DateTime? ReadAt = null,
    DateTime? FailedAt = null,
    Dictionary<string, object>? Metadata = null);

/// <summary>
/// Notification history response
/// </summary>
public record NotificationHistoryResponse(
    Guid Id,
    string Title,
    string Message,
    NotificationType Type,
    NotificationChannel Channel,
    NotificationStatus Status,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    string? ExternalMessageId,
    string? ErrorMessage,
    string? ErrorCode,
    int RetryCount,
    DateTime? ScheduledAt,
    DateTime? SentAt,
    DateTime? DeliveredAt,
    DateTime? ReadAt,
    DateTime? FailedAt,
    string? Metadata,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Notification history query
/// </summary>
public record NotificationHistoryQuery(
    NotificationType? Type = null,
    NotificationChannel? Channel = null,
    NotificationStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 20);

/// <summary>
/// Notification statistics
/// </summary>
public record NotificationStatistics(
    int TotalNotifications,
    int SentNotifications,
    int DeliveredNotifications,
    int FailedNotifications,
    int ReadNotifications,
    int EmailNotifications,
    int SmsNotifications,
    int PushNotifications,
    int InAppNotifications,
    TimeSpan? AverageDeliveryTime,
    decimal SuccessRate,
    DateTime GeneratedAt);
