using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.RateLimiting;

/// <summary>
/// Rate limit izleme servisi: istatistikler, ihlaller, top violators, gerçek zamanlı loglar, dashboard.
/// </summary>
public interface IRateLimitMonitoringService
{
    /// <summary>Rate limit istatistiklerini getirir (tip filtresi ile).</summary>
    Task<List<RateLimitStatisticsDto>> GetStatisticsAsync(DateTime startDate, DateTime endDate, RateLimitType? type = null);
    /// <summary>Endpoint istatistiklerini getirir (request/block/throttle sayıları).</summary>
    Task<RateLimitStatisticsDto> GetEndpointStatisticsAsync(string endpoint, DateTime startDate, DateTime endDate);
    /// <summary>Kullanıcı istatistiklerini getirir.</summary>
    Task<RateLimitStatisticsDto> GetUserStatisticsAsync(string userId, DateTime startDate, DateTime endDate);
    /// <summary>IP istatistiklerini getirir.</summary>
    Task<RateLimitStatisticsDto> GetIpStatisticsAsync(string ipAddress, DateTime startDate, DateTime endDate);
    /// <summary>Son ihlalleri getirir (zaman sıralı).</summary>
    Task<List<RateLimitViolationDto>> GetRecentViolationsAsync(int count = 10);
    /// <summary>Çözülmemiş ihlalleri getirir.</summary>
    Task<List<RateLimitViolationDto>> GetUnresolvedViolationsAsync();
    /// <summary>Yüksek öncelikli ihlalleri getirir (severity >= 3).</summary>
    Task<List<RateLimitViolationDto>> GetHighSeverityViolationsAsync();
    /// <summary>En çok ihlal yapılan endpointleri getirir (top N).</summary>
    Task<Dictionary<string, int>> GetTopViolatingEndpointsAsync(DateTime startDate, DateTime endDate, int count = 10);
    /// <summary>En çok ihlal yapan kullanıcıları getirir (top N).</summary>
    Task<Dictionary<string, int>> GetTopViolatingUsersAsync(DateTime startDate, DateTime endDate, int count = 10);
    /// <summary>En çok ihlal yapan IP'leri getirir (top N).</summary>
    Task<Dictionary<string, int>> GetTopViolatingIpsAsync(DateTime startDate, DateTime endDate, int count = 10);
    /// <summary>Uyarı gönderir (email/SMS).</summary>
    Task<bool> SendAlertAsync(string message, string? recipients = null);
    /// <summary>Uyarı eşiklerini kontrol eder.</summary>
    Task<bool> CheckAlertThresholdsAsync();
    /// <summary>Gerçek zamanlı logları getirir (son N adet).</summary>
    Task<List<RateLimitLogDto>> GetRealTimeLogsAsync(int count = 100);
    /// <summary>Dashboard verilerini getirir (özet istatistikler, top violators).</summary>
    Task<Dictionary<string, object>> GetDashboardDataAsync();
}
