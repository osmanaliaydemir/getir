using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.RealtimeTracking;

/// <summary>
/// Tracking bildirim servisi: durum/konum/ETA bildirimleri, gecikme/teslimat uyarıları, okundu işaretleme.
/// </summary>
public interface ITrackingNotificationService
{
    /// <summary>Bildirim gönderir (push/email/sms).</summary>
    Task<SendNotificationResponse> SendNotificationAsync(SendNotificationRequest request);
    /// <summary>Tracking ID bazlı bildirimleri getirir.</summary>
    Task<List<TrackingNotificationDto>> GetNotificationsByTrackingIdAsync(Guid orderTrackingId);
    /// <summary>Kullanıcı bazlı bildirimleri getirir.</summary>
    Task<List<TrackingNotificationDto>> GetNotificationsByUserIdAsync(Guid userId);
    /// <summary>Okunmamış bildirimleri getirir.</summary>
    Task<List<TrackingNotificationDto>> GetUnreadNotificationsAsync(Guid userId);
    /// <summary>Bildirimi okundu olarak işaretler.</summary>
    Task<bool> MarkNotificationAsReadAsync(Guid notificationId);
    /// <summary>Tüm bildirimleri okundu olarak işaretler.</summary>
    Task<bool> MarkAllNotificationsAsReadAsync(Guid userId);
    /// <summary>Bildirimi siler.</summary>
    Task<bool> DeleteNotificationAsync(Guid notificationId);
    /// <summary>Tip bazlı bildirimleri getirir (tarih filtresi).</summary>
    Task<List<TrackingNotificationDto>> GetNotificationsByTypeAsync(NotificationType type, DateTime? startDate = null, DateTime? endDate = null);
    /// <summary>Durum güncelleme bildirimi gönderir.</summary>
    Task<bool> SendStatusUpdateNotificationAsync(Guid orderTrackingId, TrackingStatus newStatus, string? message = null);
    /// <summary>Konum güncelleme bildirimi gönderir.</summary>
    Task<bool> SendLocationUpdateNotificationAsync(Guid orderTrackingId, double latitude, double longitude);
    /// <summary>ETA güncelleme bildirimi gönderir.</summary>
    Task<bool> SendETAUpdateNotificationAsync(Guid orderTrackingId, DateTime estimatedArrivalTime);
    /// <summary>Teslimat uyarısı gönderir (yaklaşıyor).</summary>
    Task<bool> SendDeliveryAlertAsync(Guid orderTrackingId);
    /// <summary>Gecikme uyarısı gönderir.</summary>
    Task<bool> SendDelayAlertAsync(Guid orderTrackingId, int delayMinutes);
    /// <summary>Tamamlanma bildirimi gönderir.</summary>
    Task<bool> SendCompletionAlertAsync(Guid orderTrackingId);
    /// <summary>Okunmamış bildirim sayısını getirir.</summary>
    Task<int> GetUnreadNotificationCountAsync(Guid userId);
}
