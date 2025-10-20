using Getir.Application.DTO;

namespace Getir.Application.Services.RealtimeTracking;

/// <summary>
/// Tracking ayarları servisi implementasyonu: mock data ile user/merchant ayarları, validasyon.
/// </summary>
public class TrackingSettingsService : ITrackingSettingsService
{
    private readonly List<TrackingSettingsDto> _mockSettings;

    public TrackingSettingsService()
    {
        // Mock tracking settings
        _mockSettings = new List<TrackingSettingsDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EnableLocationTracking = true,
                EnablePushNotifications = true,
                EnableSMSNotifications = true,
                EnableEmailNotifications = false,
                LocationUpdateInterval = 30,
                NotificationInterval = 300,
                LocationAccuracyThreshold = 100,
                EnableETAUpdates = true,
                ETAUpdateInterval = 60,
                EnableDelayAlerts = true,
                DelayThresholdMinutes = 15,
                EnableNearbyAlerts = true,
                NearbyDistanceMeters = 500,
                PreferredLanguage = "tr",
                TimeZone = "Europe/Istanbul",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                MerchantId = Guid.NewGuid(),
                EnableLocationTracking = true,
                EnablePushNotifications = true,
                EnableSMSNotifications = false,
                EnableEmailNotifications = true,
                LocationUpdateInterval = 60,
                NotificationInterval = 600,
                LocationAccuracyThreshold = 200,
                EnableETAUpdates = true,
                ETAUpdateInterval = 120,
                EnableDelayAlerts = true,
                DelayThresholdMinutes = 30,
                EnableNearbyAlerts = false,
                NearbyDistanceMeters = 1000,
                PreferredLanguage = "tr",
                TimeZone = "Europe/Istanbul",
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };
    }

    /// <summary>
    /// Kullanıcı tracking ayarlarını getirir (mock data).
    /// </summary>
    public Task<TrackingSettingsDto?> GetUserSettingsAsync(Guid userId)
    {
        var settings = _mockSettings.FirstOrDefault(s => s.UserId == userId);
        return Task.FromResult(settings);
    }

    /// <summary>
    /// Merchant tracking ayarlarını getirir (mock data).
    /// </summary>
    public Task<TrackingSettingsDto?> GetMerchantSettingsAsync(Guid merchantId)
    {
        var settings = _mockSettings.FirstOrDefault(s => s.MerchantId == merchantId);
        return Task.FromResult(settings);
    }

    /// <summary>
    /// Kullanıcı için tracking ayarları oluşturur (mock data).
    /// </summary>
    public Task<TrackingSettingsDto> CreateUserSettingsAsync(Guid userId, UpdateTrackingSettingsRequest request)
    {
        var settings = new TrackingSettingsDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EnableLocationTracking = request.EnableLocationTracking,
            EnablePushNotifications = request.EnablePushNotifications,
            EnableSMSNotifications = request.EnableSMSNotifications,
            EnableEmailNotifications = request.EnableEmailNotifications,
            LocationUpdateInterval = request.LocationUpdateInterval,
            NotificationInterval = request.NotificationInterval,
            LocationAccuracyThreshold = request.LocationAccuracyThreshold,
            EnableETAUpdates = request.EnableETAUpdates,
            ETAUpdateInterval = request.ETAUpdateInterval,
            EnableDelayAlerts = request.EnableDelayAlerts,
            DelayThresholdMinutes = request.DelayThresholdMinutes,
            EnableNearbyAlerts = request.EnableNearbyAlerts,
            NearbyDistanceMeters = request.NearbyDistanceMeters,
            PreferredLanguage = request.PreferredLanguage,
            TimeZone = request.TimeZone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockSettings.Add(settings);
        return Task.FromResult(settings);
    }

    /// <summary>
    /// Merchant için tracking ayarları oluşturur (mock data).
    /// </summary>
    public Task<TrackingSettingsDto> CreateMerchantSettingsAsync(Guid merchantId, UpdateTrackingSettingsRequest request)
    {
        var settings = new TrackingSettingsDto
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            EnableLocationTracking = request.EnableLocationTracking,
            EnablePushNotifications = request.EnablePushNotifications,
            EnableSMSNotifications = request.EnableSMSNotifications,
            EnableEmailNotifications = request.EnableEmailNotifications,
            LocationUpdateInterval = request.LocationUpdateInterval,
            NotificationInterval = request.NotificationInterval,
            LocationAccuracyThreshold = request.LocationAccuracyThreshold,
            EnableETAUpdates = request.EnableETAUpdates,
            ETAUpdateInterval = request.ETAUpdateInterval,
            EnableDelayAlerts = request.EnableDelayAlerts,
            DelayThresholdMinutes = request.DelayThresholdMinutes,
            EnableNearbyAlerts = request.EnableNearbyAlerts,
            NearbyDistanceMeters = request.NearbyDistanceMeters,
            PreferredLanguage = request.PreferredLanguage,
            TimeZone = request.TimeZone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockSettings.Add(settings);
        return Task.FromResult(settings);
    }

    /// <summary>
    /// Kullanıcı tracking ayarlarını günceller (mock data).
    /// </summary>
    public Task<TrackingSettingsDto> UpdateUserSettingsAsync(Guid userId, UpdateTrackingSettingsRequest request)
    {
        var settings = _mockSettings.FirstOrDefault(s => s.UserId == userId);
        if (settings == null)
        {
            throw new ArgumentException("User settings not found");
        }

        settings.EnableLocationTracking = request.EnableLocationTracking;
        settings.EnablePushNotifications = request.EnablePushNotifications;
        settings.EnableSMSNotifications = request.EnableSMSNotifications;
        settings.EnableEmailNotifications = request.EnableEmailNotifications;
        settings.LocationUpdateInterval = request.LocationUpdateInterval;
        settings.NotificationInterval = request.NotificationInterval;
        settings.LocationAccuracyThreshold = request.LocationAccuracyThreshold;
        settings.EnableETAUpdates = request.EnableETAUpdates;
        settings.ETAUpdateInterval = request.ETAUpdateInterval;
        settings.EnableDelayAlerts = request.EnableDelayAlerts;
        settings.DelayThresholdMinutes = request.DelayThresholdMinutes;
        settings.EnableNearbyAlerts = request.EnableNearbyAlerts;
        settings.NearbyDistanceMeters = request.NearbyDistanceMeters;
        settings.PreferredLanguage = request.PreferredLanguage;
        settings.TimeZone = request.TimeZone;
        settings.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult(settings);
    }

    /// <summary>
    /// Merchant tracking ayarlarını günceller (mock data).
    /// </summary>
    public Task<TrackingSettingsDto> UpdateMerchantSettingsAsync(Guid merchantId, UpdateTrackingSettingsRequest request)
    {
        var settings = _mockSettings.FirstOrDefault(s => s.MerchantId == merchantId);
        if (settings == null)
        {
            throw new ArgumentException("Merchant settings not found");
        }

        settings.EnableLocationTracking = request.EnableLocationTracking;
        settings.EnablePushNotifications = request.EnablePushNotifications;
        settings.EnableSMSNotifications = request.EnableSMSNotifications;
        settings.EnableEmailNotifications = request.EnableEmailNotifications;
        settings.LocationUpdateInterval = request.LocationUpdateInterval;
        settings.NotificationInterval = request.NotificationInterval;
        settings.LocationAccuracyThreshold = request.LocationAccuracyThreshold;
        settings.EnableETAUpdates = request.EnableETAUpdates;
        settings.ETAUpdateInterval = request.ETAUpdateInterval;
        settings.EnableDelayAlerts = request.EnableDelayAlerts;
        settings.DelayThresholdMinutes = request.DelayThresholdMinutes;
        settings.EnableNearbyAlerts = request.EnableNearbyAlerts;
        settings.NearbyDistanceMeters = request.NearbyDistanceMeters;
        settings.PreferredLanguage = request.PreferredLanguage;
        settings.TimeZone = request.TimeZone;
        settings.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult(settings);
    }

    /// <summary>
    /// Kullanıcı tracking ayarlarını siler (mock data).
    /// </summary>
    public Task<bool> DeleteUserSettingsAsync(Guid userId)
    {
        var settings = _mockSettings.FirstOrDefault(s => s.UserId == userId);
        if (settings != null)
        {
            _mockSettings.Remove(settings);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <summary>
    /// Merchant tracking ayarlarını siler (mock data).
    /// </summary>
    public Task<bool> DeleteMerchantSettingsAsync(Guid merchantId)
    {
        var settings = _mockSettings.FirstOrDefault(s => s.MerchantId == merchantId);
        if (settings != null)
        {
            _mockSettings.Remove(settings);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <summary>
    /// Varsayılan tracking ayarlarını getirir (mock data).
    /// </summary>
    public Task<TrackingSettingsDto> GetDefaultSettingsAsync()
    {
        var defaultSettings = new TrackingSettingsDto
        {
            Id = Guid.NewGuid(),
            EnableLocationTracking = true,
            EnablePushNotifications = true,
            EnableSMSNotifications = true,
            EnableEmailNotifications = false,
            LocationUpdateInterval = 30,
            NotificationInterval = 300,
            LocationAccuracyThreshold = 100,
            EnableETAUpdates = true,
            ETAUpdateInterval = 60,
            EnableDelayAlerts = true,
            DelayThresholdMinutes = 15,
            EnableNearbyAlerts = true,
            NearbyDistanceMeters = 500,
            PreferredLanguage = "tr",
            TimeZone = "Europe/Istanbul",
            CreatedAt = DateTime.UtcNow
        };

        return Task.FromResult(defaultSettings);
    }

    /// <summary>
    /// Tracking ayarlarını doğrular (aralık kontrolleri: 10-300s konum, 60-3600s bildirim, 10-1000m accuracy, 30-600s ETA, 5-120dk gecikme, 100-2000m yakınlık).
    /// </summary>
    public Task<bool> ValidateSettingsAsync(UpdateTrackingSettingsRequest request)
    {
        // Validate settings
        if (request.LocationUpdateInterval < 10 || request.LocationUpdateInterval > 300)
            return Task.FromResult(false);

        if (request.NotificationInterval < 60 || request.NotificationInterval > 3600)
            return Task.FromResult(false);

        if (request.LocationAccuracyThreshold < 10 || request.LocationAccuracyThreshold > 1000)
            return Task.FromResult(false);

        if (request.ETAUpdateInterval < 30 || request.ETAUpdateInterval > 600)
            return Task.FromResult(false);

        if (request.DelayThresholdMinutes < 5 || request.DelayThresholdMinutes > 120)
            return Task.FromResult(false);

        if (request.NearbyDistanceMeters < 100 || request.NearbyDistanceMeters > 2000)
            return Task.FromResult(false);

        return Task.FromResult(true);
    }

    /// <summary>
    /// Tüm kullanıcı ayarlarını getirir (mock data).
    /// </summary>
    public Task<List<TrackingSettingsDto>> GetAllUserSettingsAsync()
    {
        var userSettings = _mockSettings.Where(s => s.UserId.HasValue).ToList();
        return Task.FromResult(userSettings);
    }

    /// <summary>
    /// Tüm merchant ayarlarını getirir (mock data).
    /// </summary>
    public Task<List<TrackingSettingsDto>> GetAllMerchantSettingsAsync()
    {
        var merchantSettings = _mockSettings.Where(s => s.MerchantId.HasValue).ToList();
        return Task.FromResult(merchantSettings);
    }
}
