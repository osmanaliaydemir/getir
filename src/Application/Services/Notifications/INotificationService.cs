using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Notifications;

/// <summary>
/// Bildirim servisi: kullanıcı bildirimlerinin CRUD işlemleri ve SignalR entegrasyonu.
/// </summary>
public interface INotificationService
{
    /// <summary>Kullanıcı bildirimlerini sayfalama ile getirir.</summary>
    Task<Result<PagedResult<NotificationResponse>>> GetUserNotificationsAsync(Guid userId, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Bildirimleri okundu olarak işaretler.</summary>
    Task<Result> MarkAsReadAsync(Guid userId, MarkAsReadRequest request, CancellationToken cancellationToken = default);
    /// <summary>Yeni bildirim oluşturur ve SignalR ile gönderir.</summary>
    Task<Result<NotificationResponse>> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
    
    // SignalR Hub methods
    /// <summary>Bildirimi okundu olarak işaretler.</summary>
    Task<Result> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Okunmamış bildirim sayısını getirir.</summary>
    Task<Result<int>> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcı bildirimlerini belirtilen sayıda getirir.</summary>
    Task<Result<List<NotificationResponse>>> GetUserNotificationsAsync(Guid userId, int count, CancellationToken cancellationToken = default);
    /// <summary>Tüm bildirimleri okundu olarak işaretler.</summary>
    Task<Result> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Bildirimi siler.</summary>
    Task<Result> DeleteNotificationAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Sipariş durum bildirimi gönderir.</summary>
    Task<Result> SendOrderStatusNotificationAsync(Guid userId, Guid orderId, Domain.Enums.OrderStatus status, CancellationToken cancellationToken = default);
}
