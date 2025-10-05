using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Notification history entity for tracking all notification activities
/// </summary>
public class NotificationHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public NotificationType Type { get; set; }
    public NotificationChannel Channel { get; set; }
    public NotificationStatus Status { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? ExternalMessageId { get; set; } // Provider message ID
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public int RetryCount { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? Metadata { get; set; } // JSON metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = default!;
}


/// <summary>
/// Notification channel enum
/// </summary>
public enum NotificationChannel
{
    Email = 0,
    Sms = 1,
    Push = 2,
    InApp = 3
}

/// <summary>
/// Notification status enum
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
