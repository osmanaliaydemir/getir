using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Notifications;

/// <summary>
/// Interface for notification history service
/// </summary>
public interface INotificationHistoryService
{
    /// <summary>
    /// Log a notification request
    /// </summary>
    Task<Result<Guid>> LogNotificationAsync(LogNotificationRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Update notification status
    /// </summary>
    Task<Result> UpdateNotificationStatusAsync(Guid historyId, Domain.Entities.NotificationStatus status, string? errorMessage = null, string? errorCode = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Get notification history for a user
    /// </summary>
    Task<Result<PagedResult<NotificationHistoryResponse>>> GetNotificationHistoryAsync(Guid userId, NotificationHistoryQuery query, CancellationToken cancellationToken = default);
    /// <summary>
    /// Get notification statistics for a user
    /// </summary>
    Task<Result<NotificationStatistics>> GetNotificationStatisticsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// Retry a failed notification
    /// </summary>
    Task<Result> RetryFailedNotificationAsync(Guid historyId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Cleanup old notifications
    /// </summary>
    Task<Result> CleanupOldNotificationsAsync(int daysToKeep = 90, CancellationToken cancellationToken = default);
}
