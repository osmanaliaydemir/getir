using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.RealtimeTracking;

/// <summary>
/// Tracking bildirim servisi implementasyonu: mock data ile push/email/sms bildirimleri, okundu yönetimi.
/// </summary>
public class TrackingNotificationService : ITrackingNotificationService
{
    private readonly List<TrackingNotificationDto> _mockNotifications;

    public TrackingNotificationService()
    {
        // Mock notifications
        _mockNotifications = new List<TrackingNotificationDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderTrackingId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Type = NotificationType.StatusUpdate,
                TypeDisplayName = "Durum Güncellemesi",
                Title = "Sipariş Durumu Güncellendi",
                Message = "Siparişiniz hazırlanıyor",
                Data = "{\"status\":\"Preparing\",\"estimatedTime\":\"30 minutes\"}",
                IsSent = true,
                IsRead = false,
                SentAt = DateTime.UtcNow.AddMinutes(-5),
                DeliveryMethod = "push",
                DeliveryStatus = "delivered",
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderTrackingId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Type = NotificationType.LocationUpdate,
                TypeDisplayName = "Konum Güncellemesi",
                Title = "Konum Güncellendi",
                Message = "Siparişiniz size doğru yolda",
                Data = "{\"latitude\":41.0082,\"longitude\":28.9784}",
                IsSent = true,
                IsRead = true,
                SentAt = DateTime.UtcNow.AddMinutes(-2),
                ReadAt = DateTime.UtcNow.AddMinutes(-1),
                DeliveryMethod = "push",
                DeliveryStatus = "delivered",
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            }
        };
    }

    /// <summary>
    /// Bildirim gönderir (push/email/sms, mock data).
    /// </summary>
    public Task<SendNotificationResponse> SendNotificationAsync(SendNotificationRequest request)
    {
        var notification = new TrackingNotificationDto
        {
            Id = Guid.NewGuid(),
            OrderTrackingId = request.OrderTrackingId,
            UserId = request.UserId,
            Type = request.Type,
            TypeDisplayName = request.Type.GetDisplayName(),
            Title = request.Title,
            Message = request.Message,
            Data = request.Data,
            IsSent = true,
            IsRead = false,
            SentAt = DateTime.UtcNow,
            DeliveryMethod = request.DeliveryMethod ?? "push",
            DeliveryStatus = "sent",
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _mockNotifications.Add(notification);

        return Task.FromResult(new SendNotificationResponse
        {
            Success = true,
            Message = "Notification sent successfully",
            NotificationId = notification.Id,
            SentAt = notification.SentAt ?? DateTime.UtcNow
        });
    }

    /// <summary>
    /// Tracking ID bazlı bildirimleri getirir (zaman sıralı, mock data).
    /// </summary>
    public Task<List<TrackingNotificationDto>> GetNotificationsByTrackingIdAsync(Guid orderTrackingId)
    {
        var notifications = _mockNotifications
            .Where(n => n.OrderTrackingId == orderTrackingId)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        return Task.FromResult(notifications);
    }

    /// <summary>
    /// Kullanıcı bazlı bildirimleri getirir (zaman sıralı, mock data).
    /// </summary>
    public Task<List<TrackingNotificationDto>> GetNotificationsByUserIdAsync(Guid userId)
    {
        var notifications = _mockNotifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        return Task.FromResult(notifications);
    }

    /// <summary>
    /// Okunmamış bildirimleri getirir (mock data).
    /// </summary>
    public Task<List<TrackingNotificationDto>> GetUnreadNotificationsAsync(Guid userId)
    {
        var unreadNotifications = _mockNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        return Task.FromResult(unreadNotifications);
    }

    /// <summary>
    /// Bildirimi okundu olarak işaretler (mock data).
    /// </summary>
    public Task<bool> MarkNotificationAsReadAsync(Guid notificationId)
    {
        var notification = _mockNotifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <summary>
    /// Tüm bildirimleri okundu olarak işaretler (mock data).
    /// </summary>
    public Task<bool> MarkAllNotificationsAsReadAsync(Guid userId)
    {
        var userNotifications = _mockNotifications.Where(n => n.UserId == userId && !n.IsRead);
        foreach (var notification in userNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }
        return Task.FromResult(true);
    }

    /// <summary>
    /// Bildirimi siler (mock data).
    /// </summary>
    public Task<bool> DeleteNotificationAsync(Guid notificationId)
    {
        var notification = _mockNotifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification != null)
        {
            _mockNotifications.Remove(notification);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <summary>
    /// Tip bazlı bildirimleri getirir (tarih filtresi, mock data).
    /// </summary>
    public Task<List<TrackingNotificationDto>> GetNotificationsByTypeAsync(NotificationType type, DateTime? startDate = null, DateTime? endDate = null)
    {
        var notifications = _mockNotifications.Where(n => n.Type == type);

        if (startDate.HasValue)
            notifications = notifications.Where(n => n.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            notifications = notifications.Where(n => n.CreatedAt <= endDate.Value);

        return Task.FromResult(notifications.OrderByDescending(n => n.CreatedAt).ToList());
    }

    /// <summary>
    /// Durum güncelleme bildirimi gönderir (mock data).
    /// </summary>
    public Task<bool> SendStatusUpdateNotificationAsync(Guid orderTrackingId, TrackingStatus newStatus, string? message = null)
    {
        var notification = new TrackingNotificationDto
        {
            Id = Guid.NewGuid(),
            OrderTrackingId = orderTrackingId,
            Type = NotificationType.StatusUpdate,
            TypeDisplayName = "Durum Güncellemesi",
            Title = "Sipariş Durumu Güncellendi",
            Message = message ?? $"Sipariş durumu: {newStatus.GetDisplayName()}",
            Data = $"{{\"status\":\"{newStatus}\",\"statusName\":\"{newStatus.GetDisplayName()}\"}}",
            IsSent = true,
            IsRead = false,
            SentAt = DateTime.UtcNow,
            DeliveryMethod = "push",
            DeliveryStatus = "sent",
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _mockNotifications.Add(notification);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Konum güncelleme bildirimi gönderir (mock data).
    /// </summary>
    public Task<bool> SendLocationUpdateNotificationAsync(Guid orderTrackingId, double latitude, double longitude)
    {
        var notification = new TrackingNotificationDto
        {
            Id = Guid.NewGuid(),
            OrderTrackingId = orderTrackingId,
            Type = NotificationType.LocationUpdate,
            TypeDisplayName = "Konum Güncellemesi",
            Title = "Konum Güncellendi",
            Message = "Siparişinizin konumu güncellendi",
            Data = $"{{\"latitude\":{latitude},\"longitude\":{longitude}}}",
            IsSent = true,
            IsRead = false,
            SentAt = DateTime.UtcNow,
            DeliveryMethod = "push",
            DeliveryStatus = "sent",
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _mockNotifications.Add(notification);
        return Task.FromResult(true);
    }

    /// <summary>
    /// ETA güncelleme bildirimi gönderir (mock data).
    /// </summary>
    public Task<bool> SendETAUpdateNotificationAsync(Guid orderTrackingId, DateTime estimatedArrivalTime)
    {
        var notification = new TrackingNotificationDto
        {
            Id = Guid.NewGuid(),
            OrderTrackingId = orderTrackingId,
            Type = NotificationType.ETAUpdate,
            TypeDisplayName = "Tahmini Varış Süresi",
            Title = "Tahmini Varış Süresi Güncellendi",
            Message = $"Tahmini varış süresi: {estimatedArrivalTime:HH:mm}",
            Data = $"{{\"estimatedArrivalTime\":\"{estimatedArrivalTime:yyyy-MM-ddTHH:mm:ssZ}\"}}",
            IsSent = true,
            IsRead = false,
            SentAt = DateTime.UtcNow,
            DeliveryMethod = "push",
            DeliveryStatus = "sent",
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _mockNotifications.Add(notification);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Teslimat uyarısı gönderir (yaklaşıyor bildirimi, mock data).
    /// </summary>
    public Task<bool> SendDeliveryAlertAsync(Guid orderTrackingId)
    {
        var notification = new TrackingNotificationDto
        {
            Id = Guid.NewGuid(),
            OrderTrackingId = orderTrackingId,
            Type = NotificationType.DeliveryAlert,
            TypeDisplayName = "Teslimat Uyarısı",
            Title = "Teslimat Uyarısı",
            Message = "Siparişiniz yaklaşıyor! Lütfen hazır olun.",
            Data = "{\"alertType\":\"delivery\",\"urgent\":true}",
            IsSent = true,
            IsRead = false,
            SentAt = DateTime.UtcNow,
            DeliveryMethod = "push",
            DeliveryStatus = "sent",
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _mockNotifications.Add(notification);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Gecikme uyarısı gönderir (dakika bilgisi ile, mock data).
    /// </summary>
    public Task<bool> SendDelayAlertAsync(Guid orderTrackingId, int delayMinutes)
    {
        var notification = new TrackingNotificationDto
        {
            Id = Guid.NewGuid(),
            OrderTrackingId = orderTrackingId,
            Type = NotificationType.DelayAlert,
            TypeDisplayName = "Gecikme Uyarısı",
            Title = "Teslimat Gecikmesi",
            Message = $"Siparişinizde {delayMinutes} dakika gecikme var. Özür dileriz.",
            Data = $"{{\"delayMinutes\":{delayMinutes},\"alertType\":\"delay\"}}",
            IsSent = true,
            IsRead = false,
            SentAt = DateTime.UtcNow,
            DeliveryMethod = "push",
            DeliveryStatus = "sent",
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _mockNotifications.Add(notification);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Tamamlanma bildirimi gönderir (teslimat başarılı, mock data).
    /// </summary>
    public Task<bool> SendCompletionAlertAsync(Guid orderTrackingId)
    {
        var notification = new TrackingNotificationDto
        {
            Id = Guid.NewGuid(),
            OrderTrackingId = orderTrackingId,
            Type = NotificationType.CompletionAlert,
            TypeDisplayName = "Tamamlanma Bildirimi",
            Title = "Sipariş Teslim Edildi",
            Message = "Siparişiniz başarıyla teslim edildi. Teşekkür ederiz!",
            Data = "{\"alertType\":\"completion\",\"status\":\"delivered\"}",
            IsSent = true,
            IsRead = false,
            SentAt = DateTime.UtcNow,
            DeliveryMethod = "push",
            DeliveryStatus = "sent",
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _mockNotifications.Add(notification);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Okunmamış bildirim sayısını getirir (mock data).
    /// </summary>
    public Task<int> GetUnreadNotificationCountAsync(Guid userId)
    {
        var unreadCount = _mockNotifications.Count(n => n.UserId == userId && !n.IsRead);
        return Task.FromResult(unreadCount);
    }
}
