using Getir.Application.DTO;

namespace Getir.Application.Services.RateLimiting;

public interface IRateLimitConfigurationService
{
    Task<List<RateLimitConfigurationDto>> GetAllConfigurationsAsync();
    Task<RateLimitConfigurationDto?> GetConfigurationByIdAsync(Guid id);
    Task<RateLimitConfigurationDto> CreateConfigurationAsync(CreateRateLimitConfigurationRequest request);
    Task<RateLimitConfigurationDto> UpdateConfigurationAsync(Guid id, UpdateRateLimitConfigurationRequest request);
    Task<bool> DeleteConfigurationAsync(Guid id);
    Task<bool> EnableConfigurationAsync(Guid id);
    Task<bool> DisableConfigurationAsync(Guid id);
    Task<List<RateLimitConfigurationDto>> GetActiveConfigurationsAsync();
    Task<RateLimitConfigurationDto?> GetConfigurationByEndpointAsync(string endpoint, string httpMethod);
    Task<bool> ValidateConfigurationAsync(CreateRateLimitConfigurationRequest request);
    Task<bool> TestConfigurationAsync(Guid configurationId, RateLimitCheckRequest testRequest);
}
