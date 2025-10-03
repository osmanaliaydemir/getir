using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Payments;

/// <summary>
/// Nakit ödeme audit log servisi
/// </summary>
public interface ICashPaymentAuditService
{
    /// <summary>
    /// Audit log oluştur
    /// </summary>
    Task<Result<CashPaymentAuditLogResponse>> CreateAuditLogAsync(
        CreateAuditLogRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Audit log güncelle
    /// </summary>
    Task<Result<CashPaymentAuditLogResponse>> UpdateAuditLogAsync(
        Guid auditLogId,
        UpdateAuditLogRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Audit log'ları sorgula
    /// </summary>
    Task<Result<PagedResult<CashPaymentAuditLogResponse>>> GetAuditLogsAsync(
        CashPaymentAuditLogQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Audit log'u ID ile getir
    /// </summary>
    Task<Result<CashPaymentAuditLogResponse>> GetAuditLogByIdAsync(
        Guid auditLogId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ödeme için audit log'ları getir
    /// </summary>
    Task<Result<IEnumerable<CashPaymentAuditLogResponse>>> GetAuditLogsByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kurye için audit log'ları getir
    /// </summary>
    Task<Result<IEnumerable<CashPaymentAuditLogResponse>>> GetAuditLogsByCourierIdAsync(
        Guid courierId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Audit log istatistikleri getir
    /// </summary>
    Task<Result<AuditLogStatisticsResponse>> GetAuditLogStatisticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Risk analizi yap
    /// </summary>
    Task<Result<RiskAnalysisResponse>> PerformRiskAnalysisAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compliance raporu oluştur
    /// </summary>
    Task<Result<ComplianceReportResponse>> GenerateComplianceReportAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Güvenlik olaylarını getir
    /// </summary>
    Task<Result<IEnumerable<CashPaymentAuditLogResponse>>> GetSecurityIncidentsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        SecurityRiskLevel? minRiskLevel = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kritik olayları getir
    /// </summary>
    Task<Result<IEnumerable<CashPaymentAuditLogResponse>>> GetCriticalEventsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Audit log'u sil (soft delete)
    /// </summary>
    Task<Result> DeleteAuditLogAsync(
        Guid auditLogId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Eski audit log'ları temizle
    /// </summary>
    Task<Result<int>> CleanupOldAuditLogsAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Audit log'ları arşivle
    /// </summary>
    Task<Result> ArchiveAuditLogsAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default);
}
