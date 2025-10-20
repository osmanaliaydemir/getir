using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.AuditLogging;

/// <summary>
/// Kullanıcı aktivite log interface'i: oluşturma, sorgulama, istatistik ve temizlik işlemleri.
/// </summary>
public interface IUserActivityLogService
{
    /// <summary>Kullanıcı aktivite logu oluşturur.</summary>
    Task<Result<UserActivityLogResponse>> CreateUserActivityLogAsync(CreateUserActivityLogRequest request, CancellationToken cancellationToken = default);

    /// <summary>ID'ye göre aktivite logu getirir.</summary>
    Task<Result<UserActivityLogResponse>> GetUserActivityLogByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>Filtre ve sayfalama ile aktivite loglarını getirir.</summary>
    Task<Result<IEnumerable<UserActivityLogResponse>>> GetUserActivityLogsAsync(UserActivityQueryRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>Kullanıcı ID'ye göre aktivite loglarını getirir.</summary>
    Task<Result<IEnumerable<UserActivityLogResponse>>> GetUserActivityLogsByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    
    /// <summary>Aktivite türüne göre logları getirir.</summary>
    Task<Result<IEnumerable<UserActivityLogResponse>>> GetUserActivityLogsByActivityTypeAsync(string activityType, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    
    /// <summary>Aktivite istatistiklerini getirir.</summary>
    Task<Result<Dictionary<string, int>>> GetUserActivityStatisticsAsync(DateTime startDate, DateTime endDate, Guid? userId = null, CancellationToken cancellationToken = default);
    
    /// <summary>Şüpheli aktiviteleri getirir.</summary>
    Task<Result<IEnumerable<UserActivityLogResponse>>> GetSuspiciousActivitiesAsync(DateTime startDate, DateTime endDate, int threshold = 10, CancellationToken cancellationToken = default);
    
    /// <summary>Eski aktivite loglarını siler.</summary>
    Task<Result> DeleteOldUserActivityLogsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
}
