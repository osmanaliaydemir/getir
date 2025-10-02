using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Abstractions;

/// <summary>
/// Push notification service interface
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Send push notification to device
    /// </summary>
    Task<Result> SendPushNotificationAsync(
        PushNotificationRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send push notification to multiple devices
    /// </summary>
    Task<Result> SendBulkPushNotificationAsync(
        IEnumerable<PushNotificationRequest> requests, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send push notification to topic
    /// </summary>
    Task<Result> SendPushToTopicAsync(
        PushTopicRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send push notification to user
    /// </summary>
    Task<Result> SendPushToUserAsync(
        Guid userId,
        PushNotificationRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Register device token
    /// </summary>
    Task<Result> RegisterDeviceTokenAsync(
        DeviceTokenRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregister device token
    /// </summary>
    Task<Result> UnregisterDeviceTokenAsync(
        string deviceToken, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get push notification status
    /// </summary>
    Task<Result<PushNotificationStatus>> GetNotificationStatusAsync(
        string messageId, 
        CancellationToken cancellationToken = default);
}
