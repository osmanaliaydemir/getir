using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.AuditLogging;

public class SecurityEventLogService : ISecurityEventLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggingService _loggingService;
    private readonly ILogger<SecurityEventLogService> _logger;

    public SecurityEventLogService(
        IUnitOfWork unitOfWork,
        ILoggingService loggingService,
        ILogger<SecurityEventLogService> logger)
    {
        _unitOfWork = unitOfWork;
        _loggingService = loggingService;
        _logger = logger;
    }

    public async Task<Result<SecurityEventLogResponse>> CreateSecurityEventLogAsync(
        CreateSecurityEventLogRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var securityEventLog = new SecurityEventLog
            {
                Id = Guid.NewGuid(),
                EventType = request.EventType,
                EventTitle = request.EventTitle,
                EventDescription = request.EventDescription,
                Severity = request.Severity,
                RiskLevel = request.RiskLevel,
                UserId = request.UserId,
                UserName = request.UserName,
                UserRole = request.UserRole,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                DeviceFingerprint = request.DeviceFingerprint,
                SessionId = request.SessionId,
                RequestId = request.RequestId,
                CorrelationId = request.CorrelationId,
                EventData = request.EventData,
                ThreatIndicators = request.ThreatIndicators,
                MitigationActions = request.MitigationActions,
                Source = request.Source,
                Category = request.Category,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Location = request.Location,
                Timestamp = DateTime.UtcNow,
                RequiresInvestigation = request.RequiresInvestigation,
                IsFalsePositive = request.IsFalsePositive
            };

            await _unitOfWork.Repository<SecurityEventLog>().AddAsync(securityEventLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(securityEventLog);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating security event log for event type {EventType}", request.EventType);
            _loggingService.LogError("Create security event log failed", ex, new { request.EventType });
            return Result.Fail<SecurityEventLogResponse>("Failed to create security event log", "CREATE_SECURITY_EVENT_LOG_FAILED");
        }
    }

    public async Task<Result<SecurityEventLogResponse>> GetSecurityEventLogByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var securityEventLogs = await _unitOfWork.ReadRepository<SecurityEventLog>()
                .ListAsync(x => x.Id == id, cancellationToken: cancellationToken);

            var securityEventLog = securityEventLogs.FirstOrDefault();
            if (securityEventLog == null)
            {
                return Result.Fail<SecurityEventLogResponse>("Security event log not found", "SECURITY_EVENT_LOG_NOT_FOUND");
            }

            var response = MapToResponse(securityEventLog);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security event log {Id}", id);
            _loggingService.LogError("Get security event log failed", ex, new { Id = id });
            return Result.Fail<SecurityEventLogResponse>("Failed to get security event log", "GET_SECURITY_EVENT_LOG_FAILED");
        }
    }

    public async Task<Result<IEnumerable<SecurityEventLogResponse>>> GetSecurityEventLogsAsync(
        SecurityEventQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<SecurityEventLog>()
                .ListAsync(x => true, cancellationToken: cancellationToken);

            var filteredLogs = allLogs.AsQueryable();

            if (request.StartDate.HasValue)
                filteredLogs = filteredLogs.Where(x => x.Timestamp >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                filteredLogs = filteredLogs.Where(x => x.Timestamp <= request.EndDate.Value);

            if (!string.IsNullOrEmpty(request.EventType))
                filteredLogs = filteredLogs.Where(x => x.EventType == request.EventType);

            if (!string.IsNullOrEmpty(request.Severity))
                filteredLogs = filteredLogs.Where(x => x.Severity == request.Severity);

            if (!string.IsNullOrEmpty(request.RiskLevel))
                filteredLogs = filteredLogs.Where(x => x.RiskLevel == request.RiskLevel);

            if (request.UserId.HasValue)
                filteredLogs = filteredLogs.Where(x => x.UserId == request.UserId.Value);

            if (!string.IsNullOrEmpty(request.UserName))
                filteredLogs = filteredLogs.Where(x => x.UserName.Contains(request.UserName));

            if (!string.IsNullOrEmpty(request.UserRole))
                filteredLogs = filteredLogs.Where(x => x.UserRole == request.UserRole);

            if (!string.IsNullOrEmpty(request.IpAddress))
                filteredLogs = filteredLogs.Where(x => x.IpAddress == request.IpAddress);

            if (!string.IsNullOrEmpty(request.Source))
                filteredLogs = filteredLogs.Where(x => x.Source == request.Source);

            if (!string.IsNullOrEmpty(request.Category))
                filteredLogs = filteredLogs.Where(x => x.Category == request.Category);

            if (request.IsResolved.HasValue)
                filteredLogs = filteredLogs.Where(x => x.IsResolved == request.IsResolved.Value);

            if (request.RequiresInvestigation.HasValue)
                filteredLogs = filteredLogs.Where(x => x.RequiresInvestigation == request.RequiresInvestigation.Value);

            if (request.IsFalsePositive.HasValue)
                filteredLogs = filteredLogs.Where(x => x.IsFalsePositive == request.IsFalsePositive.Value);

            // Apply sorting
            var sortedLogs = request.SortDirection?.ToUpper() == "ASC"
                ? filteredLogs.OrderBy(x => x.Timestamp)
                : filteredLogs.OrderByDescending(x => x.Timestamp);

            // Apply pagination
            var pagedLogs = sortedLogs
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

            var responses = pagedLogs.Select(MapToResponse);
            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security event logs");
            _loggingService.LogError("Get security event logs failed", ex);
            return Result.Fail<IEnumerable<SecurityEventLogResponse>>("Failed to get security event logs", "GET_SECURITY_EVENT_LOGS_FAILED");
        }
    }

    public async Task<Result<IEnumerable<SecurityEventLogResponse>>> GetUnresolvedSecurityEventsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? severity = null,
        string? riskLevel = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<SecurityEventLog>()
                .ListAsync(x => !x.IsResolved, cancellationToken: cancellationToken);

            var filteredLogs = allLogs.AsQueryable();

            if (startDate.HasValue)
                filteredLogs = filteredLogs.Where(x => x.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                filteredLogs = filteredLogs.Where(x => x.Timestamp <= endDate.Value);

            if (!string.IsNullOrEmpty(severity))
                filteredLogs = filteredLogs.Where(x => x.Severity == severity);

            if (!string.IsNullOrEmpty(riskLevel))
                filteredLogs = filteredLogs.Where(x => x.RiskLevel == riskLevel);

            var pagedLogs = filteredLogs
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var responses = pagedLogs.Select(MapToResponse);
            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unresolved security events");
            _loggingService.LogError("Get unresolved security events failed", ex);
            return Result.Fail<IEnumerable<SecurityEventLogResponse>>("Failed to get unresolved security events", "GET_UNRESOLVED_SECURITY_EVENTS_FAILED");
        }
    }

    public async Task<Result<IEnumerable<SecurityEventLogResponse>>> GetSecurityEventsRequiringInvestigationAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<SecurityEventLog>()
                .ListAsync(x => x.RequiresInvestigation, cancellationToken: cancellationToken);

            var filteredLogs = allLogs.AsQueryable();

            if (startDate.HasValue)
                filteredLogs = filteredLogs.Where(x => x.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                filteredLogs = filteredLogs.Where(x => x.Timestamp <= endDate.Value);

            var pagedLogs = filteredLogs
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var responses = pagedLogs.Select(MapToResponse);
            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security events requiring investigation");
            _loggingService.LogError("Get security events requiring investigation failed", ex);
            return Result.Fail<IEnumerable<SecurityEventLogResponse>>("Failed to get security events requiring investigation", "GET_SECURITY_EVENTS_REQUIRING_INVESTIGATION_FAILED");
        }
    }

    public async Task<Result<SecurityEventLogResponse>> ResolveSecurityEventAsync(
        Guid id,
        string resolvedBy,
        string resolutionNotes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var securityEventLogs = await _unitOfWork.ReadRepository<SecurityEventLog>()
                .ListAsync(x => x.Id == id, cancellationToken: cancellationToken);

            var securityEventLog = securityEventLogs.FirstOrDefault();
            if (securityEventLog == null)
            {
                return Result.Fail<SecurityEventLogResponse>("Security event log not found", "SECURITY_EVENT_LOG_NOT_FOUND");
            }

            securityEventLog.IsResolved = true;
            securityEventLog.ResolvedAt = DateTime.UtcNow;
            securityEventLog.ResolvedBy = resolvedBy;
            securityEventLog.ResolutionNotes = resolutionNotes;

            _unitOfWork.Repository<SecurityEventLog>().Update(securityEventLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(securityEventLog);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving security event {Id}", id);
            _loggingService.LogError("Resolve security event failed", ex, new { Id = id });
            return Result.Fail<SecurityEventLogResponse>("Failed to resolve security event", "RESOLVE_SECURITY_EVENT_FAILED");
        }
    }

    public async Task<Result<SecurityEventLogResponse>> MarkAsFalsePositiveAsync(
        Guid id,
        string resolvedBy,
        string resolutionNotes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var securityEventLogs = await _unitOfWork.ReadRepository<SecurityEventLog>()
                .ListAsync(x => x.Id == id, cancellationToken: cancellationToken);

            var securityEventLog = securityEventLogs.FirstOrDefault();
            if (securityEventLog == null)
            {
                return Result.Fail<SecurityEventLogResponse>("Security event log not found", "SECURITY_EVENT_LOG_NOT_FOUND");
            }

            securityEventLog.IsResolved = true;
            securityEventLog.IsFalsePositive = true;
            securityEventLog.ResolvedAt = DateTime.UtcNow;
            securityEventLog.ResolvedBy = resolvedBy;
            securityEventLog.ResolutionNotes = resolutionNotes;

            _unitOfWork.Repository<SecurityEventLog>().Update(securityEventLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(securityEventLog);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking security event as false positive {Id}", id);
            _loggingService.LogError("Mark security event as false positive failed", ex, new { Id = id });
            return Result.Fail<SecurityEventLogResponse>("Failed to mark security event as false positive", "MARK_SECURITY_EVENT_FALSE_POSITIVE_FAILED");
        }
    }

    public async Task<Result<Dictionary<string, int>>> GetSecurityEventStatisticsAsync(
        DateTime startDate,
        DateTime endDate,
        string? severity = null,
        string? riskLevel = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<SecurityEventLog>()
                .ListAsync(x => x.Timestamp >= startDate && x.Timestamp <= endDate, cancellationToken: cancellationToken);

            var filteredLogs = allLogs.AsQueryable();

            if (!string.IsNullOrEmpty(severity))
                filteredLogs = filteredLogs.Where(x => x.Severity == severity);

            if (!string.IsNullOrEmpty(riskLevel))
                filteredLogs = filteredLogs.Where(x => x.RiskLevel == riskLevel);

            var statistics = filteredLogs
                .GroupBy(x => x.EventType)
                .ToDictionary(g => g.Key, g => g.Count());

            return Result.Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security event statistics");
            _loggingService.LogError("Get security event statistics failed", ex);
            return Result.Fail<Dictionary<string, int>>("Failed to get security event statistics", "GET_SECURITY_EVENT_STATISTICS_FAILED");
        }
    }

    public async Task<Result<IEnumerable<SecurityEventLogResponse>>> GetHighRiskSecurityEventsAsync(
        DateTime startDate,
        DateTime endDate,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<SecurityEventLog>()
                .ListAsync(x => x.Timestamp >= startDate && x.Timestamp <= endDate, cancellationToken: cancellationToken);

            var highRiskLogs = allLogs
                .Where(x => x.RiskLevel == "HIGH" || x.RiskLevel == "CRITICAL")
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var responses = highRiskLogs.Select(MapToResponse);
            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting high risk security events");
            _loggingService.LogError("Get high risk security events failed", ex);
            return Result.Fail<IEnumerable<SecurityEventLogResponse>>("Failed to get high risk security events", "GET_HIGH_RISK_SECURITY_EVENTS_FAILED");
        }
    }

    public async Task<Result<IEnumerable<SecurityEventLogResponse>>> GetSecurityEventsByUserAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<SecurityEventLog>()
                .ListAsync(x => x.UserId == userId, cancellationToken: cancellationToken);

            var filteredLogs = allLogs.AsQueryable();

            if (startDate.HasValue)
                filteredLogs = filteredLogs.Where(x => x.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                filteredLogs = filteredLogs.Where(x => x.Timestamp <= endDate.Value);

            var pagedLogs = filteredLogs
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var responses = pagedLogs.Select(MapToResponse);
            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security events for user {UserId}", userId);
            _loggingService.LogError("Get security events by user failed", ex, new { UserId = userId });
            return Result.Fail<IEnumerable<SecurityEventLogResponse>>("Failed to get security events", "GET_SECURITY_EVENTS_BY_USER_FAILED");
        }
    }

    public async Task<Result<IEnumerable<SecurityEventLogResponse>>> GetSecurityEventsByIpAddressAsync(
        string ipAddress,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<SecurityEventLog>()
                .ListAsync(x => x.IpAddress == ipAddress, cancellationToken: cancellationToken);

            var filteredLogs = allLogs.AsQueryable();

            if (startDate.HasValue)
                filteredLogs = filteredLogs.Where(x => x.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                filteredLogs = filteredLogs.Where(x => x.Timestamp <= endDate.Value);

            var pagedLogs = filteredLogs
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var responses = pagedLogs.Select(MapToResponse);
            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security events for IP address {IpAddress}", ipAddress);
            _loggingService.LogError("Get security events by IP address failed", ex, new { IpAddress = ipAddress });
            return Result.Fail<IEnumerable<SecurityEventLogResponse>>("Failed to get security events", "GET_SECURITY_EVENTS_BY_IP_FAILED");
        }
    }

    public async Task<Result> DeleteOldSecurityEventLogsAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var oldLogs = await _unitOfWork.ReadRepository<SecurityEventLog>()
                .ListAsync(x => x.Timestamp < cutoffDate, cancellationToken: cancellationToken);

            if (oldLogs.Any())
            {
                _unitOfWork.Repository<SecurityEventLog>().RemoveRange(oldLogs);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting old security event logs");
            _loggingService.LogError("Delete old security event logs failed", ex);
            return Result.Fail("Failed to delete old security event logs", "DELETE_OLD_SECURITY_EVENT_LOGS_FAILED");
        }
    }

    private static SecurityEventLogResponse MapToResponse(SecurityEventLog securityEventLog)
    {
        return new SecurityEventLogResponse(
            securityEventLog.Id,
            securityEventLog.EventType,
            securityEventLog.EventTitle,
            securityEventLog.EventDescription,
            securityEventLog.Severity,
            securityEventLog.RiskLevel,
            securityEventLog.UserId,
            securityEventLog.UserName,
            securityEventLog.UserRole,
            securityEventLog.IpAddress,
            securityEventLog.UserAgent,
            securityEventLog.DeviceFingerprint,
            securityEventLog.SessionId,
            securityEventLog.RequestId,
            securityEventLog.CorrelationId,
            securityEventLog.EventData,
            securityEventLog.ThreatIndicators,
            securityEventLog.MitigationActions,
            securityEventLog.Source,
            securityEventLog.Category,
            securityEventLog.Latitude,
            securityEventLog.Longitude,
            securityEventLog.Location,
            securityEventLog.Timestamp,
            securityEventLog.IsResolved,
            securityEventLog.ResolvedAt,
            securityEventLog.ResolvedBy,
            securityEventLog.ResolutionNotes,
            securityEventLog.RequiresInvestigation,
            securityEventLog.IsFalsePositive);
    }
}