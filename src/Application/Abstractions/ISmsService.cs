using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Abstractions;

/// <summary>
/// SMS service interface
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// SMS gönder
    /// </summary>
    /// <param name="request">SMS isteği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>SMS gönderim sonucu</returns>
    Task<Result> SendSmsAsync(
        SmsRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Çoklu SMS gönder
    /// </summary>
    /// <param name="requests">SMS istekleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>SMS gönderim sonucu</returns>
    Task<Result> SendBulkSmsAsync(
        IEnumerable<SmsRequest> requests, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Template kullanarak SMS gönder
    /// </summary>
    /// <param name="request">SMS isteği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>SMS gönderim sonucu</returns>
    Task<Result> SendTemplateSmsAsync(
        SmsTemplateRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Telefon numarası doğrula
    /// </summary>
    /// <param name="phoneNumber">Telefon numarası</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Telefon numarası doğrulama sonucu</returns>
    Task<Result<bool>> ValidatePhoneNumberAsync(
        string phoneNumber, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// SMS gönderim durumu al
    /// </summary>
    /// <param name="messageId">Mesaj ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>SMS gönderim durumu</returns>
    Task<Result<SmsDeliveryStatus>> GetDeliveryStatusAsync(
        string messageId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// SMS bakiyeyi getir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>SMS bakiyeyi</returns>
    Task<Result<SmsBalance>> GetBalanceAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// OTP SMS gönder
    /// </summary>
    /// <param name="phoneNumber">Telefon numarası</param>
    /// <param name="otpCode">OTP kodu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>OTP SMS gönderim sonucu</returns>
    Task<Result> SendOtpSmsAsync(
        string phoneNumber, 
        string otpCode, 
        CancellationToken cancellationToken = default);
}
