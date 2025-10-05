using Getir.Application.DTO;
using Getir.Application.Services.AuditLogging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Kapsamlı audit logging sistemi için API endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditLoggingController : BaseController
{
    private readonly IUserActivityLogService _userActivityLogService;
    private readonly ISystemChangeLogService _systemChangeLogService;
    private readonly ISecurityEventLogService _securityEventLogService;
    private readonly ILogAnalysisService _logAnalysisService;

    public AuditLoggingController(
        IUserActivityLogService userActivityLogService,
        ISystemChangeLogService systemChangeLogService,
        ISecurityEventLogService securityEventLogService,
        ILogAnalysisService logAnalysisService)
    {
        _userActivityLogService = userActivityLogService;
        _systemChangeLogService = systemChangeLogService;
        _securityEventLogService = securityEventLogService;
        _logAnalysisService = logAnalysisService;
    }

    #region User Activity Logs

    /// <summary>
    /// Kullanıcı aktivite log'u oluşturur
    /// </summary>
    [HttpPost("user-activity")]
    [ProducesResponseType(typeof(UserActivityLogResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateUserActivityLog([FromBody] CreateUserActivityLogRequest request)
    {
        var result = await _userActivityLogService.CreateUserActivityLogAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to create user activity log", ErrorCode = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetUserActivityLog), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Kullanıcı aktivite log'unu getirir
    /// </summary>
    [HttpGet("user-activity/{id}")]
    [ProducesResponseType(typeof(UserActivityLogResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserActivityLog(Guid id)
    {
        var result = await _userActivityLogService.GetUserActivityLogByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(new { Error = result.Error ?? "User activity log not found", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Kullanıcı aktivite log'larını getirir
    /// </summary>
    [HttpGet("user-activity")]
    [ProducesResponseType(typeof(IEnumerable<UserActivityLogResponse>), 200)]
    public async Task<IActionResult> GetUserActivityLogs([FromQuery] UserActivityQueryRequest request)
    {
        var result = await _userActivityLogService.GetUserActivityLogsAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get user activity logs", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Kullanıcıya göre aktivite log'larını getirir
    /// </summary>
    [HttpGet("user-activity/user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<UserActivityLogResponse>), 200)]
    public async Task<IActionResult> GetUserActivityLogsByUser(
        Guid userId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _userActivityLogService.GetUserActivityLogsByUserIdAsync(
            userId, startDate, endDate, pageNumber, pageSize);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get user activity logs", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Aktivite tipine göre log'ları getirir
    /// </summary>
    [HttpGet("user-activity/type/{activityType}")]
    [ProducesResponseType(typeof(IEnumerable<UserActivityLogResponse>), 200)]
    public async Task<IActionResult> GetUserActivityLogsByType(
        string activityType,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _userActivityLogService.GetUserActivityLogsByActivityTypeAsync(
            activityType, startDate, endDate, pageNumber, pageSize);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get user activity logs", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Kullanıcı aktivite istatistiklerini getirir
    /// </summary>
    [HttpGet("user-activity/statistics")]
    [ProducesResponseType(typeof(Dictionary<string, int>), 200)]
    public async Task<IActionResult> GetUserActivityStatistics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? userId = null)
    {
        var result = await _userActivityLogService.GetUserActivityStatisticsAsync(startDate, endDate, userId);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get user activity statistics", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Şüpheli aktiviteleri getirir
    /// </summary>
    [HttpGet("user-activity/suspicious")]
    [ProducesResponseType(typeof(IEnumerable<UserActivityLogResponse>), 200)]
    public async Task<IActionResult> GetSuspiciousActivities(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int threshold = 10)
    {
        var result = await _userActivityLogService.GetSuspiciousActivitiesAsync(startDate, endDate, threshold);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get suspicious activities", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    #endregion

    #region System Change Logs

    /// <summary>
    /// Sistem değişiklik log'u oluşturur
    /// </summary>
    [HttpPost("system-change")]
    [ProducesResponseType(typeof(SystemChangeLogResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateSystemChangeLog([FromBody] CreateSystemChangeLogRequest request)
    {
        var result = await _systemChangeLogService.CreateSystemChangeLogAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to create system change log", ErrorCode = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetSystemChangeLog), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Sistem değişiklik log'unu getirir
    /// </summary>
    [HttpGet("system-change/{id}")]
    [ProducesResponseType(typeof(SystemChangeLogResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSystemChangeLog(Guid id)
    {
        var result = await _systemChangeLogService.GetSystemChangeLogByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(new { Error = result.Error ?? "System change log not found", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Sistem değişiklik log'larını getirir
    /// </summary>
    [HttpGet("system-change")]
    [ProducesResponseType(typeof(IEnumerable<SystemChangeLogResponse>), 200)]
    public async Task<IActionResult> GetSystemChangeLogs([FromQuery] SystemChangeQueryRequest request)
    {
        var result = await _systemChangeLogService.GetSystemChangeLogsAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get system change logs", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Entity'ye göre değişiklik log'larını getirir
    /// </summary>
    [HttpGet("system-change/entity/{entityType}/{entityId}")]
    [ProducesResponseType(typeof(IEnumerable<SystemChangeLogResponse>), 200)]
    public async Task<IActionResult> GetSystemChangeLogsByEntity(
        string entityType,
        string entityId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _systemChangeLogService.GetSystemChangeLogsByEntityAsync(
            entityType, entityId, startDate, endDate, pageNumber, pageSize);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get system change logs", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Kullanıcıya göre değişiklik log'larını getirir
    /// </summary>
    [HttpGet("system-change/user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<SystemChangeLogResponse>), 200)]
    public async Task<IActionResult> GetSystemChangeLogsByUser(
        Guid userId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _systemChangeLogService.GetSystemChangeLogsByUserAsync(
            userId, startDate, endDate, pageNumber, pageSize);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get system change logs", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Sistem değişiklik istatistiklerini getirir
    /// </summary>
    [HttpGet("system-change/statistics")]
    [ProducesResponseType(typeof(Dictionary<string, int>), 200)]
    public async Task<IActionResult> GetSystemChangeStatistics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? entityType = null)
    {
        var result = await _systemChangeLogService.GetSystemChangeStatisticsAsync(startDate, endDate, entityType);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get system change statistics", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Son sistem değişikliklerini getirir
    /// </summary>
    [HttpGet("system-change/recent")]
    [ProducesResponseType(typeof(IEnumerable<SystemChangeLogResponse>), 200)]
    public async Task<IActionResult> GetRecentSystemChanges(
        [FromQuery] int count = 50,
        [FromQuery] string? entityType = null)
    {
        var result = await _systemChangeLogService.GetRecentSystemChangesAsync(count, entityType);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get recent system changes", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    #endregion

    #region Security Event Logs

    /// <summary>
    /// Güvenlik event log'u oluşturur
    /// </summary>
    [HttpPost("security-event")]
    [ProducesResponseType(typeof(SecurityEventLogResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateSecurityEventLog([FromBody] CreateSecurityEventLogRequest request)
    {
        var result = await _securityEventLogService.CreateSecurityEventLogAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to create security event log", ErrorCode = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetSecurityEventLog), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Güvenlik event log'unu getirir
    /// </summary>
    [HttpGet("security-event/{id}")]
    [ProducesResponseType(typeof(SecurityEventLogResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSecurityEventLog(Guid id)
    {
        var result = await _securityEventLogService.GetSecurityEventLogByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(new { Error = result.Error ?? "Security event log not found", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Güvenlik event log'larını getirir
    /// </summary>
    [HttpGet("security-event")]
    [ProducesResponseType(typeof(IEnumerable<SecurityEventLogResponse>), 200)]
    public async Task<IActionResult> GetSecurityEventLogs([FromQuery] SecurityEventQueryRequest request)
    {
        var result = await _securityEventLogService.GetSecurityEventLogsAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get security event logs", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Çözülmemiş güvenlik event'lerini getirir
    /// </summary>
    [HttpGet("security-event/unresolved")]
    [ProducesResponseType(typeof(IEnumerable<SecurityEventLogResponse>), 200)]
    public async Task<IActionResult> GetUnresolvedSecurityEvents(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? severity = null,
        [FromQuery] string? riskLevel = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _securityEventLogService.GetUnresolvedSecurityEventsAsync(
            startDate, endDate, severity, riskLevel, pageNumber, pageSize);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get unresolved security events", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// İnceleme gerektiren güvenlik event'lerini getirir
    /// </summary>
    [HttpGet("security-event/requires-investigation")]
    [ProducesResponseType(typeof(IEnumerable<SecurityEventLogResponse>), 200)]
    public async Task<IActionResult> GetSecurityEventsRequiringInvestigation(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _securityEventLogService.GetSecurityEventsRequiringInvestigationAsync(
            startDate, endDate, pageNumber, pageSize);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get security events requiring investigation", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Güvenlik event'ini çözer
    /// </summary>
    [HttpPost("security-event/{id}/resolve")]
    [ProducesResponseType(typeof(SecurityEventLogResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResolveSecurityEvent(
        Guid id,
        [FromBody] ResolveSecurityEventRequest request)
    {
        var result = await _securityEventLogService.ResolveSecurityEventAsync(
            id, request.ResolvedBy, request.ResolutionNotes);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to resolve security event", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Güvenlik event'ini false positive olarak işaretler
    /// </summary>
    [HttpPost("security-event/{id}/mark-false-positive")]
    [ProducesResponseType(typeof(SecurityEventLogResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> MarkSecurityEventAsFalsePositive(
        Guid id,
        [FromBody] MarkFalsePositiveRequest request)
    {
        var result = await _securityEventLogService.MarkAsFalsePositiveAsync(
            id, request.ResolvedBy, request.ResolutionNotes);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to mark security event as false positive", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Güvenlik event istatistiklerini getirir
    /// </summary>
    [HttpGet("security-event/statistics")]
    [ProducesResponseType(typeof(Dictionary<string, int>), 200)]
    public async Task<IActionResult> GetSecurityEventStatistics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? severity = null,
        [FromQuery] string? riskLevel = null)
    {
        var result = await _securityEventLogService.GetSecurityEventStatisticsAsync(
            startDate, endDate, severity, riskLevel);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get security event statistics", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Yüksek riskli güvenlik event'lerini getirir
    /// </summary>
    [HttpGet("security-event/high-risk")]
    [ProducesResponseType(typeof(IEnumerable<SecurityEventLogResponse>), 200)]
    public async Task<IActionResult> GetHighRiskSecurityEvents(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _securityEventLogService.GetHighRiskSecurityEventsAsync(
            startDate, endDate, pageNumber, pageSize);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get high risk security events", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    #endregion

    #region Log Analysis & Reporting

    /// <summary>
    /// Log analiz raporu oluşturur
    /// </summary>
    [HttpPost("analysis-report")]
    [ProducesResponseType(typeof(LogAnalysisReportResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateLogAnalysisReport([FromBody] CreateLogAnalysisReportRequest request)
    {
        var result = await _logAnalysisService.CreateLogAnalysisReportAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to create log analysis report", ErrorCode = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetLogAnalysisReport), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Log analiz raporunu getirir
    /// </summary>
    [HttpGet("analysis-report/{id}")]
    [ProducesResponseType(typeof(LogAnalysisReportResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetLogAnalysisReport(Guid id)
    {
        var result = await _logAnalysisService.GetLogAnalysisReportByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(new { Error = result.Error ?? "Log analysis report not found", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Log analiz raporlarını getirir
    /// </summary>
    [HttpGet("analysis-report")]
    [ProducesResponseType(typeof(IEnumerable<LogAnalysisReportResponse>), 200)]
    public async Task<IActionResult> GetLogAnalysisReports(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? reportType = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? generatedByUserId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _logAnalysisService.GetLogAnalysisReportsAsync(
            startDate, endDate, reportType, status, generatedByUserId, pageNumber, pageSize);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get log analysis reports", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Audit log analitiklerini oluşturur
    /// </summary>
    [HttpPost("analytics/audit-log")]
    [ProducesResponseType(typeof(AuditLogAnalyticsResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GenerateAuditLogAnalytics([FromBody] AuditLogAnalyticsRequest request)
    {
        var result = await _logAnalysisService.GenerateAuditLogAnalyticsAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to generate audit log analytics", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Kullanıcı aktivite analitiklerini oluşturur
    /// </summary>
    [HttpPost("analytics/user-activity")]
    [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GenerateUserActivityAnalytics(
        [FromBody] UserActivityAnalyticsRequest request)
    {
        var result = await _logAnalysisService.GenerateUserActivityAnalyticsAsync(
            request.StartDate, request.EndDate, request.UserId, request.ActivityType);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to generate user activity analytics", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Sistem değişiklik analitiklerini oluşturur
    /// </summary>
    [HttpPost("analytics/system-change")]
    [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GenerateSystemChangeAnalytics(
        [FromBody] SystemChangeAnalyticsRequest request)
    {
        var result = await _logAnalysisService.GenerateSystemChangeAnalyticsAsync(
            request.StartDate, request.EndDate, request.EntityType, request.ChangeType);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to generate system change analytics", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Güvenlik event analitiklerini oluşturur
    /// </summary>
    [HttpPost("analytics/security-event")]
    [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GenerateSecurityEventAnalytics(
        [FromBody] SecurityEventAnalyticsRequest request)
    {
        var result = await _logAnalysisService.GenerateSecurityEventAnalyticsAsync(
            request.StartDate, request.EndDate, request.Severity, request.RiskLevel);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to generate security event analytics", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Raporu yeniden oluşturur
    /// </summary>
    [HttpPost("analysis-report/{id}/regenerate")]
    [ProducesResponseType(typeof(LogAnalysisReportResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RegenerateReport(Guid id)
    {
        var result = await _logAnalysisService.RegenerateReportAsync(id);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to regenerate report", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Raporu export eder
    /// </summary>
    [HttpGet("analysis-report/{id}/export")]
    [ProducesResponseType(typeof(byte[]), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ExportReport(
        Guid id,
        [FromQuery] string format = "PDF")
    {
        var result = await _logAnalysisService.ExportReportAsync(id, format);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to export report", ErrorCode = result.ErrorCode });
        }

        return File(result.Value!, "application/octet-stream", $"report_{id}.{format.ToLower()}");
    }

    #endregion

    #region Maintenance Operations

    /// <summary>
    /// Eski kullanıcı aktivite log'larını siler
    /// </summary>
    [HttpDelete("user-activity/cleanup")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> DeleteOldUserActivityLogs([FromQuery] DateTime cutoffDate)
    {
        var result = await _userActivityLogService.DeleteOldUserActivityLogsAsync(cutoffDate);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to delete old user activity logs", ErrorCode = result.ErrorCode });
        }

        return NoContent();
    }

    /// <summary>
    /// Eski sistem değişiklik log'larını siler
    /// </summary>
    [HttpDelete("system-change/cleanup")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> DeleteOldSystemChangeLogs([FromQuery] DateTime cutoffDate)
    {
        var result = await _systemChangeLogService.DeleteOldSystemChangeLogsAsync(cutoffDate);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to delete old system change logs", ErrorCode = result.ErrorCode });
        }

        return NoContent();
    }

    /// <summary>
    /// Eski güvenlik event log'larını siler
    /// </summary>
    [HttpDelete("security-event/cleanup")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> DeleteOldSecurityEventLogs([FromQuery] DateTime cutoffDate)
    {
        var result = await _securityEventLogService.DeleteOldSecurityEventLogsAsync(cutoffDate);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to delete old security event logs", ErrorCode = result.ErrorCode });
        }

        return NoContent();
    }

    /// <summary>
    /// Süresi dolmuş raporları siler
    /// </summary>
    [HttpDelete("analysis-report/cleanup")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> DeleteExpiredReports()
    {
        var result = await _logAnalysisService.DeleteExpiredReportsAsync();
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to delete expired reports", ErrorCode = result.ErrorCode });
        }

        return NoContent();
    }

    #endregion
}

// Additional request DTOs for controller actions
public record ResolveSecurityEventRequest(string ResolvedBy, string ResolutionNotes);
public record MarkFalsePositiveRequest(string ResolvedBy, string ResolutionNotes);
public record UserActivityAnalyticsRequest(DateTime StartDate, DateTime EndDate, Guid? UserId = null, string? ActivityType = null);
public record SystemChangeAnalyticsRequest(DateTime StartDate, DateTime EndDate, string? EntityType = null, string? ChangeType = null);
public record SecurityEventAnalyticsRequest(DateTime StartDate, DateTime EndDate, string? Severity = null, string? RiskLevel = null);
