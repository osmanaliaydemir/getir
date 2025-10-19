using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NotificationChannel = Getir.Application.DTO.NotificationChannel;
using NotificationStatus = Getir.Application.DTO.NotificationStatus;

namespace Getir.Application.Services.Notifications;

/// <summary>
/// Notification history service for tracking and managing notification history
/// </summary>
public class NotificationHistoryService : BaseService, INotificationHistoryService
{
    public NotificationHistoryService(IUnitOfWork unitOfWork, ILogger<NotificationHistoryService> logger, ILoggingService loggingService, ICacheService cacheService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
    }

    public async Task<Result<Guid>> LogNotificationAsync(LogNotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var history = new NotificationHistory
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                Channel = (Domain.Entities.NotificationChannel)request.Channel,
                Status = (Domain.Entities.NotificationStatus)request.Status,
                RelatedEntityId = request.RelatedEntityId,
                RelatedEntityType = request.RelatedEntityType,
                ExternalMessageId = request.ExternalMessageId,
                ErrorMessage = request.ErrorMessage,
                ErrorCode = request.ErrorCode,
                RetryCount = request.RetryCount,
                ScheduledAt = request.ScheduledAt,
                SentAt = request.SentAt,
                DeliveredAt = request.DeliveredAt,
                ReadAt = request.ReadAt,
                FailedAt = request.FailedAt,
                Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<NotificationHistory>().AddAsync(history, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("NotificationHistoryLogged", new
            {
                history.Id,
                request.UserId,
                request.Type,
                request.Channel,
                request.Status
            });

            return Result.Ok(history.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging notification history for user {UserId}", request.UserId);
            _loggingService.LogError("Log notification history failed", ex, new { request.UserId, request.Type, request.Channel });
            return Result.Fail<Guid>("Failed to log notification history", "LOG_NOTIFICATION_HISTORY_ERROR");
        }
    }

    public async Task<Result> UpdateNotificationStatusAsync(Guid historyId, Domain.Entities.NotificationStatus status, string? errorMessage = null, string? errorCode = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _unitOfWork.ReadRepository<NotificationHistory>()
                .FirstOrDefaultAsync(
                    filter: h => h.Id == historyId,
                    cancellationToken: cancellationToken);

            if (history == null)
            {
                return Result.Fail("Notification history not found", "HISTORY_NOT_FOUND");
            }

            history.Status = status;
            history.UpdatedAt = DateTime.UtcNow;

            switch (status)
            {
                case Domain.Entities.NotificationStatus.Sent:
                    history.SentAt = DateTime.UtcNow;
                    break;
                case Domain.Entities.NotificationStatus.Delivered:
                    history.DeliveredAt = DateTime.UtcNow;
                    break;
                case Domain.Entities.NotificationStatus.Failed:
                    history.FailedAt = DateTime.UtcNow;
                    history.ErrorMessage = errorMessage;
                    history.ErrorCode = errorCode;
                    break;
                    // Read status is tracked via ReadAt timestamp, not as a separate status
            }

            _unitOfWork.Repository<NotificationHistory>().Update(history);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("NotificationStatusUpdated", new
            {
                HistoryId = historyId,
                NewStatus = status,
                ErrorMessage = errorMessage
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification status for history {HistoryId}", historyId);
            _loggingService.LogError("Update notification status failed", ex, new { historyId, status });
            return Result.Fail("Failed to update notification status", "UPDATE_NOTIFICATION_STATUS_ERROR");
        }
    }

    public async Task<Result<PagedResult<NotificationHistoryResponse>>> GetNotificationHistoryAsync(Guid userId, NotificationHistoryQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var histories = await _unitOfWork.ReadRepository<NotificationHistory>()
                .GetPagedAsync(
                    filter: h => h.UserId == userId &&
                                (query.Type == null || h.Type == query.Type) &&
                                (query.Channel == null || h.Channel == (Domain.Entities.NotificationChannel)query.Channel) &&
                                (query.Status == null || h.Status == (Domain.Entities.NotificationStatus)query.Status) &&
                                (query.FromDate == null || h.CreatedAt >= query.FromDate) &&
                                (query.ToDate == null || h.CreatedAt <= query.ToDate),
                    orderBy: h => h.CreatedAt,
                    ascending: false,
                    page: query.Page,
                    pageSize: query.PageSize,
                    cancellationToken: cancellationToken);

            var total = await _unitOfWork.ReadRepository<NotificationHistory>()
                .CountAsync(
                    filter: h => h.UserId == userId &&
                                (query.Type == null || h.Type == query.Type) &&
                                (query.Channel == null || h.Channel == (Domain.Entities.NotificationChannel)query.Channel) &&
                                (query.Status == null || h.Status == (Domain.Entities.NotificationStatus)query.Status) &&
                                (query.FromDate == null || h.CreatedAt >= query.FromDate) &&
                                (query.ToDate == null || h.CreatedAt <= query.ToDate),
                    cancellationToken: cancellationToken);

            var response = histories.Select(h => new NotificationHistoryResponse(
                h.Id,
                h.Title,
                h.Message,
                (NotificationType)h.Type,
                (NotificationChannel)h.Channel,
                (NotificationStatus)h.Status,
                h.RelatedEntityId,
                h.RelatedEntityType,
                h.ExternalMessageId,
                h.ErrorMessage,
                h.ErrorCode,
                h.RetryCount,
                h.ScheduledAt,
                h.SentAt,
                h.DeliveredAt,
                h.ReadAt,
                h.FailedAt,
                h.Metadata,
                h.CreatedAt,
                h.UpdatedAt
            )).ToList();

            var pagedResult = PagedResult<NotificationHistoryResponse>.Create(response, total, query.Page, query.PageSize);

            return Result.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification history for user {UserId}", userId);
            _loggingService.LogError("Get notification history failed", ex, new { userId });
            return Result.Fail<PagedResult<NotificationHistoryResponse>>("Failed to get notification history", "GET_NOTIFICATION_HISTORY_ERROR");
        }
    }

    public async Task<Result<NotificationStatistics>> GetNotificationStatisticsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var histories = await _unitOfWork.ReadRepository<NotificationHistory>()
                .ListAsync(
                    filter: h => h.UserId == userId &&
                                (fromDate == null || h.CreatedAt >= fromDate) &&
                                (toDate == null || h.CreatedAt <= toDate),
                    cancellationToken: cancellationToken);

            var statistics = new NotificationStatistics(
                TotalNotifications: histories.Count,
                SentNotifications: histories.Count(h => h.Status == Domain.Entities.NotificationStatus.Sent),
                DeliveredNotifications: histories.Count(h => h.Status == Domain.Entities.NotificationStatus.Delivered),
                FailedNotifications: histories.Count(h => h.Status == Domain.Entities.NotificationStatus.Failed),
                ReadNotifications: histories.Count(h => h.ReadAt.HasValue),
                EmailNotifications: histories.Count(h => h.Channel == Domain.Entities.NotificationChannel.Email),
                SmsNotifications: histories.Count(h => h.Channel == Domain.Entities.NotificationChannel.Sms),
                PushNotifications: histories.Count(h => h.Channel == Domain.Entities.NotificationChannel.Push),
                InAppNotifications: histories.Count(h => h.Channel == Domain.Entities.NotificationChannel.InApp),
                AverageDeliveryTime: CalculateAverageDeliveryTime(histories),
                SuccessRate: CalculateSuccessRate(histories),
                GeneratedAt: DateTime.UtcNow
            );

            return Result.Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification statistics for user {UserId}", userId);
            _loggingService.LogError("Get notification statistics failed", ex, new { userId });
            return Result.Fail<NotificationStatistics>("Failed to get notification statistics", "GET_NOTIFICATION_STATISTICS_ERROR");
        }
    }

    public async Task<Result> RetryFailedNotificationAsync(Guid historyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _unitOfWork.ReadRepository<NotificationHistory>()
                .FirstOrDefaultAsync(
                    filter: h => h.Id == historyId && h.Status == Domain.Entities.NotificationStatus.Failed,
                    cancellationToken: cancellationToken);

            if (history == null)
            {
                return Result.Fail("Failed notification not found", "FAILED_NOTIFICATION_NOT_FOUND");
            }

            // Update retry count and reset status
            history.RetryCount++;
            history.Status = Domain.Entities.NotificationStatus.Pending;
            history.ErrorMessage = null;
            history.ErrorCode = null;
            history.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<NotificationHistory>().Update(history);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("NotificationRetryInitiated", new
            {
                historyId,
                history.RetryCount
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying failed notification {HistoryId}", historyId);
            _loggingService.LogError("Retry failed notification failed", ex, new { historyId });
            return Result.Fail("Failed to retry notification", "RETRY_NOTIFICATION_ERROR");
        }
    }

    public async Task<Result> CleanupOldNotificationsAsync(int daysToKeep = 90, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            var oldHistories = await _unitOfWork.ReadRepository<NotificationHistory>()
                .ListAsync(
                    filter: h => h.CreatedAt < cutoffDate,
                    cancellationToken: cancellationToken);

            foreach (var history in oldHistories)
            {
                _unitOfWork.Repository<NotificationHistory>().Remove(history);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("OldNotificationsCleanedUp", new
            {
                DeletedCount = oldHistories.Count,
                CutoffDate = cutoffDate
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old notifications");
            _loggingService.LogError("Cleanup old notifications failed", ex);
            return Result.Fail("Failed to cleanup old notifications", "CLEANUP_NOTIFICATIONS_ERROR");
        }
    }

    #region Helper Methods

    private TimeSpan? CalculateAverageDeliveryTime(IEnumerable<NotificationHistory> histories)
    {
        var deliveredHistories = histories
            .Where(h => h.SentAt.HasValue && h.DeliveredAt.HasValue)
            .ToList();

        if (!deliveredHistories.Any())
            return null;

        var totalDeliveryTime = deliveredHistories
            .Sum(h => (h.DeliveredAt!.Value - h.SentAt!.Value).TotalMilliseconds);

        return TimeSpan.FromMilliseconds(totalDeliveryTime / deliveredHistories.Count);
    }

    private decimal CalculateSuccessRate(IEnumerable<NotificationHistory> histories)
    {
        var historyList = histories.ToList();
        var totalNotifications = historyList.Count;
        if (totalNotifications == 0)
            return 0;

        var successfulNotifications = historyList.Count(h =>
            h.Status == Domain.Entities.NotificationStatus.Sent ||
            h.Status == Domain.Entities.NotificationStatus.Delivered);

        return Math.Round((decimal)successfulNotifications / totalNotifications * 100, 2);
    }

    #endregion
}

