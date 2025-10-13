using Getir.Application.Common;

namespace Getir.Application.DTO;

/// <summary>
/// Email request
/// </summary>
public record EmailRequest(
    string To,
    string Subject,
    string Content,
    string? From = null,
    string? ReplyTo = null,
    bool IsHtml = true,
    Dictionary<string, string>? Headers = null);

/// <summary>
/// Email attachment
/// </summary>
public record EmailAttachment(
    string FileName,
    byte[] Content,
    string ContentType,
    string? ContentId = null,
    bool IsInline = false);

/// <summary>
/// Email template request
/// </summary>
public record EmailTemplateRequest(
    string To,
    string TemplateName,
    Dictionary<string, object> TemplateData,
    string? From = null,
    string? Subject = null);

/// <summary>
/// Email response
/// </summary>
public record EmailResponse(
    string MessageId,
    string To,
    string Subject,
    DateTime SentAt,
    EmailDeliveryStatus Status);

/// <summary>
/// Email delivery status
/// </summary>
public record EmailDeliveryStatus(
    string MessageId,
    EmailStatus Status,
    DateTime? DeliveredAt,
    string? ErrorMessage,
    int RetryCount);

/// <summary>
/// Email status enum
/// </summary>
public enum EmailStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Failed = 3,
    Bounced = 4,
    Opened = 5,
    Clicked = 6
}

/// <summary>
/// Email template
/// </summary>
public record EmailTemplate(
    string Name,
    string Subject,
    string HtmlContent,
    string? TextContent = null,
    string? Description = null,
    bool IsActive = true,
    DateTime CreatedAt = default,
    DateTime? UpdatedAt = null);

/// <summary>
/// Email configuration
/// </summary>
public class EmailConfiguration
{
    public string SmtpServer { get; set; } = default!;
    public int SmtpPort { get; set; }
    public bool UseSsl { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FromEmail { get; set; } = default!;
    public string FromName { get; set; } = default!;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
}

/// <summary>
/// Bulk email request
/// </summary>
public record BulkEmailRequest(
    IEnumerable<string> Recipients,
    string Subject,
    string Content,
    bool IsHtml = true,
    string? From = null,
    int BatchSize = 100,
    int DelayBetweenBatches = 1000);
