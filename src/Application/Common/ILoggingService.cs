using Microsoft.Extensions.Logging;

namespace Getir.Application.Common;

/// <summary>
/// Business logic için özelleştirilmiş logging servisi
/// </summary>
public interface ILoggingService
{
    void LogBusinessEvent(string eventName, object? data = null, LogLevel level = LogLevel.Information);
    void LogUserAction(string userId, string action, object? data = null);
    void LogPerformance(string operation, TimeSpan duration, object? metadata = null);
    void LogSecurityEvent(string eventName, string? userId = null, object? data = null);
    void LogError(string message, Exception? exception = null, object? data = null);
}

/// <summary>
/// Business logic için özelleştirilmiş logging servisi implementasyonu
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;

    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger;
    }

    public void LogBusinessEvent(string eventName, object? data = null, LogLevel level = LogLevel.Information)
    {
        var logData = new
        {
            EventType = "BusinessEvent",
            EventName = eventName,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        _logger.Log(level, "Business Event: {EventName} - Data: {@Data}", eventName, logData);
    }

    public void LogUserAction(string userId, string action, object? data = null)
    {
        var logData = new
        {
            EventType = "UserAction",
            UserId = userId,
            Action = action,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("User Action: {UserId} performed {Action} - Data: {@Data}", userId, action, logData);
    }

    public void LogPerformance(string operation, TimeSpan duration, object? metadata = null)
    {
        var logData = new
        {
            EventType = "Performance",
            Operation = operation,
            Duration = duration.TotalMilliseconds,
            Metadata = metadata,
            Timestamp = DateTime.UtcNow
        };

        var level = duration.TotalMilliseconds > 1000 ? LogLevel.Warning : LogLevel.Information;
        _logger.Log(level, "Performance: {Operation} took {Duration}ms - Metadata: {@Metadata}", 
            operation, duration.TotalMilliseconds, metadata);
    }

    public void LogSecurityEvent(string eventName, string? userId = null, object? data = null)
    {
        var logData = new
        {
            EventType = "SecurityEvent",
            EventName = eventName,
            UserId = userId,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogWarning("Security Event: {EventName} - User: {UserId} - Data: {@Data}", 
            eventName, userId, logData);
    }

    public void LogError(string message, Exception? exception = null, object? data = null)
    {
        var logData = new
        {
            EventType = "Error",
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        if (exception != null)
        {
            _logger.LogError(exception, "Error: {Message} - Data: {@Data}", message, logData);
        }
        else
        {
            _logger.LogError("Error: {Message} - Data: {@Data}", message, logData);
        }
    }
}
