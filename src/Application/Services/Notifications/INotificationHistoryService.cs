using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Notifications;

/// <summary>
/// Bildirim geçmişi servisi: bildirimlerin loglanması, durum takibi, istatistikler.
/// </summary>
public interface INotificationHistoryService
{
    /// <summary>Bildirim isteğini loglar (metadata, timestamp'ler).</summary>
    Task<Result<Guid>> LogNotificationAsync(LogNotificationRequest request, CancellationToken cancellationToken = default);
    /// <summary>Bildirim durumunu günceller (sent/delivered/failed).</summary>
    Task<Result> UpdateNotificationStatusAsync(Guid historyId, Domain.Entities.NotificationStatus status, string? errorMessage = null, string? errorCode = null, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcı için bildirim geçmişini getirir (filtreleme ve sayfalama).</summary>
    Task<Result<PagedResult<NotificationHistoryResponse>>> GetNotificationHistoryAsync(Guid userId, NotificationHistoryQuery query, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcı için bildirim istatistiklerini getirir.</summary>
    Task<Result<NotificationStatistics>> GetNotificationStatisticsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    /// <summary>Başarısız bildirimi yeniden dener.</summary>
    Task<Result> RetryFailedNotificationAsync(Guid historyId, CancellationToken cancellationToken = default);
    /// <summary>Eski bildirimleri temizler.</summary>
    Task<Result> CleanupOldNotificationsAsync(int daysToKeep = 90, CancellationToken cancellationToken = default);
}
