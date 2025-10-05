using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.RealtimeTracking;

public interface ITrackingNotificationService
{
    Task<SendNotificationResponse> SendNotificationAsync(SendNotificationRequest request);
    Task<List<TrackingNotificationDto>> GetNotificationsByTrackingIdAsync(Guid orderTrackingId);
    Task<List<TrackingNotificationDto>> GetNotificationsByUserIdAsync(Guid userId);
    Task<List<TrackingNotificationDto>> GetUnreadNotificationsAsync(Guid userId);
    Task<bool> MarkNotificationAsReadAsync(Guid notificationId);
    Task<bool> MarkAllNotificationsAsReadAsync(Guid userId);
    Task<bool> DeleteNotificationAsync(Guid notificationId);
    Task<List<TrackingNotificationDto>> GetNotificationsByTypeAsync(NotificationType type, DateTime? startDate = null, DateTime? endDate = null);
    Task<bool> SendStatusUpdateNotificationAsync(Guid orderTrackingId, TrackingStatus newStatus, string? message = null);
    Task<bool> SendLocationUpdateNotificationAsync(Guid orderTrackingId, double latitude, double longitude);
    Task<bool> SendETAUpdateNotificationAsync(Guid orderTrackingId, DateTime estimatedArrivalTime);
    Task<bool> SendDeliveryAlertAsync(Guid orderTrackingId);
    Task<bool> SendDelayAlertAsync(Guid orderTrackingId, int delayMinutes);
    Task<bool> SendCompletionAlertAsync(Guid orderTrackingId);
    Task<int> GetUnreadNotificationCountAsync(Guid userId);
}
