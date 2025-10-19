using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.Common.Extensions;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Getir.Application.Services.Payments;

/// <summary>
/// Nakit ödeme audit log servisi implementasyonu
/// </summary>
public class CashPaymentAuditService : BaseService, ICashPaymentAuditService
{
    public CashPaymentAuditService(IUnitOfWork unitOfWork, ILogger<CashPaymentAuditService> logger, ILoggingService loggingService, ICacheService cacheService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
    }
    public async Task<Result<CashPaymentAuditLogResponse>> CreateAuditLogAsync(CreateAuditLogRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new CashPaymentAuditLog
            {
                Id = Guid.NewGuid(),
                PaymentId = request.PaymentId,
                CourierId = request.CourierId,
                CustomerId = request.CustomerId,
                AdminId = request.AdminId,
                EventType = request.EventType,
                SeverityLevel = request.SeverityLevel,
                Title = request.Title,
                Description = request.Description,
                Details = request.Details,
                RiskLevel = request.RiskLevel,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                DeviceInfo = request.DeviceInfo,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                SessionId = request.SessionId,
                RequestId = request.RequestId,
                CorrelationId = request.CorrelationId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<CashPaymentAuditLog>().AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(auditLog);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "CreateAuditLog", new { EventType = request.EventType, PaymentId = request.PaymentId });
            return Result.Fail<CashPaymentAuditLogResponse>("Failed to create audit log", "CREATE_AUDIT_LOG_FAILED");
        }
    }
    public async Task<Result<CashPaymentAuditLogResponse>> UpdateAuditLogAsync(Guid auditLogId, UpdateAuditLogRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = await _unitOfWork.Repository<CashPaymentAuditLog>()
                .GetByIdAsync(auditLogId, cancellationToken);

            if (auditLog == null)
            {
                return Result.Fail<CashPaymentAuditLogResponse>("Audit log not found", "AUDIT_LOG_NOT_FOUND");
            }

            if (!string.IsNullOrEmpty(request.Description))
                auditLog.Description = request.Description;

            if (!string.IsNullOrEmpty(request.Details))
                auditLog.Details = request.Details;

            if (request.RiskLevel.HasValue)
                auditLog.RiskLevel = request.RiskLevel.Value;

            auditLog.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<CashPaymentAuditLog>().Update(auditLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(auditLog);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "UpdateAuditLog", new { AuditLogId = auditLogId });
            return Result.Fail<CashPaymentAuditLogResponse>("Failed to update audit log", "UPDATE_AUDIT_LOG_FAILED");
        }
    }
    public async Task<Result<PagedResult<CashPaymentAuditLogResponse>>> GetAuditLogsAsync(CashPaymentAuditLogQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogs = await _unitOfWork.Repository<CashPaymentAuditLog>()
                .GetPagedAsync(
                    filter: BuildAuditLogPredicate(query),
                    orderBy: x => x.CreatedAt,
                    ascending: false,
                    page: query.Page,
                    pageSize: query.PageSize,
                    cancellationToken: cancellationToken);

            var responses = auditLogs.Select(MapToResponse).ToList();

            var total = await _unitOfWork.ReadRepository<CashPaymentAuditLog>()
                .CountAsync(BuildAuditLogPredicate(query), cancellationToken);

            var pagedResult = PagedResult<CashPaymentAuditLogResponse>.Create(
                responses,
                total,
                query.Page,
                query.PageSize);

            return Result.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GetAuditLogs", new { Query = query });
            return Result.Fail<PagedResult<CashPaymentAuditLogResponse>>("Failed to get audit logs", "GET_AUDIT_LOGS_FAILED");
        }
    }
    public async Task<Result<CashPaymentAuditLogResponse>> GetAuditLogByIdAsync(Guid auditLogId, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = await _unitOfWork.Repository<CashPaymentAuditLog>()
                .GetByIdAsync(auditLogId, cancellationToken);

            if (auditLog == null)
            {
                return Result.Fail<CashPaymentAuditLogResponse>("Audit log not found", "AUDIT_LOG_NOT_FOUND");
            }

            var response = MapToResponse(auditLog);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GetAuditLogById", new { AuditLogId = auditLogId });
            return Result.Fail<CashPaymentAuditLogResponse>("Failed to get audit log", "GET_AUDIT_LOG_FAILED");
        }
    }
    public async Task<Result<IEnumerable<CashPaymentAuditLogResponse>>> GetAuditLogsByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogs = await _unitOfWork.Repository<CashPaymentAuditLog>()
                .ListAsync(
                    filter: x => x.PaymentId == paymentId && !x.IsDeleted,
                    orderBy: x => x.CreatedAt,
                    ascending: false,
                    cancellationToken: cancellationToken);

            var responses = auditLogs.Select(MapToResponse);
            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GetAuditLogsByPaymentId", new { PaymentId = paymentId });
            return Result.Fail<IEnumerable<CashPaymentAuditLogResponse>>("Failed to get audit logs by payment ID", "GET_AUDIT_LOGS_BY_PAYMENT_FAILED");
        }
    }
    public async Task<Result<IEnumerable<CashPaymentAuditLogResponse>>> GetAuditLogsByCourierIdAsync(Guid courierId, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogs = await _unitOfWork.Repository<CashPaymentAuditLog>()
                .ListAsync(
                    filter: x => x.CourierId == courierId && !x.IsDeleted,
                    orderBy: x => x.CreatedAt,
                    ascending: false,
                    cancellationToken: cancellationToken);

            var responses = auditLogs.Select(MapToResponse);
            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GetAuditLogsByCourierId", new { CourierId = courierId });
            return Result.Fail<IEnumerable<CashPaymentAuditLogResponse>>("Failed to get audit logs by courier ID", "GET_AUDIT_LOGS_BY_COURIER_FAILED");
        }
    }
    public async Task<Result<AuditLogStatisticsResponse>> GetAuditLogStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var predicate = BuildDateRangePredicate(startDate, endDate);

            var auditLogs = await _unitOfWork.Repository<CashPaymentAuditLog>()
                .ListAsync(filter: predicate, cancellationToken: cancellationToken);

            var statistics = new AuditLogStatisticsResponse(
                TotalEvents: auditLogs.Count(),
                SecurityEvents: auditLogs.Count(x => x.SeverityLevel == AuditSeverityLevel.Security),
                CriticalEvents: auditLogs.Count(x => x.SeverityLevel == AuditSeverityLevel.Critical),
                ErrorEvents: auditLogs.Count(x => x.SeverityLevel == AuditSeverityLevel.Error),
                WarningEvents: auditLogs.Count(x => x.SeverityLevel == AuditSeverityLevel.Warning),
                InformationEvents: auditLogs.Count(x => x.SeverityLevel == AuditSeverityLevel.Information),
                EventsByType: auditLogs.GroupBy(x => x.EventType).ToDictionary(g => g.Key, g => g.Count()),
                EventsBySeverity: auditLogs.GroupBy(x => x.SeverityLevel).ToDictionary(g => g.Key, g => g.Count()),
                EventsByRiskLevel: auditLogs.Where(x => x.RiskLevel.HasValue).GroupBy(x => x.RiskLevel!.Value).ToDictionary(g => g.Key, g => g.Count()),
                FirstEventDate: auditLogs.Any() ? auditLogs.Min(x => x.CreatedAt) : null,
                LastEventDate: auditLogs.Any() ? auditLogs.Max(x => x.CreatedAt) : null
            );

            return Result.Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GetAuditLogStatistics", new { StartDate = startDate, EndDate = endDate });
            return Result.Fail<AuditLogStatisticsResponse>("Failed to get audit log statistics", "GET_AUDIT_LOG_STATISTICS_FAILED");
        }
    }
    public async Task<Result<RiskAnalysisResponse>> PerformRiskAnalysisAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var predicate = BuildDateRangePredicate(startDate, endDate);

            var auditLogs = await _unitOfWork.Repository<CashPaymentAuditLog>()
                .ListAsync(filter: predicate, cancellationToken: cancellationToken);

            var riskLogs = auditLogs.Where(x => x.RiskLevel.HasValue).ToList();

            var overallRiskLevel = CalculateOverallRiskLevel(riskLogs);
            var totalRisks = riskLogs.Count;
            var highRiskEvents = riskLogs.Count(x => x.RiskLevel == SecurityRiskLevel.High);
            var criticalRiskEvents = riskLogs.Count(x => x.RiskLevel == SecurityRiskLevel.Critical);

            var recentRisks = riskLogs
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new RiskEventSummary(
                    x.Id,
                    x.EventType,
                    x.RiskLevel!.Value,
                    x.Title,
                    x.Description,
                    x.CreatedAt,
                    x.PaymentId,
                    x.CourierId))
                .ToList();

            var riskTrends = CalculateRiskTrends(riskLogs);
            var recommendations = GenerateRiskRecommendations(riskLogs);

            var analysis = new RiskAnalysisResponse(
                overallRiskLevel,
                totalRisks,
                highRiskEvents,
                criticalRiskEvents,
                recentRisks,
                riskTrends,
                recommendations
            );

            return Result.Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "PerformRiskAnalysis", new { StartDate = startDate, EndDate = endDate });
            return Result.Fail<RiskAnalysisResponse>("Failed to perform risk analysis", "RISK_ANALYSIS_FAILED");
        }
    }
    public async Task<Result<ComplianceReportResponse>> GenerateComplianceReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var predicate = BuildDateRangePredicate(startDate, endDate);

            var auditLogs = await _unitOfWork.Repository<CashPaymentAuditLog>()
                .ListAsync(filter: predicate, cancellationToken: cancellationToken);

            var totalAuditLogs = auditLogs.Count();
            var securityIncidents = auditLogs.Count(x => x.SeverityLevel == AuditSeverityLevel.Security);
            var manualApprovals = auditLogs.Count(x => x.EventType == AuditEventType.ManualApprovalRequired);
            var rejectedEvidences = auditLogs.Count(x => x.EventType == AuditEventType.EvidenceRejected);
            var fakeMoneyDetections = auditLogs.Count(x => x.EventType == AuditEventType.FakeMoneyDetected);

            var violations = IdentifyComplianceViolations(auditLogs);
            var complianceScore = CalculateComplianceScore(auditLogs, violations);

            var report = new ComplianceReportResponse(
                DateTime.UtcNow,
                startDate,
                endDate,
                totalAuditLogs,
                securityIncidents,
                manualApprovals,
                rejectedEvidences,
                fakeMoneyDetections,
                violations,
                complianceScore
            );

            return Result.Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GenerateComplianceReport", new { StartDate = startDate, EndDate = endDate });
            return Result.Fail<ComplianceReportResponse>("Failed to generate compliance report", "COMPLIANCE_REPORT_FAILED");
        }
    }
    public async Task<Result<IEnumerable<CashPaymentAuditLogResponse>>> GetSecurityIncidentsAsync(DateTime? startDate = null, DateTime? endDate = null, SecurityRiskLevel? minRiskLevel = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var predicate = BuildSecurityIncidentPredicate(startDate, endDate, minRiskLevel);

            var auditLogs = await _unitOfWork.Repository<CashPaymentAuditLog>()
                .ListAsync(
                    filter: predicate,
                    cancellationToken: cancellationToken);

            var responses = auditLogs.Select(MapToResponse);
            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GetSecurityIncidents", new { StartDate = startDate, EndDate = endDate, MinRiskLevel = minRiskLevel });
            return Result.Fail<IEnumerable<CashPaymentAuditLogResponse>>("Failed to get security incidents", "GET_SECURITY_INCIDENTS_FAILED");
        }
    }
    public async Task<Result<IEnumerable<CashPaymentAuditLogResponse>>> GetCriticalEventsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var predicate = BuildCriticalEventPredicate(startDate, endDate);

            var auditLogs = await _unitOfWork.Repository<CashPaymentAuditLog>()
                .ListAsync(
                    filter: predicate,
                    cancellationToken: cancellationToken);

            var responses = auditLogs.Select(MapToResponse);
            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GetCriticalEvents", new { StartDate = startDate, EndDate = endDate });
            return Result.Fail<IEnumerable<CashPaymentAuditLogResponse>>("Failed to get critical events", "GET_CRITICAL_EVENTS_FAILED");
        }
    }
    public async Task<Result> DeleteAuditLogAsync(Guid auditLogId, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = await _unitOfWork.Repository<CashPaymentAuditLog>()
                .GetByIdAsync(auditLogId, cancellationToken);

            if (auditLog == null)
            {
                return Result.Fail("Audit log not found", "AUDIT_LOG_NOT_FOUND");
            }

            auditLog.IsDeleted = true;
            auditLog.DeletedAt = DateTime.UtcNow;
            auditLog.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<CashPaymentAuditLog>().Update(auditLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "DeleteAuditLog", new { AuditLogId = auditLogId });
            return Result.Fail("Failed to delete audit log", "DELETE_AUDIT_LOG_FAILED");
        }
    }
    public async Task<Result<int>> CleanupOldAuditLogsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var oldAuditLogs = await _unitOfWork.Repository<CashPaymentAuditLog>()
                .ListAsync(filter: x => x.CreatedAt < cutoffDate && !x.IsDeleted, cancellationToken: cancellationToken);

            var count = oldAuditLogs.Count();

            foreach (var auditLog in oldAuditLogs)
            {
                auditLog.IsDeleted = true;
                auditLog.DeletedAt = DateTime.UtcNow;
                auditLog.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<CashPaymentAuditLog>().Update(auditLog);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "CleanupOldAuditLogs", new { CutoffDate = cutoffDate });
            return Result.Fail<int>("Failed to cleanup old audit logs", "CLEANUP_AUDIT_LOGS_FAILED");
        }
    }
    public async Task<Result> ArchiveAuditLogsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        try
        {
            // Bu method gelecekte arşivleme sistemi implementasyonu için placeholder
            // Şimdilik sadece log yazıyoruz
            _logger.LogInformation("Archive audit logs requested for date: {CutoffDate}", cutoffDate);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "ArchiveAuditLogs", new { CutoffDate = cutoffDate });
            return Result.Fail("Failed to archive audit logs", "ARCHIVE_AUDIT_LOGS_FAILED");
        }
    }
    // Private helper methods
    private static CashPaymentAuditLogResponse MapToResponse(CashPaymentAuditLog auditLog)
    {
        return new CashPaymentAuditLogResponse(
            auditLog.Id,
            auditLog.PaymentId,
            auditLog.CourierId,
            auditLog.CustomerId,
            auditLog.AdminId,
            auditLog.EventType,
            auditLog.SeverityLevel,
            auditLog.Title,
            auditLog.Description,
            auditLog.Details,
            auditLog.RiskLevel,
            auditLog.IpAddress,
            auditLog.UserAgent,
            auditLog.DeviceInfo,
            auditLog.Latitude,
            auditLog.Longitude,
            auditLog.SessionId,
            auditLog.RequestId,
            auditLog.CorrelationId,
            auditLog.CreatedAt,
            auditLog.UpdatedAt
        );
    }
    private static System.Linq.Expressions.Expression<Func<CashPaymentAuditLog, bool>> BuildAuditLogPredicate(CashPaymentAuditLogQuery query)
    {
        return x => !x.IsDeleted &&
                   (query.PaymentId == null || x.PaymentId == query.PaymentId) &&
                   (query.CourierId == null || x.CourierId == query.CourierId) &&
                   (query.CustomerId == null || x.CustomerId == query.CustomerId) &&
                   (query.AdminId == null || x.AdminId == query.AdminId) &&
                   (query.EventType == null || x.EventType == query.EventType) &&
                   (query.SeverityLevel == null || x.SeverityLevel == query.SeverityLevel) &&
                   (query.RiskLevel == null || x.RiskLevel == query.RiskLevel) &&
                   (query.StartDate == null || x.CreatedAt >= query.StartDate) &&
                   (query.EndDate == null || x.CreatedAt <= query.EndDate) &&
                   (string.IsNullOrEmpty(query.SearchTerm) ||
                    x.Title.Contains(query.SearchTerm) ||
                    x.Description.Contains(query.SearchTerm));
    }
    private static System.Linq.Expressions.Expression<Func<CashPaymentAuditLog, bool>> BuildDateRangePredicate(DateTime? startDate, DateTime? endDate)
    {
        return x => !x.IsDeleted &&
                   (startDate == null || x.CreatedAt >= startDate) &&
                   (endDate == null || x.CreatedAt <= endDate);
    }
    private static System.Linq.Expressions.Expression<Func<CashPaymentAuditLog, bool>> BuildSecurityIncidentPredicate(DateTime? startDate, DateTime? endDate, SecurityRiskLevel? minRiskLevel)
    {
        return x => !x.IsDeleted &&
                   x.SeverityLevel == AuditSeverityLevel.Security &&
                   (startDate == null || x.CreatedAt >= startDate) &&
                   (endDate == null || x.CreatedAt <= endDate) &&
                   (minRiskLevel == null || (x.RiskLevel.HasValue && x.RiskLevel >= minRiskLevel));
    }
    private static System.Linq.Expressions.Expression<Func<CashPaymentAuditLog, bool>> BuildCriticalEventPredicate(DateTime? startDate, DateTime? endDate)
    {
        return x => !x.IsDeleted &&
                   (x.SeverityLevel == AuditSeverityLevel.Critical || x.RiskLevel == SecurityRiskLevel.Critical) &&
                   (startDate == null || x.CreatedAt >= startDate) &&
                   (endDate == null || x.CreatedAt <= endDate);
    }
    private static SecurityRiskLevel CalculateOverallRiskLevel(List<CashPaymentAuditLog> riskLogs)
    {
        if (!riskLogs.Any()) return SecurityRiskLevel.Low;

        var maxRisk = riskLogs.Max(x => x.RiskLevel ?? SecurityRiskLevel.Low);
        var criticalCount = riskLogs.Count(x => x.RiskLevel == SecurityRiskLevel.Critical);
        var highCount = riskLogs.Count(x => x.RiskLevel == SecurityRiskLevel.High);

        if (criticalCount > 0) return SecurityRiskLevel.Critical;
        if (highCount > 2) return SecurityRiskLevel.High;
        if (highCount > 0) return SecurityRiskLevel.Medium;
        return SecurityRiskLevel.Low;
    }
    private static List<RiskTrend> CalculateRiskTrends(List<CashPaymentAuditLog> riskLogs)
    {
        var trends = new List<RiskTrend>();
        var groupedByDate = riskLogs.GroupBy(x => x.CreatedAt.Date);

        foreach (var group in groupedByDate.OrderBy(x => x.Key))
        {
            var highestRisk = group.Max(x => x.RiskLevel ?? SecurityRiskLevel.Low);
            var mostCommonEvent = group.GroupBy(x => x.EventType).OrderByDescending(x => x.Count()).First().Key;

            trends.Add(new RiskTrend(
                group.Key,
                group.Count(),
                highestRisk,
                mostCommonEvent
            ));
        }

        return trends;
    }
    private static List<string> GenerateRiskRecommendations(List<CashPaymentAuditLog> riskLogs)
    {
        var recommendations = new List<string>();

        var fakeMoneyCount = riskLogs.Count(x => x.EventType == AuditEventType.FakeMoneyDetected);
        if (fakeMoneyCount > 0)
        {
            recommendations.Add($"Sahte para tespit edildi ({fakeMoneyCount} olay). Kurye eğitimini güçlendirin.");
        }

        var changeErrorCount = riskLogs.Count(x => x.EventType == AuditEventType.ChangeCalculationError);
        if (changeErrorCount > 0)
        {
            recommendations.Add($"Para üstü hesaplama hataları ({changeErrorCount} olay). Hesaplama sistemini gözden geçirin.");
        }

        var identityFailCount = riskLogs.Count(x => x.EventType == AuditEventType.IdentityVerificationFailed);
        if (identityFailCount > 0)
        {
            recommendations.Add($"Kimlik doğrulama başarısızlıkları ({identityFailCount} olay). Kimlik doğrulama sürecini iyileştirin.");
        }

        var unauthorizedCount = riskLogs.Count(x => x.EventType == AuditEventType.UnauthorizedAccessAttempt);
        if (unauthorizedCount > 0)
        {
            recommendations.Add($"Yetkisiz erişim denemeleri ({unauthorizedCount} olay). Güvenlik önlemlerini artırın.");
        }

        if (!recommendations.Any())
        {
            recommendations.Add("Risk seviyesi düşük. Mevcut güvenlik önlemlerini sürdürün.");
        }

        return recommendations;
    }
    private static List<ComplianceViolation> IdentifyComplianceViolations(IEnumerable<CashPaymentAuditLog> auditLogs)
    {
        var violations = new List<ComplianceViolation>();

        var fakeMoneyLogs = auditLogs.Where(x => x.EventType == AuditEventType.FakeMoneyDetected).ToList();
        if (fakeMoneyLogs.Any())
        {
            violations.Add(new ComplianceViolation(
                "FakeMoneyDetection",
                "Sahte para tespit edildi",
                fakeMoneyLogs.Count,
                SecurityRiskLevel.High,
                fakeMoneyLogs.Select(x => x.Id).ToList()
            ));
        }

        var unauthorizedLogs = auditLogs.Where(x => x.EventType == AuditEventType.UnauthorizedAccessAttempt).ToList();
        if (unauthorizedLogs.Any())
        {
            violations.Add(new ComplianceViolation(
                "UnauthorizedAccess",
                "Yetkisiz erişim denemesi",
                unauthorizedLogs.Count,
                SecurityRiskLevel.Critical,
                unauthorizedLogs.Select(x => x.Id).ToList()
            ));
        }

        var rejectedEvidenceLogs = auditLogs.Where(x => x.EventType == AuditEventType.EvidenceRejected).ToList();
        if (rejectedEvidenceLogs.Count > 5)
        {
            violations.Add(new ComplianceViolation(
                "HighEvidenceRejection",
                "Yüksek kanıt red oranı",
                rejectedEvidenceLogs.Count,
                SecurityRiskLevel.Medium,
                rejectedEvidenceLogs.Select(x => x.Id).ToList()
            ));
        }

        return violations;
    }
    private static ComplianceScore CalculateComplianceScore(IEnumerable<CashPaymentAuditLog> auditLogs, List<ComplianceViolation> violations)
    {
        var totalLogs = auditLogs.Count();
        var criticalViolations = violations.Count(v => v.RiskLevel == SecurityRiskLevel.Critical);
        var highViolations = violations.Count(v => v.RiskLevel == SecurityRiskLevel.High);
        var mediumViolations = violations.Count(v => v.RiskLevel == SecurityRiskLevel.Medium);

        var score = 100;
        score -= criticalViolations * 20;
        score -= highViolations * 10;
        score -= mediumViolations * 5;

        score = Math.Max(0, score);

        var grade = score switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _ => "F"
        };

        var description = grade switch
        {
            "A" => "Mükemmel compliance seviyesi",
            "B" => "İyi compliance seviyesi",
            "C" => "Orta compliance seviyesi",
            "D" => "Düşük compliance seviyesi",
            _ => "Kritik compliance sorunları"
        };

        var improvementAreas = new List<string>();
        if (criticalViolations > 0) improvementAreas.Add("Kritik güvenlik ihlallerini çözün");
        if (highViolations > 0) improvementAreas.Add("Yüksek riskli olayları azaltın");
        if (mediumViolations > 0) improvementAreas.Add("Orta seviye riskleri gözden geçirin");

        return new ComplianceScore(score, grade, description, improvementAreas);
    }
}
