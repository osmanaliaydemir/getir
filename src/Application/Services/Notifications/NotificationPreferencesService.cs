using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;
using NotificationChannel = Getir.Application.DTO.NotificationChannel;

namespace Getir.Application.Services.Notifications;

/// <summary>
/// Bildirim tercihleri servisi: email/sms/push tercihleri, sessiz saatler, kanal kontrolü.
/// </summary>
public class NotificationPreferencesService : BaseService, INotificationPreferencesService
{
    public NotificationPreferencesService(IUnitOfWork unitOfWork, ILogger<NotificationPreferencesService> logger, ILoggingService loggingService, ICacheService cacheService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
    }
    /// <summary>
    /// Kullanıcı bildirim tercihlerini getirir (yoksa varsayılan oluşturur).
    /// </summary>
    public async Task<Result<NotificationPreferencesResponse>> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await _unitOfWork.ReadRepository<UserNotificationPreferences>()
                .FirstOrDefaultAsync(
                    filter: p => p.UserId == userId,
                    cancellationToken: cancellationToken);

            if (preferences == null)
            {
                // Create default preferences if not exists
                var createResult = await CreateDefaultPreferencesAsync(userId, cancellationToken);
                if (!createResult.Success)
                {
                    return Result.Fail<NotificationPreferencesResponse>(createResult.Error ?? "Failed to create default preferences", createResult.ErrorCode ?? "CREATE_PREFERENCES_ERROR");
                }

                preferences = await _unitOfWork.ReadRepository<UserNotificationPreferences>()
                    .FirstOrDefaultAsync(
                        filter: p => p.UserId == userId,
                        cancellationToken: cancellationToken);
            }

            var response = new NotificationPreferencesResponse(
                preferences!.Id,
                preferences.UserId,
                preferences.EmailEnabled,
                preferences.EmailOrderUpdates,
                preferences.EmailPromotions,
                preferences.EmailNewsletter,
                preferences.EmailSecurityAlerts,
                preferences.SmsEnabled,
                preferences.SmsOrderUpdates,
                preferences.SmsPromotions,
                preferences.SmsSecurityAlerts,
                preferences.PushEnabled,
                preferences.PushOrderUpdates,
                preferences.PushPromotions,
                preferences.PushMerchantUpdates,
                preferences.PushSecurityAlerts,
                preferences.QuietStartTime,
                preferences.QuietEndTime,
                preferences.RespectQuietHours,
                preferences.Language,
                preferences.CreatedAt,
                preferences.UpdatedAt);

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification preferences for user {UserId}", userId);
            _loggingService.LogError("Get notification preferences failed", ex, new { userId });
            return Result.Fail<NotificationPreferencesResponse>("Failed to get notification preferences", "GET_PREFERENCES_ERROR");
        }
    }
    /// <summary>
    /// Kullanıcı bildirim tercihlerini günceller (partial update).
    /// </summary>
    public async Task<Result> UpdateUserPreferencesAsync(Guid userId, UpdateNotificationPreferencesRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await _unitOfWork.ReadRepository<UserNotificationPreferences>()
                .FirstOrDefaultAsync(
                    filter: p => p.UserId == userId,
                    cancellationToken: cancellationToken);

            if (preferences == null)
            {
                return Result.Fail("User notification preferences not found", "PREFERENCES_NOT_FOUND");
            }

            // Update only provided fields
            if (request.EmailEnabled.HasValue) preferences.EmailEnabled = request.EmailEnabled.Value;
            if (request.EmailOrderUpdates.HasValue) preferences.EmailOrderUpdates = request.EmailOrderUpdates.Value;
            if (request.EmailPromotions.HasValue) preferences.EmailPromotions = request.EmailPromotions.Value;
            if (request.EmailNewsletter.HasValue) preferences.EmailNewsletter = request.EmailNewsletter.Value;
            if (request.EmailSecurityAlerts.HasValue) preferences.EmailSecurityAlerts = request.EmailSecurityAlerts.Value;

            if (request.SmsEnabled.HasValue) preferences.SmsEnabled = request.SmsEnabled.Value;
            if (request.SmsOrderUpdates.HasValue) preferences.SmsOrderUpdates = request.SmsOrderUpdates.Value;
            if (request.SmsPromotions.HasValue) preferences.SmsPromotions = request.SmsPromotions.Value;
            if (request.SmsSecurityAlerts.HasValue) preferences.SmsSecurityAlerts = request.SmsSecurityAlerts.Value;

            if (request.PushEnabled.HasValue) preferences.PushEnabled = request.PushEnabled.Value;
            if (request.PushOrderUpdates.HasValue) preferences.PushOrderUpdates = request.PushOrderUpdates.Value;
            if (request.PushPromotions.HasValue) preferences.PushPromotions = request.PushPromotions.Value;
            if (request.PushMerchantUpdates.HasValue) preferences.PushMerchantUpdates = request.PushMerchantUpdates.Value;
            if (request.PushSecurityAlerts.HasValue) preferences.PushSecurityAlerts = request.PushSecurityAlerts.Value;

            if (request.QuietStartTime.HasValue) preferences.QuietStartTime = request.QuietStartTime.Value;
            if (request.QuietEndTime.HasValue) preferences.QuietEndTime = request.QuietEndTime.Value;
            if (request.RespectQuietHours.HasValue) preferences.RespectQuietHours = request.RespectQuietHours.Value;

            if (!string.IsNullOrEmpty(request.Language)) preferences.Language = request.Language;

            preferences.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<UserNotificationPreferences>().Update(preferences);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("NotificationPreferencesUpdated", new
            {
                UserId = userId,
                UpdatedFields = GetUpdatedFields(request)
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification preferences for user {UserId}", userId);
            _loggingService.LogError("Update notification preferences failed", ex, new { userId });
            return Result.Fail("Failed to update notification preferences", "UPDATE_PREFERENCES_ERROR");
        }
    }
    /// <summary>
    /// Kullanıcı bildirim tercihlerini varsayılana sıfırlar.
    /// </summary>
    public async Task<Result> ResetToDefaultsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await _unitOfWork.ReadRepository<UserNotificationPreferences>()
                .FirstOrDefaultAsync(
                    filter: p => p.UserId == userId,
                    cancellationToken: cancellationToken);

            if (preferences == null)
            {
                return Result.Fail("User notification preferences not found", "PREFERENCES_NOT_FOUND");
            }

            // Reset to default values
            preferences.EmailEnabled = true;
            preferences.EmailOrderUpdates = true;
            preferences.EmailPromotions = true;
            preferences.EmailNewsletter = true;
            preferences.EmailSecurityAlerts = true;

            preferences.SmsEnabled = true;
            preferences.SmsOrderUpdates = true;
            preferences.SmsPromotions = false;
            preferences.SmsSecurityAlerts = true;

            preferences.PushEnabled = true;
            preferences.PushOrderUpdates = true;
            preferences.PushPromotions = true;
            preferences.PushMerchantUpdates = true;
            preferences.PushSecurityAlerts = true;

            preferences.QuietStartTime = new TimeSpan(22, 0, 0); // 22:00
            preferences.QuietEndTime = new TimeSpan(8, 0, 0);    // 08:00
            preferences.RespectQuietHours = true;

            preferences.Language = "tr-TR";
            preferences.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<UserNotificationPreferences>().Update(preferences);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("NotificationPreferencesReset", new { UserId = userId });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting notification preferences for user {UserId}", userId);
            _loggingService.LogError("Reset notification preferences failed", ex, new { userId });
            return Result.Fail("Failed to reset notification preferences", "RESET_PREFERENCES_ERROR");
        }
    }
    /// <summary>
    /// Yeni kullanıcı için varsayılan bildirim tercihleri oluşturur.
    /// </summary>
    public async Task<Result> CreateDefaultPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingPreferences = await _unitOfWork.ReadRepository<UserNotificationPreferences>()
                .FirstOrDefaultAsync(
                    filter: p => p.UserId == userId,
                    cancellationToken: cancellationToken);

            if (existingPreferences != null)
            {
                return Result.Fail("User notification preferences already exist", "PREFERENCES_ALREADY_EXIST");
            }

            var preferences = new UserNotificationPreferences
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EmailEnabled = true,
                EmailOrderUpdates = true,
                EmailPromotions = true,
                EmailNewsletter = true,
                EmailSecurityAlerts = true,
                SmsEnabled = true,
                SmsOrderUpdates = true,
                SmsPromotions = false,
                SmsSecurityAlerts = true,
                PushEnabled = true,
                PushOrderUpdates = true,
                PushPromotions = true,
                PushMerchantUpdates = true,
                PushSecurityAlerts = true,
                QuietStartTime = new TimeSpan(22, 0, 0), // 22:00
                QuietEndTime = new TimeSpan(8, 0, 0),    // 08:00
                RespectQuietHours = true,
                Language = "tr-TR",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<UserNotificationPreferences>().AddAsync(preferences, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("DefaultNotificationPreferencesCreated", new { UserId = userId });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating default notification preferences for user {UserId}", userId);
            _loggingService.LogError("Create default notification preferences failed", ex, new { userId });
            return Result.Fail("Failed to create default notification preferences", "CREATE_PREFERENCES_ERROR");
        }
    }
    /// <summary>
    /// Bildirim tercihleri özetini getirir (aktif kanallar, dil).
    /// </summary>
    public async Task<Result<NotificationPreferencesSummary>> GetPreferencesSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await _unitOfWork.ReadRepository<UserNotificationPreferences>()
                .FirstOrDefaultAsync(
                    filter: p => p.UserId == userId,
                    cancellationToken: cancellationToken);

            if (preferences == null)
            {
                return Result.Fail<NotificationPreferencesSummary>("User notification preferences not found", "PREFERENCES_NOT_FOUND");
            }

            var summary = new NotificationPreferencesSummary(
                preferences.EmailEnabled,
                preferences.SmsEnabled,
                preferences.PushEnabled,
                preferences.RespectQuietHours,
                preferences.Language,
                GetActiveChannelsCount(preferences));

            return Result.Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification preferences summary for user {UserId}", userId);
            _loggingService.LogError("Get notification preferences summary failed", ex, new { userId });
            return Result.Fail<NotificationPreferencesSummary>("Failed to get notification preferences summary", "GET_PREFERENCES_SUMMARY_ERROR");
        }
    }
    /// <summary>
    /// Kullanıcının belirtilen tip ve kanal için bildirim alıp alamayacağını kontrol eder (kanal, tip, sessiz saatler).
    /// </summary>
    public async Task<Result<bool>> CanReceiveNotificationAsync(Guid userId, NotificationType type, NotificationChannel channel, CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await _unitOfWork.ReadRepository<UserNotificationPreferences>()
                .FirstOrDefaultAsync(
                    filter: p => p.UserId == userId,
                    cancellationToken: cancellationToken);

            if (preferences == null)
            {
                return Result.Ok(true); // Default to allowing notifications if preferences don't exist
            }

            // Check if channel is enabled
            var channelEnabled = channel switch
            {
                NotificationChannel.Email => preferences.EmailEnabled,
                NotificationChannel.Sms => preferences.SmsEnabled,
                NotificationChannel.Push => preferences.PushEnabled,
                _ => false
            };

            if (!channelEnabled)
            {
                return Result.Ok(false);
            }

            // Check if specific notification type is enabled for the channel
            var typeEnabled = (type, channel) switch
            {
                (NotificationType.OrderUpdate, NotificationChannel.Email) => preferences.EmailOrderUpdates,
                (NotificationType.OrderUpdate, NotificationChannel.Sms) => preferences.SmsOrderUpdates,
                (NotificationType.OrderUpdate, NotificationChannel.Push) => preferences.PushOrderUpdates,
                (NotificationType.Promotion, NotificationChannel.Email) => preferences.EmailPromotions,
                (NotificationType.Promotion, NotificationChannel.Sms) => preferences.SmsPromotions,
                (NotificationType.Promotion, NotificationChannel.Push) => preferences.PushPromotions,
                (NotificationType.SecurityAlert, NotificationChannel.Email) => preferences.EmailSecurityAlerts,
                (NotificationType.SecurityAlert, NotificationChannel.Sms) => preferences.SmsSecurityAlerts,
                (NotificationType.SecurityAlert, NotificationChannel.Push) => preferences.PushSecurityAlerts,
                (NotificationType.MerchantUpdate, NotificationChannel.Push) => preferences.PushMerchantUpdates,
                _ => true // Default to allowing for unknown types
            };

            if (!typeEnabled)
            {
                return Result.Ok(false);
            }

            // Check quiet hours
            if (preferences.RespectQuietHours)
            {
                var isWithinQuietHours = await IsWithinQuietHoursAsync(userId, cancellationToken);
                if (isWithinQuietHours.Success && isWithinQuietHours.Value)
                {
                    return Result.Ok(false);
                }
            }

            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user can receive notification {UserId}, {Type}, {Channel}", userId, type, channel);
            _loggingService.LogError("Check notification eligibility failed", ex, new { userId, type, channel });
            return Result.Fail<bool>("Failed to check notification eligibility", "CHECK_ELIGIBILITY_ERROR");
        }
    }
    /// <summary>
    /// Geçerli zamanın sessiz saatler içinde olup olmadığını kontrol eder (gece yarısı geçişini handle eder).
    /// </summary>
    public async Task<Result<bool>> IsWithinQuietHoursAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await _unitOfWork.ReadRepository<UserNotificationPreferences>()
                .FirstOrDefaultAsync(
                    filter: p => p.UserId == userId,
                    cancellationToken: cancellationToken);

            if (preferences?.QuietStartTime == null || preferences.QuietEndTime == null)
            {
                return Result.Ok(false);
            }

            var currentTime = DateTime.Now.TimeOfDay;
            var startTime = preferences.QuietStartTime.Value;
            var endTime = preferences.QuietEndTime.Value;

            // Handle quiet hours that span midnight (e.g., 22:00 to 08:00)
            if (startTime > endTime)
            {
                var isWithinQuietHours = currentTime >= startTime || currentTime <= endTime;
                return Result.Ok(isWithinQuietHours);
            }
            else
            {
                var isWithinQuietHours = currentTime >= startTime && currentTime <= endTime;
                return Result.Ok(isWithinQuietHours);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking quiet hours for user {UserId}", userId);
            _loggingService.LogError("Check quiet hours failed", ex, new { userId });
            return Result.Fail<bool>("Failed to check quiet hours", "CHECK_QUIET_HOURS_ERROR");
        }
    }
    /// <summary>
    /// Toplu bildirim tercihi günceller (birden fazla kullanıcı için).
    /// </summary>
    public async Task<Result> BulkUpdatePreferencesAsync(BulkNotificationPreferencesRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var results = new List<Result>();

            foreach (var userId in request.UserIds)
            {
                var result = await UpdateUserPreferencesAsync(userId, request.Preferences, cancellationToken);
                results.Add(result);
            }

            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count - successCount;

            _loggingService.LogBusinessEvent("BulkNotificationPreferencesUpdated", new
            {
                TotalCount = results.Count,
                SuccessCount = successCount,
                FailureCount = failureCount
            });

            if (failureCount == 0)
            {
                return Result.Ok();
            }

            return Result.Fail($"Bulk update completed with {failureCount} failures", "BULK_UPDATE_PARTIAL_FAILURE");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating notification preferences");
            _loggingService.LogError("Bulk update notification preferences failed", ex);
            return Result.Fail("Failed to bulk update notification preferences", "BULK_UPDATE_PREFERENCES_ERROR");
        }
    }
    /// <summary>
    /// Belirtilen bildirim tipini alabilen kullanıcıları getirir (tercih kontrolü).
    /// </summary>
    public async Task<Result<IEnumerable<Guid>>> GetEligibleUsersAsync(NotificationType type, NotificationChannel channel, CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await _unitOfWork.ReadRepository<UserNotificationPreferences>()
                .ListAsync(
                    filter: p => IsEligibleForNotification(p, type, channel),
                    cancellationToken: cancellationToken);

            var eligibleUserIds = preferences.Select(p => p.UserId).ToList();

            _loggingService.LogBusinessEvent("EligibleUsersRetrieved", new
            {
                NotificationType = type,
                Channel = channel,
                EligibleUserCount = eligibleUserIds.Count
            });

            return Result.Ok<IEnumerable<Guid>>(eligibleUserIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting eligible users for notification {Type}, {Channel}", type, channel);
            _loggingService.LogError("Get eligible users failed", ex, new { type, channel });
            return Result.Fail<IEnumerable<Guid>>("Failed to get eligible users", "GET_ELIGIBLE_USERS_ERROR");
        }
    }
    #region Helper Methods
    private List<string> GetUpdatedFields(UpdateNotificationPreferencesRequest request)
    {
        var updatedFields = new List<string>();

        if (request.EmailEnabled.HasValue) updatedFields.Add("EmailEnabled");
        if (request.EmailOrderUpdates.HasValue) updatedFields.Add("EmailOrderUpdates");
        if (request.EmailPromotions.HasValue) updatedFields.Add("EmailPromotions");
        if (request.EmailNewsletter.HasValue) updatedFields.Add("EmailNewsletter");
        if (request.EmailSecurityAlerts.HasValue) updatedFields.Add("EmailSecurityAlerts");
        if (request.SmsEnabled.HasValue) updatedFields.Add("SmsEnabled");
        if (request.SmsOrderUpdates.HasValue) updatedFields.Add("SmsOrderUpdates");
        if (request.SmsPromotions.HasValue) updatedFields.Add("SmsPromotions");
        if (request.SmsSecurityAlerts.HasValue) updatedFields.Add("SmsSecurityAlerts");
        if (request.PushEnabled.HasValue) updatedFields.Add("PushEnabled");
        if (request.PushOrderUpdates.HasValue) updatedFields.Add("PushOrderUpdates");
        if (request.PushPromotions.HasValue) updatedFields.Add("PushPromotions");
        if (request.PushMerchantUpdates.HasValue) updatedFields.Add("PushMerchantUpdates");
        if (request.PushSecurityAlerts.HasValue) updatedFields.Add("PushSecurityAlerts");
        if (request.QuietStartTime.HasValue) updatedFields.Add("QuietStartTime");
        if (request.QuietEndTime.HasValue) updatedFields.Add("QuietEndTime");
        if (request.RespectQuietHours.HasValue) updatedFields.Add("RespectQuietHours");
        if (!string.IsNullOrEmpty(request.Language)) updatedFields.Add("Language");

        return updatedFields;
    }
    private int GetActiveChannelsCount(UserNotificationPreferences preferences)
    {
        var count = 0;
        if (preferences.EmailEnabled) count++;
        if (preferences.SmsEnabled) count++;
        if (preferences.PushEnabled) count++;
        return count;
    }
    private bool IsEligibleForNotification(UserNotificationPreferences preferences, NotificationType type, NotificationChannel channel)
    {
        // Check if channel is enabled
        var channelEnabled = channel switch
        {
            NotificationChannel.Email => preferences.EmailEnabled,
            NotificationChannel.Sms => preferences.SmsEnabled,
            NotificationChannel.Push => preferences.PushEnabled,
            _ => false
        };

        if (!channelEnabled) return false;

        // Check if specific notification type is enabled for the channel
        var typeEnabled = (type, channel) switch
        {
            (NotificationType.OrderUpdate, NotificationChannel.Email) => preferences.EmailOrderUpdates,
            (NotificationType.OrderUpdate, NotificationChannel.Sms) => preferences.SmsOrderUpdates,
            (NotificationType.OrderUpdate, NotificationChannel.Push) => preferences.PushOrderUpdates,
            (NotificationType.Promotion, NotificationChannel.Email) => preferences.EmailPromotions,
            (NotificationType.Promotion, NotificationChannel.Sms) => preferences.SmsPromotions,
            (NotificationType.Promotion, NotificationChannel.Push) => preferences.PushPromotions,
            (NotificationType.SecurityAlert, NotificationChannel.Email) => preferences.EmailSecurityAlerts,
            (NotificationType.SecurityAlert, NotificationChannel.Sms) => preferences.SmsSecurityAlerts,
            (NotificationType.SecurityAlert, NotificationChannel.Push) => preferences.PushSecurityAlerts,
            (NotificationType.MerchantUpdate, NotificationChannel.Push) => preferences.PushMerchantUpdates,
            _ => true // Default to allowing for unknown types
        };

        return typeEnabled;
    }
    #endregion
}

/// <summary>
/// Bulk notification preferences request
/// </summary>
public record BulkNotificationPreferencesRequest(IEnumerable<Guid> UserIds, UpdateNotificationPreferencesRequest Preferences);
