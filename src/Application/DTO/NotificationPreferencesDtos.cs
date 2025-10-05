namespace Getir.Application.DTO;

/// <summary>
/// User notification preferences request
/// </summary>
public record UpdateNotificationPreferencesRequest(
    // Email preferences
    bool? EmailEnabled = null,
    bool? EmailOrderUpdates = null,
    bool? EmailPromotions = null,
    bool? EmailNewsletter = null,
    bool? EmailSecurityAlerts = null,
    
    // SMS preferences
    bool? SmsEnabled = null,
    bool? SmsOrderUpdates = null,
    bool? SmsPromotions = null,
    bool? SmsSecurityAlerts = null,
    
    // Push notification preferences
    bool? PushEnabled = null,
    bool? PushOrderUpdates = null,
    bool? PushPromotions = null,
    bool? PushMerchantUpdates = null,
    bool? PushSecurityAlerts = null,
    
    // Time preferences
    TimeSpan? QuietStartTime = null,
    TimeSpan? QuietEndTime = null,
    bool? RespectQuietHours = null,
    
    // Language preference
    string? Language = null);

/// <summary>
/// User notification preferences response
/// </summary>
public record NotificationPreferencesResponse(
    Guid Id,
    Guid UserId,
    
    // Email preferences
    bool EmailEnabled,
    bool EmailOrderUpdates,
    bool EmailPromotions,
    bool EmailNewsletter,
    bool EmailSecurityAlerts,
    
    // SMS preferences
    bool SmsEnabled,
    bool SmsOrderUpdates,
    bool SmsPromotions,
    bool SmsSecurityAlerts,
    
    // Push notification preferences
    bool PushEnabled,
    bool PushOrderUpdates,
    bool PushPromotions,
    bool PushMerchantUpdates,
    bool PushSecurityAlerts,
    
    // Time preferences
    TimeSpan? QuietStartTime,
    TimeSpan? QuietEndTime,
    bool RespectQuietHours,
    
    // Language preference
    string Language,
    
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Notification preferences summary
/// </summary>
public record NotificationPreferencesSummary(
    bool EmailEnabled,
    bool SmsEnabled,
    bool PushEnabled,
    bool RespectQuietHours,
    string Language,
    int ActiveChannelsCount);

/// <summary>
/// Bulk notification preferences update
/// </summary>
public record BulkNotificationPreferencesRequest(
    IEnumerable<Guid> UserIds,
    UpdateNotificationPreferencesRequest Preferences);

/// <summary>
/// Notification preference categories
/// </summary>
public record NotificationCategories(
    bool OrderUpdates,
    bool Promotions,
    bool Newsletter,
    bool SecurityAlerts,
    bool MerchantUpdates);

/// <summary>
/// Notification channels
/// </summary>
public enum NotificationChannel
{
    Email = 0,
    Sms = 1,
    Push = 2
}

/// <summary>
/// Notification types
/// </summary>
public enum NotificationType
{
    OrderUpdate = 0,
    Promotion = 1,
    Newsletter = 2,
    SecurityAlert = 3,
    MerchantUpdate = 4,
    SystemAnnouncement = 5
}
