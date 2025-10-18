using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Getir.Infrastructure.Services.Notifications;

/// <summary>
/// SMS service implementation using external SMS provider
/// Currently supports multiple providers: Netgsm, Iletimerkezi, etc.
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILoggingService _loggingService;
    private readonly HttpClient _httpClient;
    private readonly SmsConfiguration _smsConfig;
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILoggingService loggingService, HttpClient httpClient, IOptions<SmsConfiguration> smsConfig, ILogger<SmsService> logger)
    {
        _loggingService = loggingService;
        _httpClient = httpClient;
        _smsConfig = smsConfig.Value;
        _logger = logger;

        // Configure HTTP client
        _httpClient.Timeout = TimeSpan.FromSeconds(_smsConfig.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Getir-SMS-Service/1.0");
    }

    public async Task<Result> SendSmsAsync(SmsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate phone number
            var validationResult = await ValidatePhoneNumberAsync(request.PhoneNumber, cancellationToken);
            if (!validationResult.Success || !validationResult.Value)
            {
                return Result.Fail("Invalid phone number format", "INVALID_PHONE_NUMBER");
            }

            // Format phone number
            var formattedPhone = FormatPhoneNumber(request.PhoneNumber);

            // Send SMS based on provider
            var result = _smsConfig.ProviderName.ToLower() switch
            {
                "netgsm" => await SendViaNetgsmAsync(request, formattedPhone, cancellationToken),
                "iletimerkezi" => await SendViaIletimerkeziAsync(request, formattedPhone, cancellationToken),
                "mock" => await SendViaMockAsync(request, formattedPhone, cancellationToken),
                _ => Result.Fail($"Unsupported SMS provider: {_smsConfig.ProviderName}", "UNSUPPORTED_PROVIDER")
            };

            // Log SMS sending
            _loggingService.LogBusinessEvent("SmsSent", new
            {
                PhoneNumber = MaskPhoneNumber(request.PhoneNumber),
                MessageLength = request.Message.Length,
                Provider = _smsConfig.ProviderName,
                Success = result.Success
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", MaskPhoneNumber(request.PhoneNumber));
            _loggingService.LogError("SMS sending failed", ex, new { request.PhoneNumber });
            return Result.Fail("Failed to send SMS", "SMS_SEND_ERROR");
        }
    }

    public async Task<Result> SendBulkSmsAsync(IEnumerable<SmsRequest> requests, CancellationToken cancellationToken = default)
    {
        try
        {
            var results = new List<Result>();
            var batchSize = 100; // Process in batches to avoid overwhelming the provider
            var requestsList = requests.ToList();
            var batches = requestsList.Chunk(batchSize);

            foreach (var batch in batches)
            {
                var batchTasks = batch.Select(request => SendSmsAsync(request, cancellationToken));
                var batchResults = await Task.WhenAll(batchTasks);
                results.AddRange(batchResults);

                // Add delay between batches to respect rate limits
                if (batches.Count() > 1)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }

            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count - successCount;

            _loggingService.LogBusinessEvent("BulkSmsSent", new
            {
                TotalCount = results.Count,
                SuccessCount = successCount,
                FailureCount = failureCount
            });

            if (failureCount == 0)
            {
                return Result.Ok();
            }

            return Result.Fail($"Bulk SMS completed with {failureCount} failures", "BULK_SMS_PARTIAL_FAILURE");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk SMS");
            _loggingService.LogError("Bulk SMS sending failed", ex);
            return Result.Fail("Failed to send bulk SMS", "BULK_SMS_ERROR");
        }
    }

    public async Task<Result> SendTemplateSmsAsync(SmsTemplateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get template content
            var template = await GetSmsTemplateAsync(request.TemplateName, cancellationToken);
            if (!template.Success)
            {
                return Result.Fail($"Template '{request.TemplateName}' not found", "TEMPLATE_NOT_FOUND");
            }

            // Replace template variables
            var message = ReplaceTemplateVariables(template.Value!.Content, request.TemplateData);

            // Create SMS request
            var smsRequest = new SmsRequest(
                request.PhoneNumber,
                message,
                request.SenderId,
                request.ScheduledAt,
                new Dictionary<string, string> { { "TemplateName", request.TemplateName } });

            return await SendSmsAsync(smsRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending template SMS for template {TemplateName}", request.TemplateName);
            _loggingService.LogError("Template SMS sending failed", ex, new { request.TemplateName });
            return Result.Fail("Failed to send template SMS", "TEMPLATE_SMS_ERROR");
        }
    }

    public Task<Result<bool>> ValidatePhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Task.FromResult(Result.Fail<bool>("Phone number is required", "PHONE_NUMBER_REQUIRED"));
            }

            // Turkish phone number validation
            var turkishPhonePattern = @"^(\+90|0)?[5][0-9]{9}$";
            var isTurkishPhone = Regex.IsMatch(phoneNumber.Replace(" ", "").Replace("-", ""), turkishPhonePattern);

            if (!isTurkishPhone)
            {
                // International phone number validation (basic)
                var internationalPattern = @"^\+[1-9]\d{1,14}$";
                var isInternational = Regex.IsMatch(phoneNumber.Replace(" ", "").Replace("-", ""), internationalPattern);

                return Task.FromResult(Result.Ok(isInternational));
            }

            return Task.FromResult(Result.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating phone number");
            return Task.FromResult(Result.Fail<bool>("Failed to validate phone number", "PHONE_VALIDATION_ERROR"));
        }
    }

    public async Task<Result<SmsDeliveryStatus>> GetDeliveryStatusAsync(string messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = _smsConfig.ProviderName.ToLower() switch
            {
                "netgsm" => await GetNetgsmDeliveryStatusAsync(messageId, cancellationToken),
                "iletimerkezi" => await GetIletimerkeziDeliveryStatusAsync(messageId, cancellationToken),
                "mock" => await GetMockDeliveryStatusAsync(messageId, cancellationToken),
                _ => Result.Fail<SmsDeliveryStatus>($"Unsupported SMS provider: {_smsConfig.ProviderName}", "UNSUPPORTED_PROVIDER")
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SMS delivery status for message {MessageId}", messageId);
            return Result.Fail<SmsDeliveryStatus>("Failed to get delivery status", "DELIVERY_STATUS_ERROR");
        }
    }

    public async Task<Result<SmsBalance>> GetBalanceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = _smsConfig.ProviderName.ToLower() switch
            {
                "netgsm" => await GetNetgsmBalanceAsync(cancellationToken),
                "iletimerkezi" => await GetIletimerkeziBalanceAsync(cancellationToken),
                "mock" => await GetMockBalanceAsync(cancellationToken),
                _ => Result.Fail<SmsBalance>($"Unsupported SMS provider: {_smsConfig.ProviderName}", "UNSUPPORTED_PROVIDER")
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SMS balance");
            return Result.Fail<SmsBalance>("Failed to get SMS balance", "BALANCE_ERROR");
        }
    }

    public async Task<Result> SendOtpSmsAsync(string phoneNumber, string otpCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = $"Getir doğrulama kodunuz: {otpCode}. Bu kodu kimseyle paylaşmayın.";

            var request = new SmsRequest(
                phoneNumber,
                message,
                "GETIR",
                null,
                new Dictionary<string, string> { { "Type", "OTP" } });

            return await SendSmsAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP SMS to {PhoneNumber}", MaskPhoneNumber(phoneNumber));
            _loggingService.LogError("OTP SMS sending failed", ex, new { phoneNumber });
            return Result.Fail("Failed to send OTP SMS", "OTP_SMS_ERROR");
        }
    }

    #region Provider-Specific Implementations

    private async Task<Result> SendViaNetgsmAsync(SmsRequest request, string formattedPhone, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"{_smsConfig.BaseUrl}/sms/send/get";
            var parameters = new Dictionary<string, string>
            {
                ["usercode"] = _smsConfig.ApiKey,
                ["password"] = _smsConfig.ApiSecret,
                ["gsmno"] = formattedPhone,
                ["message"] = request.Message,
                ["msgheader"] = request.SenderId ?? "GETIR"
            };

            var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            var response = await _httpClient.GetAsync($"{url}?{queryString}", cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode && content.StartsWith("00"))
            {
                return Result.Ok();
            }

            return Result.Fail($"Netgsm API error: {content}", "NETGSM_API_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Netgsm SMS sending failed");
            return Result.Fail("Netgsm SMS sending failed", "NETGSM_ERROR");
        }
    }

    private async Task<Result> SendViaIletimerkeziAsync(SmsRequest request, string formattedPhone, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"{_smsConfig.BaseUrl}/api/sms/send";
            var payload = new
            {
                username = _smsConfig.ApiKey,
                password = _smsConfig.ApiSecret,
                source_addr = request.SenderId ?? "GETIR",
                dest_addr = formattedPhone,
                message = request.Message
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                if (result.TryGetProperty("response", out var responseElement) &&
                    responseElement.TryGetProperty("status", out var statusElement) &&
                    statusElement.GetString() == "success")
                {
                    return Result.Ok();
                }
            }

            return Result.Fail($"Iletimerkezi API error: {responseContent}", "ILETIMERKEZI_API_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Iletimerkezi SMS sending failed");
            return Result.Fail("Iletimerkezi SMS sending failed", "ILETIMERKEZI_ERROR");
        }
    }

    private async Task<Result> SendViaMockAsync(SmsRequest request, string formattedPhone, CancellationToken cancellationToken)
    {
        // Mock implementation for development/testing
        await Task.Delay(100, cancellationToken); // Simulate network delay

        _logger.LogInformation("Mock SMS sent to {PhoneNumber}: {Message}",
            MaskPhoneNumber(formattedPhone), request.Message);

        return Result.Ok();
    }

    #endregion

    #region Helper Methods

    private string FormatPhoneNumber(string phoneNumber)
    {
        var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        if (cleaned.StartsWith("0"))
        {
            return "+90" + cleaned.Substring(1);
        }

        if (cleaned.StartsWith("+90"))
        {
            return cleaned;
        }

        if (cleaned.StartsWith("90"))
        {
            return "+" + cleaned;
        }

        return "+90" + cleaned;
    }

    private string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
            return "***";

        return phoneNumber.Substring(0, 3) + "***" + phoneNumber.Substring(phoneNumber.Length - 2);
    }

    private string ReplaceTemplateVariables(string template, Dictionary<string, string> variables)
    {
        var result = template;
        foreach (var variable in variables)
        {
            result = result.Replace($"{{{variable.Key}}}", variable.Value);
        }
        return result;
    }

    private Task<Result<SmsTemplate>> GetSmsTemplateAsync(string templateName, CancellationToken cancellationToken)
    {
        // In a real implementation, this would fetch from database or configuration
        var templates = new Dictionary<string, SmsTemplate>
        {
            ["order_confirmation"] = new SmsTemplate("order_confirmation", "Siparişiniz onaylandı! Sipariş No: {OrderNumber}. Tahmini teslimat: {DeliveryTime}"),
            ["order_ready"] = new SmsTemplate("order_ready", "Siparişiniz hazır! Sipariş No: {OrderNumber}. Kurye yolda!"),
            ["order_delivered"] = new SmsTemplate("order_delivered", "Siparişiniz teslim edildi! Sipariş No: {OrderNumber}. Teşekkürler!"),
            ["otp"] = new SmsTemplate("otp", "Getir doğrulama kodunuz: {OtpCode}. Bu kodu kimseyle paylaşmayın."),
            ["promotion"] = new SmsTemplate("promotion", "{PromotionTitle}: {PromotionDescription}. Detaylar: {PromotionUrl}")
        };

        if (templates.TryGetValue(templateName, out var template))
        {
            return Task.FromResult(Result.Ok(template));
        }

        return Task.FromResult(Result.Fail<SmsTemplate>($"Template '{templateName}' not found", "TEMPLATE_NOT_FOUND"));
    }

    #endregion

    #region Delivery Status and Balance Methods (Mock implementations)

    private Task<Result<SmsDeliveryStatus>> GetNetgsmDeliveryStatusAsync(string messageId, CancellationToken cancellationToken)
    {
        // Mock implementation
        return Task.FromResult(Result.Ok(new SmsDeliveryStatus(messageId, SmsStatus.Delivered, DateTime.UtcNow, null, null, 0)));
    }

    private Task<Result<SmsDeliveryStatus>> GetIletimerkeziDeliveryStatusAsync(string messageId, CancellationToken cancellationToken)
    {
        // Mock implementation
        return Task.FromResult(Result.Ok(new SmsDeliveryStatus(messageId, SmsStatus.Delivered, DateTime.UtcNow, null, null, 0)));
    }

    private Task<Result<SmsDeliveryStatus>> GetMockDeliveryStatusAsync(string messageId, CancellationToken cancellationToken)
    {
        // Mock implementation
        return Task.FromResult(Result.Ok(new SmsDeliveryStatus(messageId, SmsStatus.Delivered, DateTime.UtcNow, null, null, 0)));
    }

    private Task<Result<SmsBalance>> GetNetgsmBalanceAsync(CancellationToken cancellationToken)
    {
        // Mock implementation
        return Task.FromResult(Result.Ok(new SmsBalance(1000, 500, 1000, DateTime.UtcNow)));
    }

    private Task<Result<SmsBalance>> GetIletimerkeziBalanceAsync(CancellationToken cancellationToken)
    {
        // Mock implementation
        return Task.FromResult(Result.Ok(new SmsBalance(1000, 500, 1000, DateTime.UtcNow)));
    }

    private Task<Result<SmsBalance>> GetMockBalanceAsync(CancellationToken cancellationToken)
    {
        // Mock implementation
        return Task.FromResult(Result.Ok(new SmsBalance(1000, 500, 1000, DateTime.UtcNow)));
    }

    #endregion
}
