using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Notifications;

/// <summary>
/// Notification preferences service interface
/// </summary>
public interface INotificationPreferencesService
{
    /// <summary>
    /// Get user notification preferences
    /// </summary>
    Task<Result<NotificationPreferencesResponse>> GetUserPreferencesAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user notification preferences
    /// </summary>
    Task<Result> UpdateUserPreferencesAsync(
        Guid userId, 
        UpdateNotificationPreferencesRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset user notification preferences to defaults
    /// </summary>
    Task<Result> ResetToDefaultsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create default notification preferences for new user
    /// </summary>
    Task<Result> CreateDefaultPreferencesAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get notification preferences summary
    /// </summary>
    Task<Result<NotificationPreferencesSummary>> GetPreferencesSummaryAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user can receive notification for specific type and channel
    /// </summary>
    Task<Result<bool>> CanReceiveNotificationAsync(
        Guid userId, 
        NotificationType type, 
        NotificationChannel channel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if current time is within quiet hours
    /// </summary>
    Task<Result<bool>> IsWithinQuietHoursAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update notification preferences
    /// </summary>
    Task<Result> BulkUpdatePreferencesAsync(
        BulkNotificationPreferencesRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get users who can receive specific notification type
    /// </summary>
    Task<Result<IEnumerable<Guid>>> GetEligibleUsersAsync(
        NotificationType type, 
        NotificationChannel channel,
        CancellationToken cancellationToken = default);
}
