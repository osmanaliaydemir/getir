using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.AuditLogging;

public interface ISecurityEventLogService
{
    Task<Result<SecurityEventLogResponse>> CreateSecurityEventLogAsync(
        CreateSecurityEventLogRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<SecurityEventLogResponse>> GetSecurityEventLogByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<SecurityEventLogResponse>>> GetSecurityEventLogsAsync(
        SecurityEventQueryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<SecurityEventLogResponse>>> GetUnresolvedSecurityEventsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? severity = null,
        string? riskLevel = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<SecurityEventLogResponse>>> GetSecurityEventsRequiringInvestigationAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<Result<SecurityEventLogResponse>> ResolveSecurityEventAsync(
        Guid id,
        string resolvedBy,
        string resolutionNotes,
        CancellationToken cancellationToken = default);

    Task<Result<SecurityEventLogResponse>> MarkAsFalsePositiveAsync(
        Guid id,
        string resolvedBy,
        string resolutionNotes,
        CancellationToken cancellationToken = default);

    Task<Result<Dictionary<string, int>>> GetSecurityEventStatisticsAsync(
        DateTime startDate,
        DateTime endDate,
        string? severity = null,
        string? riskLevel = null,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<SecurityEventLogResponse>>> GetHighRiskSecurityEventsAsync(
        DateTime startDate,
        DateTime endDate,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<SecurityEventLogResponse>>> GetSecurityEventsByUserAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<SecurityEventLogResponse>>> GetSecurityEventsByIpAddressAsync(
        string ipAddress,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteOldSecurityEventLogsAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default);
}
