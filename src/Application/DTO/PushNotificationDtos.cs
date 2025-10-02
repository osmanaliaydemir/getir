namespace Getir.Application.DTO;

/// <summary>
/// Push notification request
/// </summary>
public record PushNotificationRequest(
    string DeviceToken,
    string Title,
    string Body,
    Dictionary<string, object>? Data = null,
    string? ImageUrl = null,
    string? ActionUrl = null,
    PushNotificationPriority Priority = PushNotificationPriority.Normal,
    DateTime? ScheduledAt = null);

/// <summary>
/// Push topic request
/// </summary>
public record PushTopicRequest(
    string Topic,
    string Title,
    string Body,
    Dictionary<string, object>? Data = null,
    string? ImageUrl = null,
    string? ActionUrl = null,
    PushNotificationPriority Priority = PushNotificationPriority.Normal);

/// <summary>
/// Device token request
/// </summary>
public record DeviceTokenRequest(
    Guid UserId,
    string DeviceToken,
    PushPlatform Platform,
    string? DeviceModel = null,
    string? AppVersion = null,
    bool IsActive = true);

/// <summary>
/// Push notification response
/// </summary>
public record PushNotificationResponse(
    string MessageId,
    string DeviceToken,
    string Title,
    DateTime SentAt,
    PushNotificationStatus Status);

/// <summary>
/// Push notification status
/// </summary>
public record PushNotificationStatus(
    string MessageId,
    PushStatus Status,
    DateTime? DeliveredAt,
    string? ErrorMessage,
    string? ErrorCode);

/// <summary>
/// Push status enum
/// </summary>
public enum PushStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Failed = 3,
    InvalidToken = 4
}

/// <summary>
/// Push platform enum
/// </summary>
public enum PushPlatform
{
    Android = 0,
    IOS = 1,
    Web = 2
}

/// <summary>
/// Push notification priority
/// </summary>
public enum PushNotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2
}

/// <summary>
/// Push notification template
/// </summary>
public record PushNotificationTemplate(
    string Name,
    string Title,
    string Body,
    string? ImageUrl = null,
    string? ActionUrl = null,
    Dictionary<string, object>? DefaultData = null,
    string? Description = null,
    bool IsActive = true,
    DateTime CreatedAt = default,
    DateTime? UpdatedAt = null);

/// <summary>
/// Firebase configuration
/// </summary>
public record FirebaseConfiguration(
    string ProjectId,
    string PrivateKey,
    string ClientEmail,
    string DatabaseUrl = "",
    int TimeoutSeconds = 30,
    int MaxRetryAttempts = 3);

/// <summary>
/// Device token info
/// </summary>
public record DeviceTokenInfo(
    Guid UserId,
    string DeviceToken,
    PushPlatform Platform,
    string? DeviceModel,
    string? AppVersion,
    bool IsActive,
    DateTime RegisteredAt,
    DateTime? LastUsedAt);
