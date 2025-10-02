namespace Getir.Application.DTO;

/// <summary>
/// SMS request
/// </summary>
public record SmsRequest(
    string PhoneNumber,
    string Message,
    string? SenderId = null,
    DateTime? ScheduledAt = null,
    Dictionary<string, string>? Metadata = null);

/// <summary>
/// SMS template request
/// </summary>
public record SmsTemplateRequest(
    string PhoneNumber,
    string TemplateName,
    Dictionary<string, string> TemplateData,
    string? SenderId = null,
    DateTime? ScheduledAt = null);

/// <summary>
/// SMS response
/// </summary>
public record SmsResponse(
    string MessageId,
    string PhoneNumber,
    string Message,
    DateTime SentAt,
    SmsDeliveryStatus Status);

/// <summary>
/// SMS delivery status
/// </summary>
public record SmsDeliveryStatus(
    string MessageId,
    SmsStatus Status,
    DateTime? DeliveredAt,
    string? ErrorMessage,
    string? ErrorCode,
    int RetryCount);

/// <summary>
/// SMS status enum
/// </summary>
public enum SmsStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Failed = 3,
    Expired = 4,
    Rejected = 5
}

/// <summary>
/// SMS template
/// </summary>
public record SmsTemplate(
    string Name,
    string Content,
    string? Description = null,
    bool IsActive = true,
    DateTime CreatedAt = default,
    DateTime? UpdatedAt = null);

/// <summary>
/// SMS configuration
/// </summary>
public record SmsConfiguration(
    string ProviderName,
    string ApiKey,
    string ApiSecret,
    string? SenderId = null,
    string BaseUrl = "",
    int TimeoutSeconds = 30,
    int MaxRetryAttempts = 3);

/// <summary>
/// SMS balance
/// </summary>
public record SmsBalance(
    decimal AvailableCredits,
    decimal UsedCredits,
    int AvailableSms,
    DateTime LastUpdated);

/// <summary>
/// Bulk SMS request
/// </summary>
public record BulkSmsRequest(
    IEnumerable<string> PhoneNumbers,
    string Message,
    string? SenderId = null,
    DateTime? ScheduledAt = null,
    int BatchSize = 100,
    int DelayBetweenBatches = 1000);
