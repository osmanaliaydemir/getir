using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Nakit ödeme denetim işlemleri için nakit ödeme denetim controller'ı
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
    /// Denetim kaydı oluştur (yalnızca sistem)
    /// </summary>
    /// <param name="request">Denetim kaydı oluşturma talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan denetim kaydı</returns>
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
    /// Denetim kaydını güncelle
    /// </summary>
    /// <param name="auditLogId">Denetim kaydı ID'si</param>
    /// <param name="request">Denetim kaydı güncelleme talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenen denetim kaydı</returns>
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
    /// Filtreleme ile denetim kayıtlarını getir
    /// </summary>
    /// <param name="paymentId">Ödeme ID'si</param>
    /// <param name="courierId">Kurye ID'si</param>
    /// <param name="customerId">Müşteri ID'si</param>
    /// <param name="adminId">Admin ID'si</param>
    /// <param name="eventType">Olay türü</param>
    /// <param name="severityLevel">Ciddiyet seviyesi</param>
    /// <param name="riskLevel">Risk seviyesi</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="searchTerm">Arama terimi</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış denetim kayıtları</returns>
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
    /// ID'ye göre denetim kaydını getir
    /// </summary>
    /// <param name="auditLogId">Denetim kaydı ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Denetim kaydı</returns>
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
    /// Ödeme ID'sine göre denetim kayıtlarını getir
    /// </summary>
    /// <param name="paymentId">Ödeme ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Ödemeye ait denetim kayıtları</returns>
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
    /// Kurye ID'sine göre denetim kayıtlarını getir
    /// </summary>
    /// <param name="courierId">Kurye ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kuryeye ait denetim kayıtları</returns>
    [HttpGet("logs/courier/{courierId}")]
    [Authorize(Roles = "Admin,Courier")]
    [ProducesResponseType(typeof(IEnumerable<CashPaymentAuditLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditLogsByCourierId(
        [FromRoute] Guid courierId,
        CancellationToken ct = default)
    {
        // Kurye yalnızca kendi kayıtlarını görebilir
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
    /// Denetim kaydı istatistiklerini getir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Denetim kaydı istatistikleri</returns>
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
    /// Risk analizi yap
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Risk analizi</returns>
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
    /// Uyumluluk raporu oluştur
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Uyumluluk raporu</returns>
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
    /// Güvenlik olaylarını getir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="minRiskLevel">Minimum risk seviyesi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güvenlik olayları</returns>
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
    /// Kritik olayları getir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kritik olaylar</returns>
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
    /// Denetim kaydını sil (soft delete)
    /// </summary>
    /// <param name="auditLogId">Denetim kaydı ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
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
    /// Eski denetim kayıtlarını temizle
    /// </summary>
    /// <param name="cutoffDate">Eşik tarih</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Silinen kayıt sayısı</returns>
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
    /// Denetim kayıtlarını arşivle
    /// </summary>
    /// <param name="cutoffDate">Eşik tarih</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
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
