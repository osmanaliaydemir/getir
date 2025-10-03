using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Payments;
using Getir.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Getir.WebApi.Endpoints;

/// <summary>
/// Nakit ödeme audit log endpoint'leri
/// </summary>
public static class CashPaymentAuditEndpoints
{
    public static void MapCashPaymentAuditEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/cash-payment-audit")
            .WithTags("Cash Payment Audit");

        // Audit log oluşturma (sistem tarafından)
        group.MapPost("/logs", async (
            [FromBody] CreateAuditLogRequest request,
            [FromServices] ICashPaymentAuditService service,
            CancellationToken ct) =>
        {
            var result = await service.CreateAuditLogAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("CreateAuditLog")
        .Produces<CashPaymentAuditLogResponse>(200)
        .RequireAuthorization("Admin", "System");

        // Audit log güncelleme
        group.MapPut("/logs/{auditLogId}", async (
            [FromRoute] Guid auditLogId,
            [FromBody] UpdateAuditLogRequest request,
            [FromServices] ICashPaymentAuditService service,
            CancellationToken ct) =>
        {
            var result = await service.UpdateAuditLogAsync(auditLogId, request, ct);
            return result.ToIResult();
        })
        .WithName("UpdateAuditLog")
        .Produces<CashPaymentAuditLogResponse>(200)
        .RequireAuthorization("Admin");

        // Audit log'ları sorgulama
        group.MapGet("/logs", async (
            [FromQuery] Guid? paymentId,
            [FromQuery] Guid? courierId,
            [FromQuery] Guid? customerId,
            [FromQuery] Guid? adminId,
            [FromQuery] int? eventType,
            [FromQuery] int? severityLevel,
            [FromQuery] int? riskLevel,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? searchTerm,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromServices] ICashPaymentAuditService service,
            CancellationToken ct) =>
        {
            var query = new CashPaymentAuditLogQuery(
                paymentId,
                courierId,
                customerId,
                adminId,
                eventType.HasValue ? (Domain.Enums.AuditEventType?)eventType.Value : null,
                severityLevel.HasValue ? (Domain.Enums.AuditSeverityLevel?)severityLevel.Value : null,
                riskLevel.HasValue ? (Domain.Enums.SecurityRiskLevel?)riskLevel.Value : null,
                startDate,
                endDate,
                searchTerm,
                page,
                pageSize
            );

            var result = await service.GetAuditLogsAsync(query, ct);
            return result.ToIResult();
        })
        .WithName("GetAuditLogs")
        .Produces<PagedResult<CashPaymentAuditLogResponse>>(200)
        .RequireAuthorization("Admin", "Courier");

        // Audit log'u ID ile getir
        group.MapGet("/logs/{auditLogId}", async (
            [FromRoute] Guid auditLogId,
            [FromServices] ICashPaymentAuditService service,
            CancellationToken ct) =>
        {
            var result = await service.GetAuditLogByIdAsync(auditLogId, ct);
            return result.ToIResult();
        })
        .WithName("GetAuditLogById")
        .Produces<CashPaymentAuditLogResponse>(200)
        .RequireAuthorization("Admin", "Courier");

        // Ödeme için audit log'ları getir
        group.MapGet("/logs/payment/{paymentId}", async (
            [FromRoute] Guid paymentId,
            [FromServices] ICashPaymentAuditService service,
            CancellationToken ct) =>
        {
            var result = await service.GetAuditLogsByPaymentIdAsync(paymentId, ct);
            return result.ToIResult();
        })
        .WithName("GetAuditLogsByPaymentId")
        .Produces<IEnumerable<CashPaymentAuditLogResponse>>(200)
        .RequireAuthorization("Admin", "Courier");

        // Kurye için audit log'ları getir
        group.MapGet("/logs/courier/{courierId}", async (
            [FromRoute] Guid courierId,
            [FromServices] ICashPaymentAuditService service,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            // Kurye sadece kendi log'larını görebilir
            var currentCourierId = user.GetCourierId();
            if (currentCourierId != Guid.Empty && currentCourierId != courierId)
            {
                return Results.Forbid();
            }

            var result = await service.GetAuditLogsByCourierIdAsync(courierId, ct);
            return result.ToIResult();
        })
        .WithName("GetAuditLogsByCourierId")
        .Produces<IEnumerable<CashPaymentAuditLogResponse>>(200)
        .RequireAuthorization("Admin", "Courier");

        // Audit log istatistikleri
        group.MapGet("/statistics", async (
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromServices] ICashPaymentAuditService service,
            CancellationToken ct) =>
        {
            var result = await service.GetAuditLogStatisticsAsync(startDate, endDate, ct);
            return result.ToIResult();
        })
        .WithName("GetAuditLogStatistics")
        .Produces<AuditLogStatisticsResponse>(200)
        .RequireAuthorization("Admin");

        // Risk analizi
        group.MapGet("/risk-analysis", async (
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromServices] ICashPaymentAuditService service,
            CancellationToken ct) =>
        {
            var result = await service.PerformRiskAnalysisAsync(startDate, endDate, ct);
            return result.ToIResult();
        })
        .WithName("PerformRiskAnalysis")
        .Produces<RiskAnalysisResponse>(200)
        .RequireAuthorization("Admin");

        // Compliance raporu
        group.MapGet("/compliance-report", async (
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromServices] ICashPaymentAuditService service,
            CancellationToken ct) =>
        {
            var result = await service.GenerateComplianceReportAsync(startDate, endDate, ct);
            return result.ToIResult();
        })
        .WithName("GenerateComplianceReport")
        .Produces<ComplianceReportResponse>(200)
        .RequireAuthorization("Admin");

        // Güvenlik olayları
        group.MapGet("/security-incidents", async (
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? minRiskLevel,
            [FromServices] ICashPaymentAuditService service,
            CancellationToken ct) =>
        {
            var result = await service.GetSecurityIncidentsAsync(
                startDate, 
                endDate, 
                minRiskLevel.HasValue ? (Domain.Enums.SecurityRiskLevel?)minRiskLevel.Value : null, 
                ct);
            return result.ToIResult();
        })
        .WithName("GetSecurityIncidents")
        .Produces<IEnumerable<CashPaymentAuditLogResponse>>(200)
        .RequireAuthorization("Admin");

        // Kritik olaylar
        group.MapGet("/critical-events", async (
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromServices] ICashPaymentAuditService service,
            CancellationToken ct) =>
        {
            var result = await service.GetCriticalEventsAsync(startDate, endDate, ct);
            return result.ToIResult();
        })
        .WithName("GetCriticalEvents")
        .Produces<IEnumerable<CashPaymentAuditLogResponse>>(200)
        .RequireAuthorization("Admin");

        // Audit log silme (soft delete)
        group.MapDelete("/logs/{auditLogId}", async (
            [FromRoute] Guid auditLogId,
            [FromServices] ICashPaymentAuditService service,
            CancellationToken ct) =>
        {
            var result = await service.DeleteAuditLogAsync(auditLogId, ct);
            return result.ToIResult();
        })
        .WithName("DeleteAuditLog")
        .Produces(200)
        .RequireAuthorization("Admin");

        // Eski audit log'ları temizleme
        group.MapPost("/cleanup", async (
            [FromQuery] DateTime cutoffDate,
            [FromServices] ICashPaymentAuditService service,
            CancellationToken ct) =>
        {
            var result = await service.CleanupOldAuditLogsAsync(cutoffDate, ct);
            return result.ToIResult();
        })
        .WithName("CleanupOldAuditLogs")
        .Produces<int>(200)
        .RequireAuthorization("Admin");

        // Audit log'ları arşivleme
        group.MapPost("/archive", async (
            [FromQuery] DateTime cutoffDate,
            [FromServices] ICashPaymentAuditService service,
            CancellationToken ct) =>
        {
            var result = await service.ArchiveAuditLogsAsync(cutoffDate, ct);
            return result.ToIResult();
        })
        .WithName("ArchiveAuditLogs")
        .Produces(200)
        .RequireAuthorization("Admin");
    }
}
