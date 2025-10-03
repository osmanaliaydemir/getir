using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class NotificationLog
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = default!; // Sent, Delivered, Read, Clicked, Failed
    public string? ErrorMessage { get; set; }
    public string? ProviderResponse { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual Notification Notification { get; set; } = default!;
    public virtual User User { get; set; } = default!;
}
