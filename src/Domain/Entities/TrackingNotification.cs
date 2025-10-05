using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class TrackingNotification
{
    public Guid Id { get; set; }
    public Guid OrderTrackingId { get; set; }
    public Guid? UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? Data { get; set; } // JSON data for additional information
    public bool IsSent { get; set; } = false;
    public bool IsRead { get; set; } = false;
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? DeliveryMethod { get; set; } // push, sms, email
    public string? DeliveryStatus { get; set; } // sent, delivered, failed
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual OrderTracking? OrderTracking { get; set; }
    public virtual User? User { get; set; }
}
