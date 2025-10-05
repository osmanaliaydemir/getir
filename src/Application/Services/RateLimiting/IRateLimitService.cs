using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.RateLimiting;

public interface IRateLimitService
{
    Task<RateLimitCheckResponse> CheckRateLimitAsync(RateLimitCheckRequest request);
    Task<bool> IsAllowedAsync(string endpoint, string httpMethod, string? userId = null, string? ipAddress = null);
    Task<RateLimitCheckResponse> GetRateLimitStatusAsync(string endpoint, string httpMethod, string? userId = null, string? ipAddress = null);
    Task LogRequestAsync(RateLimitCheckRequest request, RateLimitCheckResponse response);
    Task<List<RateLimitRuleDto>> GetActiveRulesAsync();
    Task<RateLimitRuleDto?> GetRuleByIdAsync(Guid id);
    Task<RateLimitRuleDto> CreateRuleAsync(CreateRateLimitRuleRequest request);
    Task<RateLimitRuleDto> UpdateRuleAsync(Guid id, UpdateRateLimitRuleRequest request);
    Task<bool> DeleteRuleAsync(Guid id);
    Task<bool> EnableRuleAsync(Guid id);
    Task<bool> DisableRuleAsync(Guid id);
    Task<List<RateLimitStatisticsDto>> GetStatisticsAsync(DateTime startDate, DateTime endDate);
    Task<RateLimitSearchResponse> SearchLogsAsync(RateLimitSearchRequest request);
    Task<RateLimitViolationSearchResponse> SearchViolationsAsync(RateLimitViolationSearchRequest request);
    Task<bool> ResolveViolationAsync(Guid violationId, ResolveViolationRequest request);
    Task<bool> ClearRateLimitCacheAsync(string? endpoint = null, string? userId = null, string? ipAddress = null);
}
