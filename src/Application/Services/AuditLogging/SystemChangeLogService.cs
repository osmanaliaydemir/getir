using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.AuditLogging;

public class SystemChangeLogService : ISystemChangeLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggingService _loggingService;
    private readonly ILogger<SystemChangeLogService> _logger;

    public SystemChangeLogService(
        IUnitOfWork unitOfWork,
        ILoggingService loggingService,
        ILogger<SystemChangeLogService> logger)
    {
        _unitOfWork = unitOfWork;
        _loggingService = loggingService;
        _logger = logger;
    }

    public async Task<Result<SystemChangeLogResponse>> CreateSystemChangeLogAsync(
        CreateSystemChangeLogRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var systemChangeLog = new SystemChangeLog
            {
                Id = Guid.NewGuid(),
                ChangeType = request.ChangeType,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                EntityName = request.EntityName,
                OldValues = request.OldValues,
                NewValues = request.NewValues,
                ChangedFields = request.ChangedFields,
                ChangeReason = request.ChangeReason,
                ChangeSource = request.ChangeSource,
                ChangedByUserId = request.ChangedByUserId,
                ChangedByUserName = request.ChangedByUserName,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                SessionId = request.SessionId,
                RequestId = request.RequestId,
                CorrelationId = request.CorrelationId,
                Timestamp = DateTime.UtcNow,
                IsSuccess = request.IsSuccess,
                ErrorMessage = request.ErrorMessage,
                Severity = request.Severity
            };

            await _unitOfWork.Repository<SystemChangeLog>().AddAsync(systemChangeLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(systemChangeLog);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating system change log for entity {EntityType}:{EntityId}", request.EntityType, request.EntityId);
            _loggingService.LogError("Create system change log failed", ex, new { request.EntityType, request.EntityId });
            return Result.Fail<SystemChangeLogResponse>("Failed to create system change log", "CREATE_SYSTEM_CHANGE_LOG_FAILED");
        }
    }

    public async Task<Result<SystemChangeLogResponse>> GetSystemChangeLogByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var systemChangeLogs = await _unitOfWork.ReadRepository<SystemChangeLog>()
                .ListAsync(x => x.Id == id, cancellationToken: cancellationToken);

            var systemChangeLog = systemChangeLogs.FirstOrDefault();
            if (systemChangeLog == null)
            {
                return Result.Fail<SystemChangeLogResponse>("System change log not found", "SYSTEM_CHANGE_LOG_NOT_FOUND");
            }

            var response = MapToResponse(systemChangeLog);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system change log {Id}", id);
            _loggingService.LogError("Get system change log failed", ex, new { Id = id });
            return Result.Fail<SystemChangeLogResponse>("Failed to get system change log", "GET_SYSTEM_CHANGE_LOG_FAILED");
        }
    }

    public async Task<Result<IEnumerable<SystemChangeLogResponse>>> GetSystemChangeLogsAsync(
        SystemChangeQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<SystemChangeLog>()
                .ListAsync(x => true, cancellationToken: cancellationToken);

            var filteredLogs = allLogs.AsQueryable();

            if (request.StartDate.HasValue)
                filteredLogs = filteredLogs.Where(x => x.Timestamp >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                filteredLogs = filteredLogs.Where(x => x.Timestamp <= request.EndDate.Value);

            if (!string.IsNullOrEmpty(request.ChangeType))
                filteredLogs = filteredLogs.Where(x => x.ChangeType == request.ChangeType);

            if (!string.IsNullOrEmpty(request.EntityType))
                filteredLogs = filteredLogs.Where(x => x.EntityType == request.EntityType);

            if (!string.IsNullOrEmpty(request.EntityId))
                filteredLogs = filteredLogs.Where(x => x.EntityId == request.EntityId);

            if (request.ChangedByUserId.HasValue)
                filteredLogs = filteredLogs.Where(x => x.ChangedByUserId == request.ChangedByUserId.Value);

            if (!string.IsNullOrEmpty(request.ChangedByUserName))
                filteredLogs = filteredLogs.Where(x => x.ChangedByUserName.Contains(request.ChangedByUserName));

            if (!string.IsNullOrEmpty(request.ChangeSource))
                filteredLogs = filteredLogs.Where(x => x.ChangeSource == request.ChangeSource);

            if (!string.IsNullOrEmpty(request.Severity))
                filteredLogs = filteredLogs.Where(x => x.Severity == request.Severity);

            if (request.IsSuccess.HasValue)
                filteredLogs = filteredLogs.Where(x => x.IsSuccess == request.IsSuccess.Value);

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
            _logger.LogError(ex, "Error getting system change logs");
            _loggingService.LogError("Get system change logs failed", ex);
            return Result.Fail<IEnumerable<SystemChangeLogResponse>>("Failed to get system change logs", "GET_SYSTEM_CHANGE_LOGS_FAILED");
        }
    }

    public async Task<Result<IEnumerable<SystemChangeLogResponse>>> GetSystemChangeLogsByEntityAsync(
        string entityType,
        string entityId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<SystemChangeLog>()
                .ListAsync(x => x.EntityType == entityType && x.EntityId == entityId, cancellationToken: cancellationToken);

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
            _logger.LogError(ex, "Error getting system change logs for entity {EntityType}:{EntityId}", entityType, entityId);
            _loggingService.LogError("Get system change logs by entity failed", ex, new { EntityType = entityType, EntityId = entityId });
            return Result.Fail<IEnumerable<SystemChangeLogResponse>>("Failed to get system change logs", "GET_SYSTEM_CHANGE_LOGS_BY_ENTITY_FAILED");
        }
    }

    public async Task<Result<IEnumerable<SystemChangeLogResponse>>> GetSystemChangeLogsByUserAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<SystemChangeLog>()
                .ListAsync(x => x.ChangedByUserId == userId, cancellationToken: cancellationToken);

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
            _logger.LogError(ex, "Error getting system change logs for user {UserId}", userId);
            _loggingService.LogError("Get system change logs by user failed", ex, new { UserId = userId });
            return Result.Fail<IEnumerable<SystemChangeLogResponse>>("Failed to get system change logs", "GET_SYSTEM_CHANGE_LOGS_BY_USER_FAILED");
        }
    }

    public async Task<Result<Dictionary<string, int>>> GetSystemChangeStatisticsAsync(
        DateTime startDate,
        DateTime endDate,
        string? entityType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<SystemChangeLog>()
                .ListAsync(x => x.Timestamp >= startDate && x.Timestamp <= endDate, cancellationToken: cancellationToken);

            var filteredLogs = allLogs.AsQueryable();

            if (!string.IsNullOrEmpty(entityType))
                filteredLogs = filteredLogs.Where(x => x.EntityType == entityType);

            var statistics = filteredLogs
                .GroupBy(x => x.ChangeType)
                .ToDictionary(g => g.Key, g => g.Count());

            return Result.Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system change statistics");
            _loggingService.LogError("Get system change statistics failed", ex);
            return Result.Fail<Dictionary<string, int>>("Failed to get system change statistics", "GET_SYSTEM_CHANGE_STATISTICS_FAILED");
        }
    }

    public async Task<Result<IEnumerable<SystemChangeLogResponse>>> GetRecentSystemChangesAsync(
        int count = 50,
        string? entityType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<SystemChangeLog>()
                .ListAsync(x => true, cancellationToken: cancellationToken);

            var filteredLogs = allLogs.AsQueryable();

            if (!string.IsNullOrEmpty(entityType))
                filteredLogs = filteredLogs.Where(x => x.EntityType == entityType);

            var recentLogs = filteredLogs
                .OrderByDescending(x => x.Timestamp)
                .Take(count);

            var responses = recentLogs.Select(MapToResponse);
            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent system changes");
            _loggingService.LogError("Get recent system changes failed", ex);
            return Result.Fail<IEnumerable<SystemChangeLogResponse>>("Failed to get recent system changes", "GET_RECENT_SYSTEM_CHANGES_FAILED");
        }
    }

    public async Task<Result> DeleteOldSystemChangeLogsAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var oldLogs = await _unitOfWork.ReadRepository<SystemChangeLog>()
                .ListAsync(x => x.Timestamp < cutoffDate, cancellationToken: cancellationToken);

            if (oldLogs.Any())
            {
                _unitOfWork.Repository<SystemChangeLog>().RemoveRange(oldLogs);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting old system change logs");
            _loggingService.LogError("Delete old system change logs failed", ex);
            return Result.Fail("Failed to delete old system change logs", "DELETE_OLD_SYSTEM_CHANGE_LOGS_FAILED");
        }
    }

    private static SystemChangeLogResponse MapToResponse(SystemChangeLog systemChangeLog)
    {
        return new SystemChangeLogResponse(
            systemChangeLog.Id,
            systemChangeLog.ChangeType,
            systemChangeLog.EntityType,
            systemChangeLog.EntityId,
            systemChangeLog.EntityName,
            systemChangeLog.OldValues,
            systemChangeLog.NewValues,
            systemChangeLog.ChangedFields,
            systemChangeLog.ChangeReason,
            systemChangeLog.ChangeSource,
            systemChangeLog.ChangedByUserId,
            systemChangeLog.ChangedByUserName,
            systemChangeLog.IpAddress,
            systemChangeLog.UserAgent,
            systemChangeLog.SessionId,
            systemChangeLog.RequestId,
            systemChangeLog.CorrelationId,
            systemChangeLog.Timestamp,
            systemChangeLog.IsSuccess,
            systemChangeLog.ErrorMessage,
            systemChangeLog.Severity);
    }
}