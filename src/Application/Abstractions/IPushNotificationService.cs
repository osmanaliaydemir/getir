using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Abstractions;

/// <summary>
/// Push bildirim servisi interface
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Push bildirimi cihaza gönder
    /// </summary>
    Task<Result> SendPushNotificationAsync(
        PushNotificationRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Push bildirimi birden fazla cihaza gönder
    /// </summary>
    Task<Result> SendBulkPushNotificationAsync(
        IEnumerable<PushNotificationRequest> requests, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Push bildirimi konuya gönder
    /// </summary>
    Task<Result> SendPushToTopicAsync(
        PushTopicRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Push bildirimi kullanıcıya gönder
    /// </summary>
    Task<Result> SendPushToUserAsync(
        Guid userId,
        PushNotificationRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cihaz tokeni kaydet
    /// </summary>
    Task<Result> RegisterDeviceTokenAsync(
        DeviceTokenRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cihaz tokeni sil
    /// </summary>
    Task<Result> UnregisterDeviceTokenAsync(
        string deviceToken, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Push bildirimi durumunu al
    /// </summary>
    Task<Result<PushNotificationStatus>> GetNotificationStatusAsync(
        string messageId, 
        CancellationToken cancellationToken = default);
}
