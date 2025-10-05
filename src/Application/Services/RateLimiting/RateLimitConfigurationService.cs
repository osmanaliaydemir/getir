using Getir.Application.DTO;

namespace Getir.Application.Services.RateLimiting;

public class RateLimitConfigurationService : IRateLimitConfigurationService
{
    private readonly List<RateLimitConfigurationDto> _mockConfigurations;

    public RateLimitConfigurationService()
    {
        // Mock configurations
        _mockConfigurations = new List<RateLimitConfigurationDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Default API Configuration",
                Description = "Default rate limiting configuration for all API endpoints",
                Type = Getir.Domain.Enums.RateLimitType.Global,
                RequestLimit = 1000,
                Period = Getir.Domain.Enums.RateLimitPeriod.PerHour,
                Action = Getir.Domain.Enums.RateLimitAction.Throttle,
                ThrottleDelayMs = 100,
                IsActive = true,
                Priority = 1,
                EnableLogging = true,
                EnableAlerting = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Authentication Endpoints",
                Description = "Rate limiting for authentication endpoints",
                Type = Getir.Domain.Enums.RateLimitType.Endpoint,
                EndpointPattern = "/api/auth/*",
                HttpMethod = "POST",
                RequestLimit = 10,
                Period = Getir.Domain.Enums.RateLimitPeriod.PerMinute,
                Action = Getir.Domain.Enums.RateLimitAction.Block,
                IsActive = true,
                Priority = 10,
                EnableLogging = true,
                EnableAlerting = true,
                AlertThreshold = 5,
                AlertRecipients = "admin@getir.com"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Premium User Limits",
                Description = "Higher limits for premium users",
                Type = Getir.Domain.Enums.RateLimitType.User,
                UserTier = "premium",
                RequestLimit = 500,
                Period = Getir.Domain.Enums.RateLimitPeriod.PerMinute,
                Action = Getir.Domain.Enums.RateLimitAction.Throttle,
                ThrottleDelayMs = 50,
                IsActive = true,
                Priority = 5,
                EnableLogging = true,
                EnableAlerting = false
            }
        };
    }

    public Task<List<RateLimitConfigurationDto>> GetAllConfigurationsAsync()
    {
        return Task.FromResult(_mockConfigurations.ToList());
    }

    public Task<RateLimitConfigurationDto?> GetConfigurationByIdAsync(Guid id)
    {
        return Task.FromResult(_mockConfigurations.FirstOrDefault(c => c.Id == id));
    }

    public Task<RateLimitConfigurationDto> CreateConfigurationAsync(CreateRateLimitConfigurationRequest request)
    {
        var configuration = new RateLimitConfigurationDto
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            EndpointPattern = request.EndpointPattern,
            HttpMethod = request.HttpMethod,
            RequestLimit = request.RequestLimit,
            Period = request.Period,
            Action = request.Action,
            ThrottleDelayMs = request.ThrottleDelayMs,
            IsActive = true,
            Priority = request.Priority,
            UserRole = request.UserRole,
            UserTier = request.UserTier,
            IpWhitelist = request.IpWhitelist,
            IpBlacklist = request.IpBlacklist,
            UserWhitelist = request.UserWhitelist,
            UserBlacklist = request.UserBlacklist,
            EnableLogging = request.EnableLogging,
            EnableAlerting = request.EnableAlerting,
            AlertThreshold = request.AlertThreshold,
            AlertRecipients = request.AlertRecipients
        };

        _mockConfigurations.Add(configuration);
        return Task.FromResult(configuration);
    }

    public Task<RateLimitConfigurationDto> UpdateConfigurationAsync(Guid id, UpdateRateLimitConfigurationRequest request)
    {
        var configuration = _mockConfigurations.FirstOrDefault(c => c.Id == id);
        if (configuration == null)
        {
            throw new ArgumentException("Configuration not found");
        }

        configuration.Name = request.Name;
        configuration.Description = request.Description;
        configuration.EndpointPattern = request.EndpointPattern;
        configuration.HttpMethod = request.HttpMethod;
        configuration.RequestLimit = request.RequestLimit;
        configuration.Period = request.Period;
        configuration.Action = request.Action;
        configuration.ThrottleDelayMs = request.ThrottleDelayMs;
        configuration.IsActive = request.IsActive;
        configuration.Priority = request.Priority;
        configuration.UserRole = request.UserRole;
        configuration.UserTier = request.UserTier;
        configuration.IpWhitelist = request.IpWhitelist;
        configuration.IpBlacklist = request.IpBlacklist;
        configuration.UserWhitelist = request.UserWhitelist;
        configuration.UserBlacklist = request.UserBlacklist;
        configuration.EnableLogging = request.EnableLogging;
        configuration.EnableAlerting = request.EnableAlerting;
        configuration.AlertThreshold = request.AlertThreshold;
        configuration.AlertRecipients = request.AlertRecipients;

        return Task.FromResult(configuration);
    }

    public Task<bool> DeleteConfigurationAsync(Guid id)
    {
        var configuration = _mockConfigurations.FirstOrDefault(c => c.Id == id);
        if (configuration != null)
        {
            _mockConfigurations.Remove(configuration);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> EnableConfigurationAsync(Guid id)
    {
        var configuration = _mockConfigurations.FirstOrDefault(c => c.Id == id);
        if (configuration != null)
        {
            configuration.IsActive = true;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> DisableConfigurationAsync(Guid id)
    {
        var configuration = _mockConfigurations.FirstOrDefault(c => c.Id == id);
        if (configuration != null)
        {
            configuration.IsActive = false;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<List<RateLimitConfigurationDto>> GetActiveConfigurationsAsync()
    {
        return Task.FromResult(_mockConfigurations.Where(c => c.IsActive).ToList());
    }

    public Task<RateLimitConfigurationDto?> GetConfigurationByEndpointAsync(string endpoint, string httpMethod)
    {
        var configuration = _mockConfigurations
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.Priority)
            .FirstOrDefault(c => 
                (string.IsNullOrEmpty(c.EndpointPattern) || endpoint.Contains(c.EndpointPattern.Replace("*", ""))) &&
                (string.IsNullOrEmpty(c.HttpMethod) || c.HttpMethod == httpMethod));

        return Task.FromResult(configuration);
    }

    public Task<bool> ValidateConfigurationAsync(CreateRateLimitConfigurationRequest request)
    {
        // Mock validation
        if (request.RequestLimit <= 0)
            return Task.FromResult(false);

        if (string.IsNullOrEmpty(request.Name))
            return Task.FromResult(false);

        return Task.FromResult(true);
    }

    public Task<bool> TestConfigurationAsync(Guid configurationId, RateLimitCheckRequest testRequest)
    {
        // Mock testing
        return Task.FromResult(true);
    }
}
