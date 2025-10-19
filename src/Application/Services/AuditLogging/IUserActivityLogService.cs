using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.AuditLogging;

public interface IUserActivityLogService
{
    Task<Result<UserActivityLogResponse>> CreateUserActivityLogAsync(CreateUserActivityLogRequest request, CancellationToken cancellationToken = default);

    Task<Result<UserActivityLogResponse>> GetUserActivityLogByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserActivityLogResponse>>> GetUserActivityLogsAsync(UserActivityQueryRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserActivityLogResponse>>> GetUserActivityLogsByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserActivityLogResponse>>> GetUserActivityLogsByActivityTypeAsync(string activityType, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<Result<Dictionary<string, int>>> GetUserActivityStatisticsAsync(DateTime startDate, DateTime endDate, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserActivityLogResponse>>> GetSuspiciousActivitiesAsync(DateTime startDate, DateTime endDate, int threshold = 10, CancellationToken cancellationToken = default);
    Task<Result> DeleteOldUserActivityLogsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
}
