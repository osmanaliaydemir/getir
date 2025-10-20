using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.AuditLogging;

/// <summary>
/// Log analizi interface'i: rapor oluşturma, listeleme, analitik üretme ve dışa aktarma işlemleri.
/// </summary>
public interface ILogAnalysisService
{
    /// <summary>Log analizi raporu oluşturur.</summary>
    Task<Result<LogAnalysisReportResponse>> CreateLogAnalysisReportAsync(CreateLogAnalysisReportRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>Raporu ID'ye göre getirir.</summary>
    Task<Result<LogAnalysisReportResponse>> GetLogAnalysisReportByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>Raporları filtre ve sayfalama ile listeler.</summary>
    Task<Result<IEnumerable<LogAnalysisReportResponse>>> GetLogAnalysisReportsAsync(DateTime? startDate = null, DateTime? endDate = null, string? reportType = null, string? status = null, Guid? generatedByUserId = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    
    /// <summary>Audit log analitik verileri üretir.</summary>
    Task<Result<AuditLogAnalyticsResponse>> GenerateAuditLogAnalyticsAsync(AuditLogAnalyticsRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>Kullanıcı aktivite analitikleri üretir.</summary>
    Task<Result<Dictionary<string, object>>> GenerateUserActivityAnalyticsAsync(DateTime startDate, DateTime endDate, Guid? userId = null, string? activityType = null, CancellationToken cancellationToken = default);
    
    /// <summary>Sistem değişikliği analitikleri üretir.</summary>
    Task<Result<Dictionary<string, object>>> GenerateSystemChangeAnalyticsAsync(DateTime startDate, DateTime endDate, string? entityType = null, string? changeType = null, CancellationToken cancellationToken = default);
    
    /// <summary>Güvenlik olayı analitikleri üretir.</summary>
    Task<Result<Dictionary<string, object>>> GenerateSecurityEventAnalyticsAsync(DateTime startDate, DateTime endDate, string? severity = null, string? riskLevel = null, CancellationToken cancellationToken = default);
    
    /// <summary>Planlanmış raporları getirir.</summary>
    Task<Result<IEnumerable<LogAnalysisReportResponse>>> GetScheduledReportsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>Planlanmış raporları çalıştırır.</summary>
    Task<Result> ExecuteScheduledReportsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>Süresi dolmuş raporları siler.</summary>
    Task<Result> DeleteExpiredReportsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>Raporu yeniden üretir.</summary>
    Task<Result<LogAnalysisReportResponse>> RegenerateReportAsync(Guid reportId, CancellationToken cancellationToken = default);
    
    /// <summary>Raporu dışa aktarır.</summary>
    Task<Result<byte[]>> ExportReportAsync(Guid reportId, string format = "PDF", CancellationToken cancellationToken = default);
}
