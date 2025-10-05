using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.AuditLogging;

public interface ISystemChangeLogService
{
    Task<Result<SystemChangeLogResponse>> CreateSystemChangeLogAsync(
        CreateSystemChangeLogRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<SystemChangeLogResponse>> GetSystemChangeLogByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<SystemChangeLogResponse>>> GetSystemChangeLogsAsync(
        SystemChangeQueryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<SystemChangeLogResponse>>> GetSystemChangeLogsByEntityAsync(
        string entityType,
        string entityId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<SystemChangeLogResponse>>> GetSystemChangeLogsByUserAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<Result<Dictionary<string, int>>> GetSystemChangeStatisticsAsync(
        DateTime startDate,
        DateTime endDate,
        string? entityType = null,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<SystemChangeLogResponse>>> GetRecentSystemChangesAsync(
        int count = 50,
        string? entityType = null,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteOldSystemChangeLogsAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default);
}
