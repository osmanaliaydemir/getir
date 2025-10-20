using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.RateLimiting;

/// <summary>
/// Rate limit servisi: istek kontrolü, kural yönetimi, log/ihlal arama, cache yönetimi.
/// </summary>
public interface IRateLimitService
{
    /// <summary>İsteğin rate limit durumunu kontrol eder (global/endpoint/user/IP bazlı, priority sıralı).</summary>
    Task<RateLimitCheckResponse> CheckRateLimitAsync(RateLimitCheckRequest request);
    /// <summary>İsteğin izinli olup olmadığını kontrol eder (basit boolean).</summary>
    Task<bool> IsAllowedAsync(string endpoint, string httpMethod, string? userId = null, string? ipAddress = null);
    /// <summary>Rate limit durumunu detaylı getirir (kalan istek, reset zamanı).</summary>
    Task<RateLimitCheckResponse> GetRateLimitStatusAsync(string endpoint, string httpMethod, string? userId = null, string? ipAddress = null);
    /// <summary>İsteği loglar (response ile birlikte).</summary>
    Task LogRequestAsync(RateLimitCheckRequest request, RateLimitCheckResponse response);
    /// <summary>Aktif kuralları getirir.</summary>
    Task<List<RateLimitRuleDto>> GetActiveRulesAsync();
    /// <summary>Kuralı ID ile getirir.</summary>
    Task<RateLimitRuleDto?> GetRuleByIdAsync(Guid id);
    /// <summary>Yeni kural oluşturur.</summary>
    Task<RateLimitRuleDto> CreateRuleAsync(CreateRateLimitRuleRequest request);
    /// <summary>Kuralı günceller.</summary>
    Task<RateLimitRuleDto> UpdateRuleAsync(Guid id, UpdateRateLimitRuleRequest request);
    /// <summary>Kuralı siler.</summary>
    Task<bool> DeleteRuleAsync(Guid id);
    /// <summary>Kuralı etkinleştirir.</summary>
    Task<bool> EnableRuleAsync(Guid id);
    /// <summary>Kuralı devre dışı bırakır.</summary>
    Task<bool> DisableRuleAsync(Guid id);
    /// <summary>İstatistikleri getirir (tip bazlı).</summary>
    Task<List<RateLimitStatisticsDto>> GetStatisticsAsync(DateTime startDate, DateTime endDate);
    /// <summary>Logları arar (filtreleme ve sayfalama ile).</summary>
    Task<RateLimitSearchResponse> SearchLogsAsync(RateLimitSearchRequest request);
    /// <summary>İhlalleri arar (filtreleme ve sayfalama ile).</summary>
    Task<RateLimitViolationSearchResponse> SearchViolationsAsync(RateLimitViolationSearchRequest request);
    /// <summary>İhlali çözer (notlar ile).</summary>
    Task<bool> ResolveViolationAsync(Guid violationId, ResolveViolationRequest request);
    /// <summary>Rate limit cache'ini temizler (endpoint/user/IP filtresi).</summary>
    Task<bool> ClearRateLimitCacheAsync(string? endpoint = null, string? userId = null, string? ipAddress = null);
}
