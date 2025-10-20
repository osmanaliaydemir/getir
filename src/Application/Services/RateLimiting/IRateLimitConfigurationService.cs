using Getir.Application.DTO;

namespace Getir.Application.Services.RateLimiting;

/// <summary>
/// Rate limit konfigürasyon servisi: rate limit kurallarının yönetimi (endpoint/user/IP bazlı).
/// </summary>
public interface IRateLimitConfigurationService
{
    /// <summary>Tüm rate limit konfigürasyonlarını getirir.</summary>
    Task<List<RateLimitConfigurationDto>> GetAllConfigurationsAsync();
    /// <summary>Konfigürasyonu ID ile getirir.</summary>
    Task<RateLimitConfigurationDto?> GetConfigurationByIdAsync(Guid id);
    /// <summary>Yeni rate limit konfigürasyonu oluşturur (validasyon).</summary>
    Task<RateLimitConfigurationDto> CreateConfigurationAsync(CreateRateLimitConfigurationRequest request);
    /// <summary>Konfigürasyonu günceller.</summary>
    Task<RateLimitConfigurationDto> UpdateConfigurationAsync(Guid id, UpdateRateLimitConfigurationRequest request);
    /// <summary>Konfigürasyonu siler.</summary>
    Task<bool> DeleteConfigurationAsync(Guid id);
    /// <summary>Konfigürasyonu etkinleştirir.</summary>
    Task<bool> EnableConfigurationAsync(Guid id);
    /// <summary>Konfigürasyonu devre dışı bırakır.</summary>
    Task<bool> DisableConfigurationAsync(Guid id);
    /// <summary>Aktif konfigürasyonları getirir.</summary>
    Task<List<RateLimitConfigurationDto>> GetActiveConfigurationsAsync();
    /// <summary>Endpoint için konfigürasyonu getirir (priority sıralamalı).</summary>
    Task<RateLimitConfigurationDto?> GetConfigurationByEndpointAsync(string endpoint, string httpMethod);
    /// <summary>Konfigürasyonu doğrular (limit/isim kontrolü).</summary>
    Task<bool> ValidateConfigurationAsync(CreateRateLimitConfigurationRequest request);
    /// <summary>Konfigürasyonu test eder.</summary>
    Task<bool> TestConfigurationAsync(Guid configurationId, RateLimitCheckRequest testRequest);
}
