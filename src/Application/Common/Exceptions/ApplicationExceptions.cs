namespace Getir.Application.Common.Exceptions;

/// <summary>
/// Tüm uygulama özel exception'ları için base sınıf
/// </summary>
public abstract class ApplicationException : Exception
{
    /// <summary>
    /// Hata kodu
    /// </summary>
    public string ErrorCode { get; }
    
    /// <summary>
    /// Hata detayları
    /// </summary>
    public object? Details { get; }

    /// <summary>
    /// ApplicationException constructor
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="errorCode">Hata kodu</param>
    /// <param name="details">Hata detayları</param>
    protected ApplicationException(string message, string errorCode, object? details = null) 
        : base(message)
    {
        ErrorCode = errorCode;
        Details = details;
    }

    /// <summary>
    /// ApplicationException constructor (inner exception ile)
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="errorCode">Hata kodu</param>
    /// <param name="innerException">İç exception</param>
    /// <param name="details">Hata detayları</param>
    protected ApplicationException(string message, string errorCode, Exception innerException, object? details = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Details = details;
    }
}

/// <summary>
/// İstenen entity bulunamadığında fırlatılan exception
/// </summary>
public class EntityNotFoundException : ApplicationException
{
    /// <summary>
    /// Entity tipi
    /// </summary>
    public string EntityType { get; }
    
    /// <summary>
    /// Entity ID
    /// </summary>
    public object EntityId { get; }

    /// <summary>
    /// EntityNotFoundException constructor
    /// </summary>
    /// <param name="entityType">Entity tipi</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="customMessage">Özel hata mesajı</param>
    public EntityNotFoundException(string entityType, object entityId, string? customMessage = null)
        : base(
            customMessage ?? $"{entityType} with ID '{entityId}' was not found",
            "ENTITY_NOT_FOUND",
            new { EntityType = entityType, EntityId = entityId })
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// İş kuralı ihlal edildiğinde fırlatılan exception
/// </summary>
public class BusinessRuleViolationException : ApplicationException
{
    /// <summary>
    /// Kural adı
    /// </summary>
    public string RuleName { get; }

    /// <summary>
    /// BusinessRuleViolationException constructor
    /// </summary>
    /// <param name="ruleName">Kural adı</param>
    /// <param name="message">Hata mesajı</param>
    /// <param name="details">Hata detayları</param>
    public BusinessRuleViolationException(string ruleName, string message, object? details = null)
        : base(message, "BUSINESS_RULE_VIOLATION", details)
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Validasyon başarısız olduğunda fırlatılan exception
/// </summary>
public class ValidationException : ApplicationException
{
    /// <summary>
    /// Validasyon hataları
    /// </summary>
    public Dictionary<string, string[]> ValidationErrors { get; }

    /// <summary>
    /// ValidationException constructor (çoklu hata)
    /// </summary>
    /// <param name="validationErrors">Validasyon hataları</param>
    public ValidationException(Dictionary<string, string[]> validationErrors)
        : base("Validation failed", "VALIDATION_ERROR", validationErrors)
    {
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// ValidationException constructor (tek hata)
    /// </summary>
    /// <param name="field">Alan adı</param>
    /// <param name="error">Hata mesajı</param>
    public ValidationException(string field, string error)
        : base("Validation failed", "VALIDATION_ERROR", new Dictionary<string, string[]> { { field, new[] { error } } })
    {
        ValidationErrors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }
}

/// <summary>
/// İşlem yetkisiz olduğunda fırlatılan exception
/// </summary>
public class UnauthorizedException : ApplicationException
{
    /// <summary>
    /// Gerekli yetki
    /// </summary>
    public string RequiredPermission { get; }
    
    /// <summary>
    /// Kullanıcı ID
    /// </summary>
    public string UserId { get; }

    /// <summary>
    /// UnauthorizedException constructor
    /// </summary>
    /// <param name="requiredPermission">Gerekli yetki</param>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="customMessage">Özel hata mesajı</param>
    public UnauthorizedException(string requiredPermission, string userId, string? customMessage = null)
        : base(
            customMessage ?? $"User '{userId}' is not authorized to perform this action. Required permission: {requiredPermission}",
            "UNAUTHORIZED",
            new { RequiredPermission = requiredPermission, UserId = userId })
    {
        RequiredPermission = requiredPermission;
        UserId = userId;
    }
}

/// <summary>
/// Kaynak mevcut olmadığında fırlatılan exception
/// </summary>
public class ResourceUnavailableException : ApplicationException
{
    /// <summary>
    /// Kaynak tipi
    /// </summary>
    public string ResourceType { get; }
    
    /// <summary>
    /// Kaynak ID
    /// </summary>
    public string ResourceId { get; }

    /// <summary>
    /// ResourceUnavailableException constructor
    /// </summary>
    /// <param name="resourceType">Kaynak tipi</param>
    /// <param name="resourceId">Kaynak ID</param>
    /// <param name="customMessage">Özel hata mesajı</param>
    public ResourceUnavailableException(string resourceType, string resourceId, string? customMessage = null)
        : base(
            customMessage ?? $"{resourceType} '{resourceId}' is not available",
            "RESOURCE_UNAVAILABLE",
            new { ResourceType = resourceType, ResourceId = resourceId })
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Duplicate kaynak oluşturulduğunda fırlatılan exception
/// </summary>
public class DuplicateResourceException : ApplicationException
{
    /// <summary>
    /// Kaynak tipi
    /// </summary>
    public string ResourceType { get; }
    
    /// <summary>
    /// Alan adı
    /// </summary>
    public string FieldName { get; }
    
    /// <summary>
    /// Alan değeri
    /// </summary>
    public object FieldValue { get; }

    /// <summary>
    /// DuplicateResourceException constructor
    /// </summary>
    /// <param name="resourceType">Kaynak tipi</param>
    /// <param name="fieldName">Alan adı</param>
    /// <param name="fieldValue">Alan değeri</param>
    /// <param name="customMessage">Özel hata mesajı</param>
    public DuplicateResourceException(string resourceType, string fieldName, object fieldValue, string? customMessage = null)
        : base(
            customMessage ?? $"{resourceType} with {fieldName} '{fieldValue}' already exists",
            "DUPLICATE_RESOURCE",
            new { ResourceType = resourceType, FieldName = fieldName, FieldValue = fieldValue })
    {
        ResourceType = resourceType;
        FieldName = fieldName;
        FieldValue = fieldValue;
    }
}

/// <summary>
/// Concurrency çakışması oluştuğunda fırlatılan exception
/// </summary>
public class ConcurrencyConflictException : ApplicationException
{
    /// <summary>
    /// Entity tipi
    /// </summary>
    public string EntityType { get; }
    
    /// <summary>
    /// Entity ID
    /// </summary>
    public object EntityId { get; }

    /// <summary>
    /// ConcurrencyConflictException constructor
    /// </summary>
    /// <param name="entityType">Entity tipi</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="customMessage">Özel hata mesajı</param>
    public ConcurrencyConflictException(string entityType, object entityId, string? customMessage = null)
        : base(
            customMessage ?? $"{entityType} with ID '{entityId}' was modified by another user",
            "CONCURRENCY_CONFLICT",
            new { EntityType = entityType, EntityId = entityId })
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Dış servis mevcut olmadığında fırlatılan exception
/// </summary>
public class ExternalServiceException : ApplicationException
{
    /// <summary>
    /// Servis adı
    /// </summary>
    public string ServiceName { get; }
    
    /// <summary>
    /// İşlem adı
    /// </summary>
    public string Operation { get; }

    /// <summary>
    /// ExternalServiceException constructor
    /// </summary>
    /// <param name="serviceName">Servis adı</param>
    /// <param name="operation">İşlem adı</param>
    /// <param name="customMessage">Özel hata mesajı</param>
    /// <param name="innerException">İç exception</param>
    public ExternalServiceException(string serviceName, string operation, string? customMessage = null, Exception? innerException = null)
        : base(
            customMessage ?? $"External service '{serviceName}' failed during '{operation}'",
            "EXTERNAL_SERVICE_ERROR",
            innerException!,
            new { ServiceName = serviceName, Operation = operation })
    {
        ServiceName = serviceName;
        Operation = operation;
    }
}

/// <summary>
/// Rate limit aşıldığında fırlatılan exception
/// </summary>
public class RateLimitExceededException : ApplicationException
{
    /// <summary>
    /// Kaynak adı
    /// </summary>
    public string Resource { get; }
    
    /// <summary>
    /// Limit değeri
    /// </summary>
    public int Limit { get; }
    
    /// <summary>
    /// Reset süresi
    /// </summary>
    public TimeSpan ResetTime { get; }

    /// <summary>
    /// RateLimitExceededException constructor
    /// </summary>
    /// <param name="resource">Kaynak adı</param>
    /// <param name="limit">Limit değeri</param>
    /// <param name="resetTime">Reset süresi</param>
    /// <param name="customMessage">Özel hata mesajı</param>
    public RateLimitExceededException(string resource, int limit, TimeSpan resetTime, string? customMessage = null)
        : base(
            customMessage ?? $"Rate limit exceeded for '{resource}'. Limit: {limit}, Reset in: {resetTime.TotalSeconds} seconds",
            "RATE_LIMIT_EXCEEDED",
            new { Resource = resource, Limit = limit, ResetTime = resetTime })
    {
        Resource = resource;
        Limit = limit;
        ResetTime = resetTime;
    }
}

/// <summary>
/// Timeout oluştuğunda fırlatılan exception
/// </summary>
public class TimeoutException : ApplicationException
{
    /// <summary>
    /// İşlem adı
    /// </summary>
    public string Operation { get; }
    
    /// <summary>
    /// Timeout süresi
    /// </summary>
    public TimeSpan Timeout { get; }

    /// <summary>
    /// TimeoutException constructor
    /// </summary>
    /// <param name="operation">İşlem adı</param>
    /// <param name="timeout">Timeout süresi</param>
    /// <param name="customMessage">Özel hata mesajı</param>
    public TimeoutException(string operation, TimeSpan timeout, string? customMessage = null)
        : base(
            customMessage ?? $"Operation '{operation}' timed out after {timeout.TotalSeconds} seconds",
            "TIMEOUT",
            new { Operation = operation, Timeout = timeout })
    {
        Operation = operation;
        Timeout = timeout;
    }
}

/// <summary>
/// Konfigürasyon hatası oluştuğunda fırlatılan exception
/// </summary>
public class ConfigurationException : ApplicationException
{
    /// <summary>
    /// Konfigürasyon anahtarı
    /// </summary>
    public string ConfigurationKey { get; }

    /// <summary>
    /// ConfigurationException constructor
    /// </summary>
    /// <param name="configurationKey">Konfigürasyon anahtarı</param>
    /// <param name="customMessage">Özel hata mesajı</param>
    public ConfigurationException(string configurationKey, string? customMessage = null)
        : base(
            customMessage ?? $"Configuration error for key '{configurationKey}'",
            "CONFIGURATION_ERROR",
            new { ConfigurationKey = configurationKey })
    {
        ConfigurationKey = configurationKey;
    }
}

/// <summary>
/// Veri bütünlüğü kısıtlaması ihlal edildiğinde fırlatılan exception
/// </summary>
public class DataIntegrityException : ApplicationException
{
    /// <summary>
    /// Kısıtlama adı
    /// </summary>
    public string ConstraintName { get; }
    
    /// <summary>
    /// Tablo adı
    /// </summary>
    public string TableName { get; }

    /// <summary>
    /// DataIntegrityException constructor
    /// </summary>
    /// <param name="constraintName">Kısıtlama adı</param>
    /// <param name="tableName">Tablo adı</param>
    /// <param name="customMessage">Özel hata mesajı</param>
    /// <param name="innerException">İç exception</param>
    public DataIntegrityException(string constraintName, string tableName, string? customMessage = null, Exception? innerException = null)
        : base(
            customMessage ?? $"Data integrity constraint '{constraintName}' violated in table '{tableName}'",
            "DATA_INTEGRITY_VIOLATION",
            innerException!,
            new { ConstraintName = constraintName, TableName = tableName })
    {
        ConstraintName = constraintName;
        TableName = tableName;
    }
}
