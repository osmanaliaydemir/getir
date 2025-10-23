using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace Getir.Infrastructure.Services.Notifications;

/// <summary>
/// Email service implementation using SMTP
/// Supports multiple email providers: Gmail, Outlook, SendGrid, etc.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILoggingService _loggingService;
    private readonly EmailConfiguration _emailConfig;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        ILoggingService loggingService,
        IOptions<EmailConfiguration> emailConfig,
        ILogger<EmailService> logger)
    {
        _loggingService = loggingService;
        _emailConfig = emailConfig.Value;
        _logger = logger;
    }

    /// <summary>
    /// Email gönder
    /// </summary>
    /// <param name="request">Email isteği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Email gönderim sonucu</returns>
    public async Task<Result> SendEmailAsync(
        EmailRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate email address
            var validationResult = await ValidateEmailAsync(request.To, cancellationToken);
            if (!validationResult.Success || !validationResult.Value)
            {
                return Result.Fail("Invalid email address format", "INVALID_EMAIL_ADDRESS");
            }

            using var smtpClient = CreateSmtpClient();
            using var mailMessage = CreateMailMessage(request);

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            // Log email sending
            _loggingService.LogBusinessEvent("EmailSent", new
            {
                To = MaskEmailAddress(request.To),
                Subject = request.Subject,
                IsHtml = request.IsHtml,
                Success = true
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {EmailAddress}", MaskEmailAddress(request.To));
            _loggingService.LogError("Email sending failed", ex, new { request.To, request.Subject });
            return Result.Fail("Failed to send email", "EMAIL_SEND_ERROR");
        }
    }

    /// <summary>
    /// Birden fazla email gönder
    /// </summary>
    /// <param name="requests">Email istekleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Birden fazla email gönderim sonucu</returns>
    public async Task<Result> SendBulkEmailsAsync(
        IEnumerable<EmailRequest> requests, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var results = new List<Result>();
            var batchSize = 50; // Process in smaller batches for emails
            var requestsList = requests.ToList();
            var batches = requestsList.Chunk(batchSize);

            foreach (var batch in batches)
            {
                var batchTasks = batch.Select(request => SendEmailAsync(request, cancellationToken));
                var batchResults = await Task.WhenAll(batchTasks);
                results.AddRange(batchResults);

                // Add delay between batches to respect rate limits
                if (batches.Count() > 1)
                {
                    await Task.Delay(2000, cancellationToken);
                }
            }

            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count - successCount;

            _loggingService.LogBusinessEvent("BulkEmailSent", new
            {
                TotalCount = results.Count,
                SuccessCount = successCount,
                FailureCount = failureCount
            });

            if (failureCount == 0)
            {
                return Result.Ok();
            }

            return Result.Fail($"Bulk email completed with {failureCount} failures", "BULK_EMAIL_PARTIAL_FAILURE");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk emails");
            _loggingService.LogError("Bulk email sending failed", ex);
            return Result.Fail("Failed to send bulk emails", "BULK_EMAIL_ERROR");
        }
    }

    /// <summary>
    /// HTML içeriği ile email gönder
    /// </summary>
    /// <param name="to">Alıcı email adresi</param>
    /// <param name="subject">Email konusu</param>
    /// <param name="htmlContent">HTML içeriği</param>
    /// <param name="from">Gönderen email adresi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>HTML içeriği ile email gönderim sonucu</returns>
    public async Task<Result> SendHtmlEmailAsync(
        string to, 
        string subject, 
        string htmlContent,
        string? from = null,
        CancellationToken cancellationToken = default)
    {
        var request = new EmailRequest(
            to,
            subject,
            htmlContent,
            from ?? _emailConfig.FromEmail,
            null,
            true);

        return await SendEmailAsync(request, cancellationToken);
    }

    /// <summary>
    /// Metin içeriği ile email gönder
    /// </summary>
    /// <param name="to">Alıcı email adresi</param>
    /// <param name="subject">Email konusu</param>
    /// <param name="textContent">Metin içeriği</param>
    /// <param name="from">Gönderen email adresi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Metin içeriği ile email gönderim sonucu</returns>
    public async Task<Result> SendTextEmailAsync(
        string to, 
        string subject, 
        string textContent,
        string? from = null,
        CancellationToken cancellationToken = default)
    {
        var request = new EmailRequest(
            to,
            subject,
            textContent,
            from ?? _emailConfig.FromEmail,
            null,
            false);

        return await SendEmailAsync(request, cancellationToken);
    }

    /// <summary>
    /// Ekler ile email gönder
    /// </summary>
    /// <param name="request">Email isteği</param>
    /// <param name="attachments">Ekler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Ekler ile email gönderim sonucu</returns>
    public async Task<Result> SendEmailWithAttachmentsAsync(
        EmailRequest request,
        IEnumerable<EmailAttachment> attachments,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var smtpClient = CreateSmtpClient();
            using var mailMessage = CreateMailMessage(request);

            // Add attachments
            foreach (var attachment in attachments)
            {
                var mailAttachment = new Attachment(
                    new MemoryStream(attachment.Content), 
                    attachment.FileName, 
                    attachment.ContentType);

                if (!string.IsNullOrEmpty(attachment.ContentId))
                {
                    mailAttachment.ContentId = attachment.ContentId;
                }

                mailMessage.Attachments.Add(mailAttachment);
            }

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            _loggingService.LogBusinessEvent("EmailWithAttachmentsSent", new
            {
                To = MaskEmailAddress(request.To),
                Subject = request.Subject,
                AttachmentCount = attachments.Count(),
                Success = true
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email with attachments to {EmailAddress}", MaskEmailAddress(request.To));
            _loggingService.LogError("Email with attachments sending failed", ex, new { request.To, request.Subject });
            return Result.Fail("Failed to send email with attachments", "EMAIL_ATTACHMENTS_SEND_ERROR");
        }
    }

    /// <summary>
    /// Email adresi doğrula
    /// </summary>
    /// <param name="emailAddress">Email adresi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Email adresi doğrulama sonucu</returns>
    public Task<Result<bool>> ValidateEmailAsync(
        string emailAddress, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                return Task.FromResult(Result.Fail<bool>("Email address is required", "EMAIL_ADDRESS_REQUIRED"));
            }

            // Basic email validation
            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            var isValid = System.Text.RegularExpressions.Regex.IsMatch(emailAddress, emailPattern);

            return Task.FromResult(Result.Ok(isValid));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating email address");
            return Task.FromResult(Result.Fail<bool>("Failed to validate email address", "EMAIL_VALIDATION_ERROR"));
        }
    }

    /// <summary>
    /// Email teslimat durumunu getir
    /// </summary>
    /// <param name="messageId">Mesaj ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Email teslimat durumunu</returns>
    public Task<Result<EmailDeliveryStatus>> GetDeliveryStatusAsync(
        string messageId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would check with the email provider
            // For now, return a mock status
            var status = new EmailDeliveryStatus(
                messageId,
                EmailStatus.Delivered,
                DateTime.UtcNow,
                null,
                0);

            return Task.FromResult(Result.Ok(status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email delivery status for message {MessageId}", messageId);
            return Task.FromResult(Result.Fail<EmailDeliveryStatus>("Failed to get delivery status", "DELIVERY_STATUS_ERROR"));
        }
    }

    #region Helper Methods

    /// <summary>
    /// SMTP client oluştur
    /// </summary>
    /// <returns>SMTP client</returns>
    private SmtpClient CreateSmtpClient()
    {
        var smtpClient = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.SmtpPort)
        {
            EnableSsl = _emailConfig.UseSsl,
            Timeout = _emailConfig.TimeoutSeconds * 1000,
            Credentials = new NetworkCredential(_emailConfig.Username, _emailConfig.Password)
        };

        return smtpClient;
    }

    /// <summary>
    /// Mail mesajı oluştur
    /// </summary>
    /// <param name="request">Email isteği</param>
    /// <returns>Mail mesajı</returns>
    private MailMessage CreateMailMessage(EmailRequest request)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(request.From ?? _emailConfig.FromEmail, _emailConfig.FromName),
            Subject = request.Subject,
            Body = request.Content,
            IsBodyHtml = request.IsHtml,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };

        mailMessage.To.Add(request.To);

        if (!string.IsNullOrEmpty(request.ReplyTo))
        {
            mailMessage.ReplyToList.Add(request.ReplyTo);
        }

        // Add custom headers
        if (request.Headers != null)
        {
            foreach (var header in request.Headers)
            {
                mailMessage.Headers.Add(header.Key, header.Value);
            }
        }

        return mailMessage;
    }

    /// <summary>
    /// Email adresini maskele
    /// </summary>
    /// <param name="emailAddress">Email adresi</param>
    /// <returns>Maskeleli email adresi</returns>
    private string MaskEmailAddress(string emailAddress)
    {
        if (string.IsNullOrEmpty(emailAddress) || !emailAddress.Contains('@'))
            return "***@***.***";

        var parts = emailAddress.Split('@');
        var username = parts[0];
        var domain = parts[1];

        var maskedUsername = username.Length > 2 
            ? username.Substring(0, 2) + "***" 
            : "***";

        return $"{maskedUsername}@{domain}";
    }

    #endregion
}
