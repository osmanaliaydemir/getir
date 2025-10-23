using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Abstractions;

/// <summary>
/// Email gönderme servisi interface'i
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Template kullanarak email gönder
    /// </summary>
    Task<Result> SendEmailAsync(
        EmailRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Template kullanarak çoklu email gönder
    /// </summary>
    Task<Result> SendBulkEmailsAsync(
        IEnumerable<EmailRequest> requests, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// HTML içeriği ile email gönder
    /// </summary>
    Task<Result> SendHtmlEmailAsync(
        string to, 
        string subject, 
        string htmlContent,
        string? from = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Metin içeriği ile email gönder
    /// </summary>
    Task<Result> SendTextEmailAsync(
        string to, 
        string subject, 
        string textContent,
        string? from = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ekler ile email gönder
    /// </summary>
    Task<Result> SendEmailWithAttachmentsAsync(
        EmailRequest request,
        IEnumerable<EmailAttachment> attachments,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Email adresi doğrulama
    /// </summary>
    Task<Result<bool>> ValidateEmailAsync(
        string emailAddress, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Email gönderim durumu al
    /// </summary>
    Task<Result<EmailDeliveryStatus>> GetDeliveryStatusAsync(
        string messageId, 
        CancellationToken cancellationToken = default);
}
