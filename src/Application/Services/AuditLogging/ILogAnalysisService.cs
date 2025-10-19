using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.AuditLogging;

public interface ILogAnalysisService
{
    Task<Result<LogAnalysisReportResponse>> CreateLogAnalysisReportAsync(CreateLogAnalysisReportRequest request, CancellationToken cancellationToken = default);
    Task<Result<LogAnalysisReportResponse>> GetLogAnalysisReportByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<LogAnalysisReportResponse>>> GetLogAnalysisReportsAsync(DateTime? startDate = null, DateTime? endDate = null, string? reportType = null, string? status = null, Guid? generatedByUserId = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<Result<AuditLogAnalyticsResponse>> GenerateAuditLogAnalyticsAsync(AuditLogAnalyticsRequest request, CancellationToken cancellationToken = default);
    Task<Result<Dictionary<string, object>>> GenerateUserActivityAnalyticsAsync(DateTime startDate, DateTime endDate, Guid? userId = null, string? activityType = null, CancellationToken cancellationToken = default);
    Task<Result<Dictionary<string, object>>> GenerateSystemChangeAnalyticsAsync(DateTime startDate, DateTime endDate, string? entityType = null, string? changeType = null, CancellationToken cancellationToken = default);
    Task<Result<Dictionary<string, object>>> GenerateSecurityEventAnalyticsAsync(DateTime startDate, DateTime endDate, string? severity = null, string? riskLevel = null, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<LogAnalysisReportResponse>>> GetScheduledReportsAsync(CancellationToken cancellationToken = default);
    Task<Result> ExecuteScheduledReportsAsync(CancellationToken cancellationToken = default);
    Task<Result> DeleteExpiredReportsAsync(CancellationToken cancellationToken = default);
    Task<Result<LogAnalysisReportResponse>> RegenerateReportAsync(Guid reportId, CancellationToken cancellationToken = default);
    Task<Result<byte[]>> ExportReportAsync(Guid reportId, string format = "PDF", CancellationToken cancellationToken = default);
}
