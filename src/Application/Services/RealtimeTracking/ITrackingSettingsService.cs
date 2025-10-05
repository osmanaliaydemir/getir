using Getir.Application.DTO;

namespace Getir.Application.Services.RealtimeTracking;

public interface ITrackingSettingsService
{
    Task<TrackingSettingsDto?> GetUserSettingsAsync(Guid userId);
    Task<TrackingSettingsDto?> GetMerchantSettingsAsync(Guid merchantId);
    Task<TrackingSettingsDto> CreateUserSettingsAsync(Guid userId, UpdateTrackingSettingsRequest request);
    Task<TrackingSettingsDto> CreateMerchantSettingsAsync(Guid merchantId, UpdateTrackingSettingsRequest request);
    Task<TrackingSettingsDto> UpdateUserSettingsAsync(Guid userId, UpdateTrackingSettingsRequest request);
    Task<TrackingSettingsDto> UpdateMerchantSettingsAsync(Guid merchantId, UpdateTrackingSettingsRequest request);
    Task<bool> DeleteUserSettingsAsync(Guid userId);
    Task<bool> DeleteMerchantSettingsAsync(Guid merchantId);
    Task<TrackingSettingsDto> GetDefaultSettingsAsync();
    Task<bool> ValidateSettingsAsync(UpdateTrackingSettingsRequest request);
    Task<List<TrackingSettingsDto>> GetAllUserSettingsAsync();
    Task<List<TrackingSettingsDto>> GetAllMerchantSettingsAsync();
}
