using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Abstractions;

/// <summary>
/// Email service interface for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email using template
    /// </summary>
    Task<Result> SendEmailAsync(
        EmailRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send bulk emails using template
    /// </summary>
    Task<Result> SendBulkEmailsAsync(
        IEnumerable<EmailRequest> requests, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send email with HTML content
    /// </summary>
    Task<Result> SendHtmlEmailAsync(
        string to, 
        string subject, 
        string htmlContent,
        string? from = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send email with plain text content
    /// </summary>
    Task<Result> SendTextEmailAsync(
        string to, 
        string subject, 
        string textContent,
        string? from = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send email with attachments
    /// </summary>
    Task<Result> SendEmailWithAttachmentsAsync(
        EmailRequest request,
        IEnumerable<EmailAttachment> attachments,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate email address
    /// </summary>
    Task<Result<bool>> ValidateEmailAsync(
        string emailAddress, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get email delivery status
    /// </summary>
    Task<Result<EmailDeliveryStatus>> GetDeliveryStatusAsync(
        string messageId, 
        CancellationToken cancellationToken = default);
}
