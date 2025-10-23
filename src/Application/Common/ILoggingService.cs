using Microsoft.Extensions.Logging;

namespace Getir.Application.Common;

/// <summary>
/// Business logic için özelleştirilmiş logging servisi
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// İş mantığı olayını logla
    /// </summary>
    /// <param name="eventName">Olay adı</param>
    /// <param name="data">Olay verisi</param>
    /// <param name="level">Log seviyesi</param>
    void LogBusinessEvent(string eventName, object? data = null, LogLevel level = LogLevel.Information);
    
    /// <summary>
    /// Kullanıcı eylemini logla
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="action">Eylem</param>
    /// <param name="data">Eylem verisi</param>
    void LogUserAction(string userId, string action, object? data = null);
    
    /// <summary>
    /// Performans bilgisini logla
    /// </summary>
    /// <param name="operation">İşlem adı</param>
    /// <param name="duration">Süre</param>
    /// <param name="metadata">Metadata</param>
    void LogPerformance(string operation, TimeSpan duration, object? metadata = null);
    
    /// <summary>
    /// Güvenlik olayını logla
    /// </summary>
    /// <param name="eventName">Olay adı</param>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="data">Olay verisi</param>
    void LogSecurityEvent(string eventName, string? userId = null, object? data = null);
    
    /// <summary>
    /// Hata bilgisini logla
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="exception">Exception</param>
    /// <param name="data">Hata verisi</param>
    void LogError(string message, Exception? exception = null, object? data = null);
}

/// <summary>
/// Business logic için özelleştirilmiş logging servisi implementasyonu
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;

    /// <summary>
    /// LoggingService constructor
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// İş mantığı olayını logla
    /// </summary>
    /// <param name="eventName">Olay adı</param>
    /// <param name="data">Olay verisi</param>
    /// <param name="level">Log seviyesi</param>
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

    /// <summary>
    /// Kullanıcı eylemini logla
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="action">Eylem</param>
    /// <param name="data">Eylem verisi</param>
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

    /// <summary>
    /// Performans bilgisini logla
    /// </summary>
    /// <param name="operation">İşlem adı</param>
    /// <param name="duration">Süre</param>
    /// <param name="metadata">Metadata</param>
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

    /// <summary>
    /// Güvenlik olayını logla
    /// </summary>
    /// <param name="eventName">Olay adı</param>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="data">Olay verisi</param>
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

    /// <summary>
    /// Hata bilgisini logla
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="exception">Exception</param>
    /// <param name="data">Hata verisi</param>
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
