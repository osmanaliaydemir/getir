using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.UserPreferences;

public class UserPreferencesService : IUserPreferencesService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserPreferencesService> _logger;

    public UserPreferencesService(
        IUnitOfWork unitOfWork,
        ILogger<UserPreferencesService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserNotificationPreferencesResponse>> GetUserPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await _unitOfWork.ReadRepository<UserNotificationPreferences>()
                .FirstOrDefaultAsync(p => p.UserId == userId, null, cancellationToken);

            if (preferences == null)
            {
                return Result.Fail<UserNotificationPreferencesResponse>(
                    "User preferences not found",
                    ErrorCodes.NOT_FOUND);
            }

            return Result.Ok(MapToResponse(preferences));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user preferences for user {UserId}", userId);
            return Result.Fail<UserNotificationPreferencesResponse>(
                "Failed to get user preferences",
                ErrorCodes.INTERNAL_ERROR);
        }
    }

    public async Task<Result<UserNotificationPreferencesResponse>> GetOrCreateUserPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await _unitOfWork.ReadRepository<UserNotificationPreferences>()
                .FirstOrDefaultAsync(p => p.UserId == userId, null, cancellationToken);

            if (preferences == null)
            {
                // Create default preferences
                preferences = new UserNotificationPreferences
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<UserNotificationPreferences>().AddAsync(preferences);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Created default preferences for user {UserId}", userId);
            }

            return Result.Ok(MapToResponse(preferences));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting or creating user preferences for user {UserId}", userId);
            return Result.Fail<UserNotificationPreferencesResponse>(
                "Failed to get or create user preferences",
                ErrorCodes.INTERNAL_ERROR);
        }
    }

    public async Task<Result<UserNotificationPreferencesResponse>> UpdateUserPreferencesAsync(
        Guid userId,
        UpdateUserNotificationPreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await _unitOfWork.ReadRepository<UserNotificationPreferences>()
                .FirstOrDefaultAsync(p => p.UserId == userId, null, cancellationToken);

            if (preferences == null)
            {
                // Create new preferences
                preferences = new UserNotificationPreferences
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<UserNotificationPreferences>().AddAsync(preferences);
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
            
            // Merchant Portal fields
            if (request.SoundEnabled.HasValue) preferences.SoundEnabled = request.SoundEnabled.Value;
            if (request.DesktopNotifications.HasValue) preferences.DesktopNotifications = request.DesktopNotifications.Value;
            if (!string.IsNullOrEmpty(request.NotificationSound)) preferences.NotificationSound = request.NotificationSound;
            
            if (request.NewOrderNotifications.HasValue) preferences.NewOrderNotifications = request.NewOrderNotifications.Value;
            if (request.StatusChangeNotifications.HasValue) preferences.StatusChangeNotifications = request.StatusChangeNotifications.Value;
            if (request.CancellationNotifications.HasValue) preferences.CancellationNotifications = request.CancellationNotifications.Value;
            
            // Time preferences
            if (request.QuietStartTime.HasValue) preferences.QuietStartTime = request.QuietStartTime;
            if (request.QuietEndTime.HasValue) preferences.QuietEndTime = request.QuietEndTime;
            if (request.RespectQuietHours.HasValue) preferences.RespectQuietHours = request.RespectQuietHours.Value;
            
            if (!string.IsNullOrEmpty(request.Language)) preferences.Language = request.Language;
            
            preferences.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated preferences for user {UserId}", userId);

            return Result.Ok(MapToResponse(preferences));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences for user {UserId}", userId);
            return Result.Fail<UserNotificationPreferencesResponse>(
                "Failed to update user preferences",
                ErrorCodes.INTERNAL_ERROR);
        }
    }

    public async Task<Result<MerchantNotificationPreferencesResponse>> GetMerchantPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await GetOrCreateUserPreferencesAsync(userId, cancellationToken);
            
            if (!result.Success)
            {
                return Result.Fail<MerchantNotificationPreferencesResponse>(
                    result.Error ?? "Failed to get preferences",
                    result.ErrorCode);
            }

            var preferences = result.Value!;
            
            return Result.Ok(
                new MerchantNotificationPreferencesResponse
                {
                    SoundEnabled = preferences.SoundEnabled,
                    DesktopNotifications = preferences.DesktopNotifications,
                    EmailNotifications = preferences.EmailEnabled,
                    NewOrderNotifications = preferences.NewOrderNotifications,
                    StatusChangeNotifications = preferences.StatusChangeNotifications,
                    CancellationNotifications = preferences.CancellationNotifications,
                    DoNotDisturbEnabled = preferences.RespectQuietHours,
                    DoNotDisturbStart = preferences.QuietStartTime?.ToString(@"hh\:mm"),
                    DoNotDisturbEnd = preferences.QuietEndTime?.ToString(@"hh\:mm"),
                    NotificationSound = preferences.NotificationSound
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchant preferences for user {UserId}", userId);
            return Result.Fail<MerchantNotificationPreferencesResponse>(
                "Failed to get merchant preferences",
                ErrorCodes.INTERNAL_ERROR);
        }
    }

    public async Task<Result<MerchantNotificationPreferencesResponse>> UpdateMerchantPreferencesAsync(
        Guid userId,
        UpdateMerchantNotificationPreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Parse time strings
            TimeSpan? quietStart = null;
            TimeSpan? quietEnd = null;
            
            if (!string.IsNullOrEmpty(request.DoNotDisturbStart) && 
                TimeSpan.TryParse(request.DoNotDisturbStart, out var start))
            {
                quietStart = start;
            }
            
            if (!string.IsNullOrEmpty(request.DoNotDisturbEnd) && 
                TimeSpan.TryParse(request.DoNotDisturbEnd, out var end))
            {
                quietEnd = end;
            }

            var updateRequest = new UpdateUserNotificationPreferencesRequest
            {
                SoundEnabled = request.SoundEnabled,
                DesktopNotifications = request.DesktopNotifications,
                EmailEnabled = request.EmailNotifications,
                NewOrderNotifications = request.NewOrderNotifications,
                StatusChangeNotifications = request.StatusChangeNotifications,
                CancellationNotifications = request.CancellationNotifications,
                RespectQuietHours = request.DoNotDisturbEnabled,
                QuietStartTime = quietStart,
                QuietEndTime = quietEnd,
                NotificationSound = request.NotificationSound
            };

            var result = await UpdateUserPreferencesAsync(userId, updateRequest, cancellationToken);
            
            if (!result.Success)
            {
                return Result.Fail<MerchantNotificationPreferencesResponse>(
                    result.Error ?? "Failed to update preferences",
                    result.ErrorCode);
            }

            // Return simplified response
            return await GetMerchantPreferencesAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating merchant preferences for user {UserId}", userId);
            return Result.Fail<MerchantNotificationPreferencesResponse>(
                "Failed to update merchant preferences",
                ErrorCodes.INTERNAL_ERROR);
        }
    }

    public async Task<Result> DeleteUserPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await _unitOfWork.ReadRepository<UserNotificationPreferences>()
                .FirstOrDefaultAsync(p => p.UserId == userId, null, cancellationToken: cancellationToken);

            if (preferences == null)
            {
                return Result.Fail("User preferences not found", ErrorCodes.NOT_FOUND);
            }

            await _unitOfWork.Repository<UserNotificationPreferences>().DeleteAsync(preferences);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted preferences for user {UserId}", userId);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user preferences for user {UserId}", userId);
            return Result.Fail("Failed to delete user preferences", ErrorCodes.INTERNAL_ERROR);
        }
    }

    private static UserNotificationPreferencesResponse MapToResponse(UserNotificationPreferences entity)
    {
        return new UserNotificationPreferencesResponse
        {
            Id = entity.Id,
            UserId = entity.UserId,
            EmailEnabled = entity.EmailEnabled,
            EmailOrderUpdates = entity.EmailOrderUpdates,
            EmailPromotions = entity.EmailPromotions,
            EmailNewsletter = entity.EmailNewsletter,
            EmailSecurityAlerts = entity.EmailSecurityAlerts,
            SmsEnabled = entity.SmsEnabled,
            SmsOrderUpdates = entity.SmsOrderUpdates,
            SmsPromotions = entity.SmsPromotions,
            SmsSecurityAlerts = entity.SmsSecurityAlerts,
            PushEnabled = entity.PushEnabled,
            PushOrderUpdates = entity.PushOrderUpdates,
            PushPromotions = entity.PushPromotions,
            PushMerchantUpdates = entity.PushMerchantUpdates,
            PushSecurityAlerts = entity.PushSecurityAlerts,
            SoundEnabled = entity.SoundEnabled,
            DesktopNotifications = entity.DesktopNotifications,
            NotificationSound = entity.NotificationSound,
            NewOrderNotifications = entity.NewOrderNotifications,
            StatusChangeNotifications = entity.StatusChangeNotifications,
            CancellationNotifications = entity.CancellationNotifications,
            QuietStartTime = entity.QuietStartTime,
            QuietEndTime = entity.QuietEndTime,
            RespectQuietHours = entity.RespectQuietHours,
            Language = entity.Language,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}

