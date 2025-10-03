using Getir.Domain.Enums;

namespace Getir.Application.DTO;

/// <summary>
/// Audit log oluşturma isteği
/// </summary>
public record CreateAuditLogRequest(
    Guid? PaymentId,
    Guid? CourierId,
    Guid? CustomerId,
    Guid? AdminId,
    AuditEventType EventType,
    AuditSeverityLevel SeverityLevel,
    string Title,
    string Description,
    string? Details = null,
    SecurityRiskLevel? RiskLevel = null,
    string? IpAddress = null,
    string? UserAgent = null,
    string? DeviceInfo = null,
    double? Latitude = null,
    double? Longitude = null,
    string? SessionId = null,
    string? RequestId = null,
    string? CorrelationId = null);

/// <summary>
/// Audit log güncelleme isteği
/// </summary>
public record UpdateAuditLogRequest(
    string? Description = null,
    string? Details = null,
    SecurityRiskLevel? RiskLevel = null);

/// <summary>
/// Audit log response
/// </summary>
public record CashPaymentAuditLogResponse(
    Guid Id,
    Guid? PaymentId,
    Guid? CourierId,
    Guid? CustomerId,
    Guid? AdminId,
    AuditEventType EventType,
    AuditSeverityLevel SeverityLevel,
    string Title,
    string Description,
    string? Details,
    SecurityRiskLevel? RiskLevel,
    string? IpAddress,
    string? UserAgent,
    string? DeviceInfo,
    double? Latitude,
    double? Longitude,
    string? SessionId,
    string? RequestId,
    string? CorrelationId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// Cash payment audit log sorgu parametreleri
/// </summary>
public record CashPaymentAuditLogQuery
{
    public Guid? PaymentId { get; init; } = null;
    public Guid? CourierId { get; init; } = null;
    public Guid? CustomerId { get; init; } = null;
    public Guid? AdminId { get; init; } = null;
    public AuditEventType? EventType { get; init; } = null;
    public AuditSeverityLevel? SeverityLevel { get; init; } = null;
    public SecurityRiskLevel? RiskLevel { get; init; } = null;
    public DateTime? StartDate { get; init; } = null;
    public DateTime? EndDate { get; init; } = null;
    public string? SearchTerm { get; init; } = null;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Audit log istatistikleri
/// </summary>
public record AuditLogStatisticsResponse(
    int TotalEvents,
    int SecurityEvents,
    int CriticalEvents,
    int ErrorEvents,
    int WarningEvents,
    int InformationEvents,
    Dictionary<AuditEventType, int> EventsByType,
    Dictionary<AuditSeverityLevel, int> EventsBySeverity,
    Dictionary<SecurityRiskLevel, int> EventsByRiskLevel,
    DateTime? FirstEventDate,
    DateTime? LastEventDate);

/// <summary>
/// Risk analizi response
/// </summary>
public record RiskAnalysisResponse(
    SecurityRiskLevel OverallRiskLevel,
    int TotalRisks,
    int HighRiskEvents,
    int CriticalRiskEvents,
    List<RiskEventSummary> RecentRisks,
    List<RiskTrend> RiskTrends,
    List<string> RiskRecommendations);

/// <summary>
/// Risk event özeti
/// </summary>
public record RiskEventSummary(
    Guid Id,
    AuditEventType EventType,
    SecurityRiskLevel RiskLevel,
    string Title,
    string Description,
    DateTime CreatedAt,
    Guid? PaymentId,
    Guid? CourierId);

/// <summary>
/// Risk trend bilgisi
/// </summary>
public record RiskTrend(
    DateTime Date,
    int EventCount,
    SecurityRiskLevel HighestRiskLevel,
    AuditEventType MostCommonEventType);

/// <summary>
/// Compliance raporu
/// </summary>
public record ComplianceReportResponse(
    DateTime ReportDate,
    DateTime StartDate,
    DateTime EndDate,
    int TotalAuditLogs,
    int SecurityIncidents,
    int ManualApprovals,
    int RejectedEvidences,
    int FakeMoneyDetections,
    List<ComplianceViolation> Violations,
    ComplianceScore ComplianceScore);

/// <summary>
/// Compliance ihlali
/// </summary>
public record ComplianceViolation(
    string ViolationType,
    string Description,
    int Count,
    SecurityRiskLevel RiskLevel,
    List<Guid> RelatedAuditLogIds);

/// <summary>
/// Compliance skoru
/// </summary>
public record ComplianceScore(
    int Score, // 0-100
    string Grade, // A, B, C, D, F
    string Description,
    List<string> ImprovementAreas);
