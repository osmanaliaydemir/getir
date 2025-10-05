using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.AuditLogging;

public class UserActivityLogService : IUserActivityLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggingService _loggingService;
    private readonly ILogger<UserActivityLogService> _logger;

    public UserActivityLogService(
        IUnitOfWork unitOfWork,
        ILoggingService loggingService,
        ILogger<UserActivityLogService> logger)
    {
        _unitOfWork = unitOfWork;
        _loggingService = loggingService;
        _logger = logger;
    }

    public async Task<Result<UserActivityLogResponse>> CreateUserActivityLogAsync(
        CreateUserActivityLogRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userActivityLog = new UserActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                UserName = request.UserName,
                ActivityType = request.ActivityType,
                ActivityDescription = request.ActivityDescription,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                ActivityData = request.ActivityData,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                SessionId = request.SessionId,
                RequestId = request.RequestId,
                DeviceType = request.DeviceType,
                Browser = request.Browser,
                OperatingSystem = request.OperatingSystem,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Location = request.Location,
                Timestamp = DateTime.UtcNow,
                Duration = request.Duration,
                IsSuccess = request.IsSuccess,
                ErrorMessage = request.ErrorMessage
            };

            await _unitOfWork.Repository<UserActivityLog>().AddAsync(userActivityLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(userActivityLog);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user activity log for user {UserId}", request.UserId);
            _loggingService.LogError("Create user activity log failed", ex, new { request.UserId, request.ActivityType });
            return Result.Fail<UserActivityLogResponse>("Failed to create user activity log", "CREATE_USER_ACTIVITY_LOG_FAILED");
        }
    }

    public async Task<Result<UserActivityLogResponse>> GetUserActivityLogByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userActivityLogs = await _unitOfWork.ReadRepository<UserActivityLog>()
                .ListAsync(x => x.Id == id, cancellationToken: cancellationToken);

            var userActivityLog = userActivityLogs.FirstOrDefault();
            if (userActivityLog == null)
            {
                return Result.Fail<UserActivityLogResponse>("User activity log not found", "USER_ACTIVITY_LOG_NOT_FOUND");
            }

            var response = MapToResponse(userActivityLog);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity log {Id}", id);
            _loggingService.LogError("Get user activity log failed", ex, new { Id = id });
            return Result.Fail<UserActivityLogResponse>("Failed to get user activity log", "GET_USER_ACTIVITY_LOG_FAILED");
        }
    }

    public async Task<Result<IEnumerable<UserActivityLogResponse>>> GetUserActivityLogsAsync(
        UserActivityQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Simplified approach - get all logs and filter in memory
            var allLogs = await _unitOfWork.ReadRepository<UserActivityLog>()
                .ListAsync(x => true, cancellationToken: cancellationToken);

            var filteredLogs = allLogs.AsQueryable();

            if (request.StartDate.HasValue)
                filteredLogs = filteredLogs.Where(x => x.Timestamp >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                filteredLogs = filteredLogs.Where(x => x.Timestamp <= request.EndDate.Value);

            if (request.UserId.HasValue)
                filteredLogs = filteredLogs.Where(x => x.UserId == request.UserId.Value);

            if (!string.IsNullOrEmpty(request.UserName))
                filteredLogs = filteredLogs.Where(x => x.UserName.Contains(request.UserName));

            if (!string.IsNullOrEmpty(request.ActivityType))
                filteredLogs = filteredLogs.Where(x => x.ActivityType == request.ActivityType);

            if (!string.IsNullOrEmpty(request.EntityType))
                filteredLogs = filteredLogs.Where(x => x.EntityType == request.EntityType);

            if (!string.IsNullOrEmpty(request.EntityId))
                filteredLogs = filteredLogs.Where(x => x.EntityId == request.EntityId);

            if (!string.IsNullOrEmpty(request.DeviceType))
                filteredLogs = filteredLogs.Where(x => x.DeviceType == request.DeviceType);

            if (!string.IsNullOrEmpty(request.Browser))
                filteredLogs = filteredLogs.Where(x => x.Browser == request.Browser);

            if (!string.IsNullOrEmpty(request.OperatingSystem))
                filteredLogs = filteredLogs.Where(x => x.OperatingSystem == request.OperatingSystem);

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
            _logger.LogError(ex, "Error getting user activity logs");
            _loggingService.LogError("Get user activity logs failed", ex);
            return Result.Fail<IEnumerable<UserActivityLogResponse>>("Failed to get user activity logs", "GET_USER_ACTIVITY_LOGS_FAILED");
        }
    }

    public async Task<Result<IEnumerable<UserActivityLogResponse>>> GetUserActivityLogsByUserIdAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<UserActivityLog>()
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
            _logger.LogError(ex, "Error getting user activity logs for user {UserId}", userId);
            _loggingService.LogError("Get user activity logs by user failed", ex, new { UserId = userId });
            return Result.Fail<IEnumerable<UserActivityLogResponse>>("Failed to get user activity logs", "GET_USER_ACTIVITY_LOGS_BY_USER_FAILED");
        }
    }

    public async Task<Result<IEnumerable<UserActivityLogResponse>>> GetUserActivityLogsByActivityTypeAsync(
        string activityType,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<UserActivityLog>()
                .ListAsync(x => x.ActivityType == activityType, cancellationToken: cancellationToken);

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
            _logger.LogError(ex, "Error getting user activity logs for activity type {ActivityType}", activityType);
            _loggingService.LogError("Get user activity logs by activity type failed", ex, new { ActivityType = activityType });
            return Result.Fail<IEnumerable<UserActivityLogResponse>>("Failed to get user activity logs", "GET_USER_ACTIVITY_LOGS_BY_ACTIVITY_TYPE_FAILED");
        }
    }

    public async Task<Result<Dictionary<string, int>>> GetUserActivityStatisticsAsync(
        DateTime startDate,
        DateTime endDate,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<UserActivityLog>()
                .ListAsync(x => x.Timestamp >= startDate && x.Timestamp <= endDate, cancellationToken: cancellationToken);

            var filteredLogs = allLogs.AsQueryable();

            if (userId.HasValue)
                filteredLogs = filteredLogs.Where(x => x.UserId == userId.Value);

            var statistics = filteredLogs
                .GroupBy(x => x.ActivityType)
                .ToDictionary(g => g.Key, g => g.Count());

            return Result.Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity statistics");
            _loggingService.LogError("Get user activity statistics failed", ex);
            return Result.Fail<Dictionary<string, int>>("Failed to get user activity statistics", "GET_USER_ACTIVITY_STATISTICS_FAILED");
        }
    }

    public async Task<Result<IEnumerable<UserActivityLogResponse>>> GetSuspiciousActivitiesAsync(
        DateTime startDate,
        DateTime endDate,
        int threshold = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allLogs = await _unitOfWork.ReadRepository<UserActivityLog>()
                .ListAsync(x => x.Timestamp >= startDate && x.Timestamp <= endDate, cancellationToken: cancellationToken);

            var suspiciousUsers = allLogs
                .GroupBy(x => x.UserId)
                .Where(g => g.Count() > threshold)
                .Select(g => g.Key)
                .ToList();

            var suspiciousActivities = allLogs
                .Where(x => suspiciousUsers.Contains(x.UserId))
                .OrderByDescending(x => x.Timestamp);

            var responses = suspiciousActivities.Select(MapToResponse);
            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suspicious activities");
            _loggingService.LogError("Get suspicious activities failed", ex);
            return Result.Fail<IEnumerable<UserActivityLogResponse>>("Failed to get suspicious activities", "GET_SUSPICIOUS_ACTIVITIES_FAILED");
        }
    }

    public async Task<Result> DeleteOldUserActivityLogsAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var oldLogs = await _unitOfWork.ReadRepository<UserActivityLog>()
                .ListAsync(x => x.Timestamp < cutoffDate, cancellationToken: cancellationToken);

            if (oldLogs.Any())
            {
                _unitOfWork.Repository<UserActivityLog>().RemoveRange(oldLogs);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting old user activity logs");
            _loggingService.LogError("Delete old user activity logs failed", ex);
            return Result.Fail("Failed to delete old user activity logs", "DELETE_OLD_USER_ACTIVITY_LOGS_FAILED");
        }
    }

    private static UserActivityLogResponse MapToResponse(UserActivityLog userActivityLog)
    {
        return new UserActivityLogResponse(
            userActivityLog.Id,
            userActivityLog.UserId,
            userActivityLog.UserName,
            userActivityLog.ActivityType,
            userActivityLog.ActivityDescription,
            userActivityLog.EntityType,
            userActivityLog.EntityId,
            userActivityLog.ActivityData,
            userActivityLog.IpAddress,
            userActivityLog.UserAgent,
            userActivityLog.SessionId,
            userActivityLog.RequestId,
            userActivityLog.DeviceType,
            userActivityLog.Browser,
            userActivityLog.OperatingSystem,
            userActivityLog.Latitude,
            userActivityLog.Longitude,
            userActivityLog.Location,
            userActivityLog.Timestamp,
            userActivityLog.Duration,
            userActivityLog.IsSuccess,
            userActivityLog.ErrorMessage);
    }
}