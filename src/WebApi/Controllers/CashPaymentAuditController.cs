using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Cash payment audit controller for cash payment audit operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Cash Payment Audit")]
public class CashPaymentAuditController : BaseController
{
    private readonly ICashPaymentAuditService _cashPaymentAuditService;

    public CashPaymentAuditController(ICashPaymentAuditService cashPaymentAuditService)
    {
        _cashPaymentAuditService = cashPaymentAuditService;
    }

    /// <summary>
    /// Create audit log (system only)
    /// </summary>
    /// <param name="request">Create audit log request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created audit log</returns>
    [HttpPost("logs")]
    [Authorize(Roles = "Admin,System")]
    [ProducesResponseType(typeof(CashPaymentAuditLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAuditLog(
        [FromBody] CreateAuditLogRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _cashPaymentAuditService.CreateAuditLogAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update audit log
    /// </summary>
    /// <param name="auditLogId">Audit log ID</param>
    /// <param name="request">Update audit log request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated audit log</returns>
    [HttpPut("logs/{auditLogId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CashPaymentAuditLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAuditLog(
        [FromRoute] Guid auditLogId,
        [FromBody] UpdateAuditLogRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _cashPaymentAuditService.UpdateAuditLogAsync(auditLogId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get audit logs with filtering
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="courierId">Courier ID</param>
    /// <param name="customerId">Customer ID</param>
    /// <param name="adminId">Admin ID</param>
    /// <param name="eventType">Event type</param>
    /// <param name="severityLevel">Severity level</param>
    /// <param name="riskLevel">Risk level</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="searchTerm">Search term</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged audit logs</returns>
    [HttpGet("logs")]
    [Authorize(Roles = "Admin,Courier")]
    [ProducesResponseType(typeof(PagedResult<CashPaymentAuditLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] Guid? paymentId = null,
        [FromQuery] Guid? courierId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? adminId = null,
        [FromQuery] int? eventType = null,
        [FromQuery] int? severityLevel = null,
        [FromQuery] int? riskLevel = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new CashPaymentAuditLogQuery
        {
            PaymentId = paymentId,
            CourierId = courierId,
            CustomerId = customerId,
            AdminId = adminId,
            EventType = eventType.HasValue ? (Domain.Enums.AuditEventType?)eventType.Value : null,
            SeverityLevel = severityLevel.HasValue ? (Domain.Enums.AuditSeverityLevel?)severityLevel.Value : null,
            RiskLevel = riskLevel.HasValue ? (Domain.Enums.SecurityRiskLevel?)riskLevel.Value : null,
            StartDate = startDate,
            EndDate = endDate,
            SearchTerm = searchTerm,
            Page = page,
            PageSize = pageSize
        };

        var result = await _cashPaymentAuditService.GetAuditLogsAsync(query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get audit log by ID
    /// </summary>
    /// <param name="auditLogId">Audit log ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Audit log</returns>
    [HttpGet("logs/{auditLogId}")]
    [Authorize(Roles = "Admin,Courier")]
    [ProducesResponseType(typeof(CashPaymentAuditLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditLogById(
        [FromRoute] Guid auditLogId,
        CancellationToken ct = default)
    {
        var result = await _cashPaymentAuditService.GetAuditLogByIdAsync(auditLogId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get audit logs by payment ID
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Audit logs for payment</returns>
    [HttpGet("logs/payment/{paymentId}")]
    [Authorize(Roles = "Admin,Courier")]
    [ProducesResponseType(typeof(IEnumerable<CashPaymentAuditLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditLogsByPaymentId(
        [FromRoute] Guid paymentId,
        CancellationToken ct = default)
    {
        var result = await _cashPaymentAuditService.GetAuditLogsByPaymentIdAsync(paymentId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get audit logs by courier ID
    /// </summary>
    /// <param name="courierId">Courier ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Audit logs for courier</returns>
    [HttpGet("logs/courier/{courierId}")]
    [Authorize(Roles = "Admin,Courier")]
    [ProducesResponseType(typeof(IEnumerable<CashPaymentAuditLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditLogsByCourierId(
        [FromRoute] Guid courierId,
        CancellationToken ct = default)
    {
        // Courier can only see their own logs
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var currentCourierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        if (currentCourierId != Guid.Empty && currentCourierId != courierId)
        {
            return Forbid();
        }

        var result = await _cashPaymentAuditService.GetAuditLogsByCourierIdAsync(courierId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get audit log statistics
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Audit log statistics</returns>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AuditLogStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var result = await _cashPaymentAuditService.GetAuditLogStatisticsAsync(startDate, endDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Perform risk analysis
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Risk analysis</returns>
    [HttpGet("risk-analysis")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(RiskAnalysisResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PerformRiskAnalysis(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var result = await _cashPaymentAuditService.PerformRiskAnalysisAsync(startDate, endDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Generate compliance report
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Compliance report</returns>
    [HttpGet("compliance-report")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ComplianceReportResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateComplianceReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken ct = default)
    {
        var result = await _cashPaymentAuditService.GenerateComplianceReportAsync(startDate, endDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get security incidents
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="minRiskLevel">Minimum risk level</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Security incidents</returns>
    [HttpGet("security-incidents")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<CashPaymentAuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSecurityIncidents(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? minRiskLevel = null,
        CancellationToken ct = default)
    {
        var result = await _cashPaymentAuditService.GetSecurityIncidentsAsync(
            startDate, 
            endDate, 
            minRiskLevel.HasValue ? (Domain.Enums.SecurityRiskLevel?)minRiskLevel.Value : null, 
            ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get critical events
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Critical events</returns>
    [HttpGet("critical-events")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<CashPaymentAuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCriticalEvents(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var result = await _cashPaymentAuditService.GetCriticalEventsAsync(startDate, endDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete audit log (soft delete)
    /// </summary>
    /// <param name="auditLogId">Audit log ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("logs/{auditLogId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAuditLog(
        [FromRoute] Guid auditLogId,
        CancellationToken ct = default)
    {
        var result = await _cashPaymentAuditService.DeleteAuditLogAsync(auditLogId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Cleanup old audit logs
    /// </summary>
    /// <param name="cutoffDate">Cutoff date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Number of deleted logs</returns>
    [HttpPost("cleanup")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> CleanupOldAuditLogs(
        [FromQuery] DateTime cutoffDate,
        CancellationToken ct = default)
    {
        var result = await _cashPaymentAuditService.CleanupOldAuditLogsAsync(cutoffDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Archive audit logs
    /// </summary>
    /// <param name="cutoffDate">Cutoff date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("archive")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ArchiveAuditLogs(
        [FromQuery] DateTime cutoffDate,
        CancellationToken ct = default)
    {
        var result = await _cashPaymentAuditService.ArchiveAuditLogsAsync(cutoffDate, ct);
        return ToActionResult(result);
    }
}
