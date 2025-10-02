using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Abstractions;

/// <summary>
/// SMS service interface for sending SMS messages
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Send SMS message
    /// </summary>
    Task<Result> SendSmsAsync(
        SmsRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send bulk SMS messages
    /// </summary>
    Task<Result> SendBulkSmsAsync(
        IEnumerable<SmsRequest> requests, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send SMS with template
    /// </summary>
    Task<Result> SendTemplateSmsAsync(
        SmsTemplateRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate phone number
    /// </summary>
    Task<Result<bool>> ValidatePhoneNumberAsync(
        string phoneNumber, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get SMS delivery status
    /// </summary>
    Task<Result<SmsDeliveryStatus>> GetDeliveryStatusAsync(
        string messageId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get SMS balance
    /// </summary>
    Task<Result<SmsBalance>> GetBalanceAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send OTP SMS
    /// </summary>
    Task<Result> SendOtpSmsAsync(
        string phoneNumber, 
        string otpCode, 
        CancellationToken cancellationToken = default);
}
