namespace Getir.Domain.Entities;

/// <summary>
/// User notification preferences entity
/// </summary>
public class UserNotificationPreferences
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    // Email preferences
    public bool EmailEnabled { get; set; } = true;
    public bool EmailOrderUpdates { get; set; } = true;
    public bool EmailPromotions { get; set; } = true;
    public bool EmailNewsletter { get; set; } = true;
    public bool EmailSecurityAlerts { get; set; } = true;
    
    // SMS preferences
    public bool SmsEnabled { get; set; } = true;
    public bool SmsOrderUpdates { get; set; } = true;
    public bool SmsPromotions { get; set; } = false;
    public bool SmsSecurityAlerts { get; set; } = true;
    
    // Push notification preferences
    public bool PushEnabled { get; set; } = true;
    public bool PushOrderUpdates { get; set; } = true;
    public bool PushPromotions { get; set; } = true;
    public bool PushMerchantUpdates { get; set; } = true;
    public bool PushSecurityAlerts { get; set; } = true;
    
    // Time preferences
    public TimeSpan? QuietStartTime { get; set; } // e.g., 22:00
    public TimeSpan? QuietEndTime { get; set; }   // e.g., 08:00
    public bool RespectQuietHours { get; set; } = true;
    
    // Language preference
    public string Language { get; set; } = "tr-TR";
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual User User { get; set; } = default!;
}
