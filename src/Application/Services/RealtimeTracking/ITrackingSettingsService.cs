using Getir.Application.DTO;

namespace Getir.Application.Services.RealtimeTracking;

/// <summary>
/// Tracking ayarları servisi: konum güncelleme aralığı, bildirim tercihleri, sessiz saatler, ETA ayarları.
/// </summary>
public interface ITrackingSettingsService
{
    /// <summary>Kullanıcı tracking ayarlarını getirir.</summary>
    Task<TrackingSettingsDto?> GetUserSettingsAsync(Guid userId);
    /// <summary>Merchant tracking ayarlarını getirir.</summary>
    Task<TrackingSettingsDto?> GetMerchantSettingsAsync(Guid merchantId);
    /// <summary>Kullanıcı için tracking ayarları oluşturur.</summary>
    Task<TrackingSettingsDto> CreateUserSettingsAsync(Guid userId, UpdateTrackingSettingsRequest request);
    /// <summary>Merchant için tracking ayarları oluşturur.</summary>
    Task<TrackingSettingsDto> CreateMerchantSettingsAsync(Guid merchantId, UpdateTrackingSettingsRequest request);
    /// <summary>Kullanıcı tracking ayarlarını günceller.</summary>
    Task<TrackingSettingsDto> UpdateUserSettingsAsync(Guid userId, UpdateTrackingSettingsRequest request);
    /// <summary>Merchant tracking ayarlarını günceller.</summary>
    Task<TrackingSettingsDto> UpdateMerchantSettingsAsync(Guid merchantId, UpdateTrackingSettingsRequest request);
    /// <summary>Kullanıcı tracking ayarlarını siler.</summary>
    Task<bool> DeleteUserSettingsAsync(Guid userId);
    /// <summary>Merchant tracking ayarlarını siler.</summary>
    Task<bool> DeleteMerchantSettingsAsync(Guid merchantId);
    /// <summary>Varsayılan tracking ayarlarını getirir.</summary>
    Task<TrackingSettingsDto> GetDefaultSettingsAsync();
    /// <summary>Tracking ayarlarını doğrular (aralık kontrolleri).</summary>
    Task<bool> ValidateSettingsAsync(UpdateTrackingSettingsRequest request);
    /// <summary>Tüm kullanıcı ayarlarını getirir.</summary>
    Task<List<TrackingSettingsDto>> GetAllUserSettingsAsync();
    /// <summary>Tüm merchant ayarlarını getirir.</summary>
    Task<List<TrackingSettingsDto>> GetAllMerchantSettingsAsync();
}
