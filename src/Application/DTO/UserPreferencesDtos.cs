namespace Getir.Application.DTO;

/// <summary>
/// User notification preferences response
/// </summary>
public record UserNotificationPreferencesResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    
    // Email preferences
    public bool EmailEnabled { get; init; }
    public bool EmailOrderUpdates { get; init; }
    public bool EmailPromotions { get; init; }
    public bool EmailNewsletter { get; init; }
    public bool EmailSecurityAlerts { get; init; }
    
    // SMS preferences
    public bool SmsEnabled { get; init; }
    public bool SmsOrderUpdates { get; init; }
    public bool SmsPromotions { get; init; }
    public bool SmsSecurityAlerts { get; init; }
    
    // Push notification preferences
    public bool PushEnabled { get; init; }
    public bool PushOrderUpdates { get; init; }
    public bool PushPromotions { get; init; }
    public bool PushMerchantUpdates { get; init; }
    public bool PushSecurityAlerts { get; init; }
    
    // Merchant Portal - Sound & Desktop Notifications
    public bool SoundEnabled { get; init; }
    public bool DesktopNotifications { get; init; }
    public string NotificationSound { get; init; } = "default";
    
    // Merchant Portal - Event-specific Notifications
    public bool NewOrderNotifications { get; init; }
    public bool StatusChangeNotifications { get; init; }
    public bool CancellationNotifications { get; init; }
    
    // Time preferences (Do Not Disturb)
    public TimeSpan? QuietStartTime { get; init; }
    public TimeSpan? QuietEndTime { get; init; }
    public bool RespectQuietHours { get; init; }
    
    // Language preference
    public string Language { get; init; } = "tr-TR";
    
    // Timestamps
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Update user notification preferences request
/// </summary>
public record UpdateUserNotificationPreferencesRequest
{
    // Email preferences
    public bool? EmailEnabled { get; init; }
    public bool? EmailOrderUpdates { get; init; }
    public bool? EmailPromotions { get; init; }
    public bool? EmailNewsletter { get; init; }
    public bool? EmailSecurityAlerts { get; init; }
    
    // SMS preferences
    public bool? SmsEnabled { get; init; }
    public bool? SmsOrderUpdates { get; init; }
    public bool? SmsPromotions { get; init; }
    public bool? SmsSecurityAlerts { get; init; }
    
    // Push notification preferences
    public bool? PushEnabled { get; init; }
    public bool? PushOrderUpdates { get; init; }
    public bool? PushPromotions { get; init; }
    public bool? PushMerchantUpdates { get; init; }
    public bool? PushSecurityAlerts { get; init; }
    
    // Merchant Portal - Sound & Desktop Notifications
    public bool? SoundEnabled { get; init; }
    public bool? DesktopNotifications { get; init; }
    public string? NotificationSound { get; init; }
    
    // Merchant Portal - Event-specific Notifications
    public bool? NewOrderNotifications { get; init; }
    public bool? StatusChangeNotifications { get; init; }
    public bool? CancellationNotifications { get; init; }
    
    // Time preferences (Do Not Disturb)
    public TimeSpan? QuietStartTime { get; init; }
    public TimeSpan? QuietEndTime { get; init; }
    public bool? RespectQuietHours { get; init; }
    
    // Language preference
    public string? Language { get; init; }
}

/// <summary>
/// Simplified preferences for Merchant Portal (UI)
/// </summary>
public record MerchantNotificationPreferencesResponse
{
    public bool SoundEnabled { get; init; }
    public bool DesktopNotifications { get; init; }
    public bool EmailNotifications { get; init; }
    public bool NewOrderNotifications { get; init; }
    public bool StatusChangeNotifications { get; init; }
    public bool CancellationNotifications { get; init; }
    public bool DoNotDisturbEnabled { get; init; }
    public string? DoNotDisturbStart { get; init; } // HH:mm format
    public string? DoNotDisturbEnd { get; init; }   // HH:mm format
    public string NotificationSound { get; init; } = "default";
}

/// <summary>
/// Simplified preferences update for Merchant Portal (UI)
/// </summary>
public record UpdateMerchantNotificationPreferencesRequest
{
    public bool SoundEnabled { get; init; }
    public bool DesktopNotifications { get; init; }
    public bool EmailNotifications { get; init; }
    public bool NewOrderNotifications { get; init; }
    public bool StatusChangeNotifications { get; init; }
    public bool CancellationNotifications { get; init; }
    public bool DoNotDisturbEnabled { get; init; }
    public string? DoNotDisturbStart { get; init; } // HH:mm format
    public string? DoNotDisturbEnd { get; init; }   // HH:mm format
    public string NotificationSound { get; init; } = "default";
}
