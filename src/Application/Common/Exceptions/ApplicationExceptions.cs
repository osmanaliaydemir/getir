namespace Getir.Application.Common.Exceptions;

/// <summary>
/// Base exception for all application-specific exceptions
/// </summary>
public abstract class ApplicationException : Exception
{
    public string ErrorCode { get; }
    public object? Details { get; }

    protected ApplicationException(string message, string errorCode, object? details = null) 
        : base(message)
    {
        ErrorCode = errorCode;
        Details = details;
    }

    protected ApplicationException(string message, string errorCode, Exception innerException, object? details = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Details = details;
    }
}

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class EntityNotFoundException : ApplicationException
{
    public string EntityType { get; }
    public object EntityId { get; }

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
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : ApplicationException
{
    public string RuleName { get; }

    public BusinessRuleViolationException(string ruleName, string message, object? details = null)
        : base(message, "BUSINESS_RULE_VIOLATION", details)
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : ApplicationException
{
    public Dictionary<string, string[]> ValidationErrors { get; }

    public ValidationException(Dictionary<string, string[]> validationErrors)
        : base("Validation failed", "VALIDATION_ERROR", validationErrors)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(string field, string error)
        : base("Validation failed", "VALIDATION_ERROR", new Dictionary<string, string[]> { { field, new[] { error } } })
    {
        ValidationErrors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }
}

/// <summary>
/// Exception thrown when an operation is not authorized
/// </summary>
public class UnauthorizedException : ApplicationException
{
    public string RequiredPermission { get; }
    public string UserId { get; }

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
/// Exception thrown when a resource is not available
/// </summary>
public class ResourceUnavailableException : ApplicationException
{
    public string ResourceType { get; }
    public string ResourceId { get; }

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
/// Exception thrown when a duplicate resource is created
/// </summary>
public class DuplicateResourceException : ApplicationException
{
    public string ResourceType { get; }
    public string FieldName { get; }
    public object FieldValue { get; }

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
/// Exception thrown when a concurrency conflict occurs
/// </summary>
public class ConcurrencyConflictException : ApplicationException
{
    public string EntityType { get; }
    public object EntityId { get; }

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
/// Exception thrown when an external service is unavailable
/// </summary>
public class ExternalServiceException : ApplicationException
{
    public string ServiceName { get; }
    public string Operation { get; }

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
/// Exception thrown when a rate limit is exceeded
/// </summary>
public class RateLimitExceededException : ApplicationException
{
    public string Resource { get; }
    public int Limit { get; }
    public TimeSpan ResetTime { get; }

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
/// Exception thrown when a timeout occurs
/// </summary>
public class TimeoutException : ApplicationException
{
    public string Operation { get; }
    public TimeSpan Timeout { get; }

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
/// Exception thrown when a configuration error occurs
/// </summary>
public class ConfigurationException : ApplicationException
{
    public string ConfigurationKey { get; }

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
/// Exception thrown when a data integrity constraint is violated
/// </summary>
public class DataIntegrityException : ApplicationException
{
    public string ConstraintName { get; }
    public string TableName { get; }

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
