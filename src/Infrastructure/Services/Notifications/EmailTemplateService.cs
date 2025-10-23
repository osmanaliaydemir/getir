using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Getir.Infrastructure.Services.Notifications;

/// <summary>
/// Email template service for managing and rendering email templates
/// </summary>
public class EmailTemplateService : IEmailTemplateService
{
    private readonly ILoggingService _loggingService;
    private readonly ILogger<EmailTemplateService> _logger;
    private readonly Dictionary<string, EmailTemplate> _templates;

    public EmailTemplateService(
        ILoggingService loggingService,
        ILogger<EmailTemplateService> logger)
    {
        _loggingService = loggingService;
        _logger = logger;
        _templates = InitializeTemplates();
    }

    /// <summary>
    /// Email template'i getir
    /// </summary>
    /// <param name="templateName">Template adı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Email template'i</returns>
    public Task<Result<EmailTemplate>> GetTemplateAsync(
        string templateName, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_templates.TryGetValue(templateName, out var template))
            {
                return Task.FromResult(Result.Ok(template));
            }

            return Task.FromResult(Result.Fail<EmailTemplate>($"Template '{templateName}' not found", "TEMPLATE_NOT_FOUND"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email template {TemplateName}", templateName);
            _loggingService.LogError("Get email template failed", ex, new { templateName });
            return Task.FromResult(Result.Fail<EmailTemplate>("Failed to get email template", "GET_TEMPLATE_ERROR"));
        }
    }

    /// <summary>
    /// Email template'i içeriğini renderle
    /// </summary>
    /// <param name="templateName">Template adı</param>
    /// <param name="templateData">Template verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Renderlenmiş template içeriği</returns>
    public async Task<Result<string>> RenderTemplateAsync(
        string templateName, 
        Dictionary<string, object> templateData, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var templateResult = await GetTemplateAsync(templateName, cancellationToken);
            if (!templateResult.Success)
            {
                return Result.Fail<string>(templateResult.Error ?? "Template not found", templateResult.ErrorCode ?? "TEMPLATE_NOT_FOUND");
            }

            var template = templateResult.Value!;
            var renderedContent = RenderTemplateContent(template.HtmlContent, templateData);

            _loggingService.LogBusinessEvent("EmailTemplateRendered", new
            {
                TemplateName = templateName,
                DataKeys = templateData.Keys.ToList()
            });

            return Result.Ok(renderedContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering email template {TemplateName}", templateName);
            _loggingService.LogError("Render email template failed", ex, new { templateName });
            return Result.Fail<string>("Failed to render email template", "RENDER_TEMPLATE_ERROR");
        }
    }

    /// <summary>
    /// Email isteği oluştur
    /// </summary>
    /// <param name="request">Email isteği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Email isteği</returns>
    public async Task<Result<EmailRequest>> CreateEmailFromTemplateAsync(
        EmailTemplateRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var templateResult = await GetTemplateAsync(request.TemplateName, cancellationToken);
            if (!templateResult.Success)
            {
                return Result.Fail<EmailRequest>(templateResult.Error ?? "Template not found", templateResult.ErrorCode ?? "TEMPLATE_NOT_FOUND");
            }

            var template = templateResult.Value!;
            var renderedSubject = RenderTemplateContent(template.Subject, request.TemplateData);
            var renderedContent = RenderTemplateContent(template.HtmlContent, request.TemplateData);

            var emailRequest = new EmailRequest(
                request.To,
                request.Subject ?? renderedSubject,
                renderedContent,
                request.From,
                null,
                true);

            _loggingService.LogBusinessEvent("EmailCreatedFromTemplate", new
            {
                TemplateName = request.TemplateName,
                To = MaskEmailAddress(request.To),
                Subject = renderedSubject
            });

            return Result.Ok(emailRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email from template {TemplateName}", request.TemplateName);
            _loggingService.LogError("Create email from template failed", ex, new { request.TemplateName });
            return Result.Fail<EmailRequest>("Failed to create email from template", "CREATE_EMAIL_FROM_TEMPLATE_ERROR");
        }
    }

    /// <summary>
    /// Tüm email template'lerini getir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Tüm email template'leri</returns>
    public Task<Result<IEnumerable<EmailTemplate>>> GetAllTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var templates = _templates.Values.Where(t => t.IsActive).ToList();
            return Task.FromResult(Result.Ok<IEnumerable<EmailTemplate>>(templates));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all email templates");
            _loggingService.LogError("Get all email templates failed", ex);
            return Task.FromResult(Result.Fail<IEnumerable<EmailTemplate>>("Failed to get all email templates", "GET_ALL_TEMPLATES_ERROR"));
        }
    }

    #region Helper Methods

    /// <summary>
    /// Email template'lerini initialize et
    /// </summary>
    /// <returns>Email template'leri</returns>
    private Dictionary<string, EmailTemplate> InitializeTemplates()
    {
        return new Dictionary<string, EmailTemplate>
        {
            ["welcome"] = new EmailTemplate(
                "welcome",
                "Getir'e Hoş Geldiniz!",
                GetWelcomeTemplate(),
                null,
                "Yeni kullanıcı karşılama emaili",
                true,
                DateTime.UtcNow,
                null),

            ["order_confirmation"] = new EmailTemplate(
                "order_confirmation",
                "Siparişiniz Onaylandı - {OrderNumber}",
                GetOrderConfirmationTemplate(),
                null,
                "Sipariş onay emaili",
                true,
                DateTime.UtcNow,
                null),

            ["order_ready"] = new EmailTemplate(
                "order_ready",
                "Siparişiniz Hazır - {OrderNumber}",
                GetOrderReadyTemplate(),
                null,
                "Sipariş hazır emaili",
                true,
                DateTime.UtcNow,
                null),

            ["order_delivered"] = new EmailTemplate(
                "order_delivered",
                "Siparişiniz Teslim Edildi - {OrderNumber}",
                GetOrderDeliveredTemplate(),
                null,
                "Sipariş teslim emaili",
                true,
                DateTime.UtcNow,
                null),

            ["order_cancelled"] = new EmailTemplate(
                "order_cancelled",
                "Siparişiniz İptal Edildi - {OrderNumber}",
                GetOrderCancelledTemplate(),
                null,
                "Sipariş iptal emaili",
                true,
                DateTime.UtcNow,
                null),

            ["password_reset"] = new EmailTemplate(
                "password_reset",
                "Şifre Sıfırlama - Getir",
                GetPasswordResetTemplate(),
                null,
                "Şifre sıfırlama emaili",
                true,
                DateTime.UtcNow,
                null),

            ["email_verification"] = new EmailTemplate(
                "email_verification",
                "Email Adresinizi Doğrulayın - Getir",
                GetEmailVerificationTemplate(),
                null,
                "Email doğrulama emaili",
                true,
                DateTime.UtcNow,
                null),

            ["promotion"] = new EmailTemplate(
                "promotion",
                "{PromotionTitle} - Getir",
                GetPromotionTemplate(),
                null,
                "Promosyon emaili",
                true,
                DateTime.UtcNow,
                null),

            ["newsletter"] = new EmailTemplate(
                "newsletter",
                "Getir Bülteni - {NewsletterTitle}",
                GetNewsletterTemplate(),
                null,
                "Haber bülteni emaili",
                true,
                DateTime.UtcNow,
                null)
        };
    }

    /// <summary>
    /// Email template'i içeriğini renderle
    /// </summary>
    /// <param name="template">Template içeriği</param>
    /// <param name="data">Template verileri</param>
    /// <returns>Renderlenmiş template içeriği</returns>
    private string RenderTemplateContent(string template, Dictionary<string, object> data)
    {
        var result = template;
        
        foreach (var item in data)
        {
            var placeholder = $"{{{item.Key}}}";
            var value = item.Value?.ToString() ?? "";
            result = result.Replace(placeholder, value);
        }

        return result;
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

    #region Template Content

    /// <summary>
    /// Hoşgeldin email template'i
    /// </summary>
    /// <returns>Hoşgeldin email template'i</returns>
    private string GetWelcomeTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Getir'e Hoş Geldiniz</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #00d4aa; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #666; }
        .button { background-color: #00d4aa; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Getir'e Hoş Geldiniz!</h1>
        </div>
        <div class='content'>
            <h2>Merhaba {FirstName}!</h2>
            <p>Getir ailesine katıldığınız için teşekkür ederiz. Artık binlerce ürünü dakikalar içinde kapınıza getirebiliriz.</p>
            <p>İlk siparişinizde %20 indirim kazanmak için aşağıdaki butona tıklayın:</p>
            <p style='text-align: center;'>
                <a href='{DiscountLink}' class='button'>İlk Siparişimi Ver</a>
            </p>
            <h3>Getir'in Avantajları:</h3>
            <ul>
                <li>⚡ Dakikalar içinde teslimat</li>
                <li>🛒 Binlerce ürün seçeneği</li>
                <li>💰 Uygun fiyatlar</li>
                <li>🔒 Güvenli ödeme</li>
            </ul>
        </div>
        <div class='footer'>
            <p>Bu emaili {EmailAddress} adresine gönderdik.</p>
            <p>Getir - Dakikalar içinde kapınızda</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Sipariş onayı email template'i
    /// </summary>
    /// <returns>Sipariş onayı email template'i</returns>
    private string GetOrderConfirmationTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Sipariş Onayı</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #00d4aa; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .order-details { background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Siparişiniz Onaylandı!</h1>
        </div>
        <div class='content'>
            <h2>Merhaba {CustomerName}!</h2>
            <p>Siparişiniz başarıyla alındı ve onaylandı. Sipariş detaylarınız aşağıdadır:</p>
            
            <div class='order-details'>
                <h3>Sipariş Bilgileri</h3>
                <p><strong>Sipariş No:</strong> {OrderNumber}</p>
                <p><strong>Tarih:</strong> {OrderDate}</p>
                <p><strong>Teslimat Adresi:</strong> {DeliveryAddress}</p>
                <p><strong>Tahmini Teslimat:</strong> {EstimatedDelivery}</p>
                <p><strong>Toplam Tutar:</strong> {TotalAmount}</p>
            </div>

            <h3>Sipariş İçeriği:</h3>
            {OrderItems}

            <p>Siparişinizin durumunu takip etmek için uygulamamızı kullanabilirsiniz.</p>
        </div>
        <div class='footer'>
            <p>Getir - Dakikalar içinde kapınızda</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Sipariş hazır email template'i
    /// </summary>
    /// <returns>Sipariş hazır email template'i</returns>
    private string GetOrderReadyTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Sipariş Hazır</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #ff6b35; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Siparişiniz Hazır! 🚀</h1>
        </div>
        <div class='content'>
            <h2>Merhaba {CustomerName}!</h2>
            <p>Siparişiniz hazırlandı ve kuryemiz yola çıktı!</p>
            <p><strong>Sipariş No:</strong> {OrderNumber}</p>
            <p><strong>Tahmini Varış:</strong> {EstimatedArrival}</p>
            <p>Kuryemizin konumunu gerçek zamanlı olarak takip edebilirsiniz.</p>
        </div>
        <div class='footer'>
            <p>Getir - Dakikalar içinde kapınızda</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Sipariş teslim email template'i
    /// </summary>
    /// <returns>Sipariş teslim email template'i</returns>
    private string GetOrderDeliveredTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Sipariş Teslim Edildi</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #28a745; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Siparişiniz Teslim Edildi! ✅</h1>
        </div>
        <div class='content'>
            <h2>Merhaba {CustomerName}!</h2>
            <p>Siparişiniz başarıyla teslim edildi. Siparişinizden memnun kaldıysanız, lütfen değerlendirmenizi yapın.</p>
            <p><strong>Sipariş No:</strong> {OrderNumber}</p>
            <p><strong>Teslimat Tarihi:</strong> {DeliveryDate}</p>
            <p>Tekrar sipariş vermek için uygulamamızı kullanabilirsiniz.</p>
        </div>
        <div class='footer'>
            <p>Getir - Dakikalar içinde kapınızda</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetOrderCancelledTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Sipariş İptal Edildi</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #dc3545; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Siparişiniz İptal Edildi</h1>
        </div>
        <div class='content'>
            <h2>Merhaba {CustomerName}!</h2>
            <p>Maalesef siparişiniz iptal edilmiştir.</p>
            <p><strong>Sipariş No:</strong> {OrderNumber}</p>
            <p><strong>İptal Nedeni:</strong> {CancellationReason}</p>
            <p>Ödeme yaptıysanız, tutar 1-3 iş günü içinde hesabınıza iade edilecektir.</p>
        </div>
        <div class='footer'>
            <p>Getir - Dakikalar içinde kapınızda</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPasswordResetTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Şifre Sıfırlama</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #007bff; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .button { background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Şifre Sıfırlama</h1>
        </div>
        <div class='content'>
            <h2>Merhaba {FirstName}!</h2>
            <p>Hesabınız için şifre sıfırlama talebinde bulundunuz. Şifrenizi sıfırlamak için aşağıdaki butona tıklayın:</p>
            <p style='text-align: center;'>
                <a href='{ResetLink}' class='button'>Şifremi Sıfırla</a>
            </p>
            <p>Bu link 24 saat geçerlidir. Eğer bu talebi siz yapmadıysanız, bu emaili görmezden gelebilirsiniz.</p>
        </div>
        <div class='footer'>
            <p>Getir - Dakikalar içinde kapınızda</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetEmailVerificationTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Email Doğrulama</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #17a2b8; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .button { background-color: #17a2b8; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Email Adresinizi Doğrulayın</h1>
        </div>
        <div class='content'>
            <h2>Merhaba {FirstName}!</h2>
            <p>Getir hesabınızı oluşturmak için email adresinizi doğrulamanız gerekiyor.</p>
            <p style='text-align: center;'>
                <a href='{VerificationLink}' class='button'>Email Adresimi Doğrula</a>
            </p>
            <p>Bu link 24 saat geçerlidir.</p>
        </div>
        <div class='footer'>
            <p>Getir - Dakikalar içinde kapınızda</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPromotionTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Promosyon</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #ffc107; color: #333; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .button { background-color: #ffc107; color: #333; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{PromotionTitle}</h1>
        </div>
        <div class='content'>
            <h2>Merhaba {FirstName}!</h2>
            <p>{PromotionDescription}</p>
            <p style='text-align: center;'>
                <a href='{PromotionLink}' class='button'>Hemen Alışverişe Başla</a>
            </p>
            <p><strong>Geçerlilik Tarihi:</strong> {ValidUntil}</p>
        </div>
        <div class='footer'>
            <p>Getir - Dakikalar içinde kapınızda</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetNewsletterTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Getir Bülteni</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #6f42c1; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{NewsletterTitle}</h1>
        </div>
        <div class='content'>
            <h2>Merhaba {FirstName}!</h2>
            <p>{NewsletterContent}</p>
        </div>
        <div class='footer'>
            <p>Getir - Dakikalar içinde kapınızda</p>
        </div>
    </div>
</body>
</html>";
    }

    #endregion
}

/// <summary>
/// Email template service interface
/// </summary>
public interface IEmailTemplateService
{
    Task<Result<EmailTemplate>> GetTemplateAsync(string templateName, CancellationToken cancellationToken = default);
    Task<Result<string>> RenderTemplateAsync(string templateName, Dictionary<string, object> templateData, CancellationToken cancellationToken = default);
    Task<Result<EmailRequest>> CreateEmailFromTemplateAsync(EmailTemplateRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<EmailTemplate>>> GetAllTemplatesAsync(CancellationToken cancellationToken = default);
}
