using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.RateLimiting;

public interface IRateLimitMonitoringService
{
    Task<List<RateLimitStatisticsDto>> GetStatisticsAsync(DateTime startDate, DateTime endDate, RateLimitType? type = null);
    Task<RateLimitStatisticsDto> GetEndpointStatisticsAsync(string endpoint, DateTime startDate, DateTime endDate);
    Task<RateLimitStatisticsDto> GetUserStatisticsAsync(string userId, DateTime startDate, DateTime endDate);
    Task<RateLimitStatisticsDto> GetIpStatisticsAsync(string ipAddress, DateTime startDate, DateTime endDate);
    Task<List<RateLimitViolationDto>> GetRecentViolationsAsync(int count = 10);
    Task<List<RateLimitViolationDto>> GetUnresolvedViolationsAsync();
    Task<List<RateLimitViolationDto>> GetHighSeverityViolationsAsync();
    Task<Dictionary<string, int>> GetTopViolatingEndpointsAsync(DateTime startDate, DateTime endDate, int count = 10);
    Task<Dictionary<string, int>> GetTopViolatingUsersAsync(DateTime startDate, DateTime endDate, int count = 10);
    Task<Dictionary<string, int>> GetTopViolatingIpsAsync(DateTime startDate, DateTime endDate, int count = 10);
    Task<bool> SendAlertAsync(string message, string? recipients = null);
    Task<bool> CheckAlertThresholdsAsync();
    Task<List<RateLimitLogDto>> GetRealTimeLogsAsync(int count = 100);
    Task<Dictionary<string, object>> GetDashboardDataAsync();
}
