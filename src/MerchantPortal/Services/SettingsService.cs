using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public class SettingsService : ISettingsService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(IApiClient apiClient, ILogger<SettingsService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Bildirim tercihlerini getirir.
    /// </summary>
    /// <returns>Bildirim tercihleri</returns>
    public async Task<MerchantNotificationPreferencesDto?> GetNotificationPreferencesAsync()
    {
        try
        {
            var response = await _apiClient.GetAsync<MerchantNotificationPreferencesDto>(
                "api/v1/userpreferences/merchant");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification preferences");
            return null;
        }
    }

    /// <summary>
    /// Bildirim tercihlerini günceller.
    /// </summary>
    /// <param name="preferences">Bildirim tercihleri</param>
    /// <returns>Başarılı olup olmadığı</returns>
    public async Task<bool> UpdateNotificationPreferencesAsync(UpdateNotificationPreferencesDto preferences)
    {
        try
        {
            var response = await _apiClient.PutAsync<MerchantNotificationPreferencesDto>(
                "api/v1/userpreferences/merchant",
                preferences);

            return response != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification preferences");
            return false;
        }
    }
}

