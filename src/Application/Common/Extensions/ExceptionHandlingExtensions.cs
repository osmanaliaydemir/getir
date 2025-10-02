using Getir.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Getir.Application.Common.Extensions;

/// <summary>
/// Extension methods for exception handling
/// </summary>
public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// Maps application exceptions to HTTP status codes
    /// </summary>
    public static HttpStatusCode ToHttpStatusCode(this Exceptions.ApplicationException exception)
    {
        return exception switch
        {
            EntityNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedException => HttpStatusCode.Unauthorized,
            ValidationException => HttpStatusCode.BadRequest,
            BusinessRuleViolationException => HttpStatusCode.BadRequest,
            DuplicateResourceException => HttpStatusCode.Conflict,
            ConcurrencyConflictException => HttpStatusCode.Conflict,
            ResourceUnavailableException => HttpStatusCode.ServiceUnavailable,
            RateLimitExceededException => HttpStatusCode.TooManyRequests,
            ExternalServiceException => HttpStatusCode.BadGateway,
            ConfigurationException => HttpStatusCode.InternalServerError,
            DataIntegrityException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };
    }

    /// <summary>
    /// Determines if an exception should be logged as an error
    /// </summary>
    public static bool ShouldLogAsError(this Exception exception)
    {
        return exception switch
        {
            ValidationException => false, // Validation errors are expected
            BusinessRuleViolationException => false, // Business rule violations are expected
            EntityNotFoundException => false, // Not found is expected
            UnauthorizedException => false, // Unauthorized is expected
            DuplicateResourceException => false, // Duplicate is expected
            ConcurrencyConflictException => false, // Concurrency conflicts are expected
            RateLimitExceededException => false, // Rate limiting is expected
            _ => true // All other exceptions should be logged as errors
        };
    }

    /// <summary>
    /// Determines if an exception should be logged as a warning
    /// </summary>
    public static bool ShouldLogAsWarning(this Exception exception)
    {
        return exception switch
        {
            ResourceUnavailableException => true,
            System.TimeoutException => true,
            ExternalServiceException => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets a user-friendly error message for an exception
    /// </summary>
    public static string GetUserFriendlyMessage(this Exception exception)
    {
        return exception switch
        {
            EntityNotFoundException ex => $"The requested {ex.EntityType.ToLower()} was not found.",
            UnauthorizedException => "You are not authorized to perform this action.",
            ValidationException => "The provided data is invalid. Please check your input and try again.",
            BusinessRuleViolationException ex => ex.Message,
            DuplicateResourceException ex => $"A {ex.ResourceType.ToLower()} with this {ex.FieldName.ToLower()} already exists.",
            ConcurrencyConflictException ex => $"The {ex.EntityType.ToLower()} was modified by another user. Please refresh and try again.",
            ResourceUnavailableException ex => $"The {ex.ResourceType.ToLower()} is currently unavailable. Please try again later.",
            RateLimitExceededException ex => $"Too many requests. Please try again in {ex.ResetTime.TotalSeconds} seconds.",
            System.TimeoutException => $"The operation timed out. Please try again.",
            ExternalServiceException ex => $"External service '{ex.ServiceName}' is temporarily unavailable. Please try again later.",
            ConfigurationException => "A configuration error occurred. Please contact support.",
            DataIntegrityException => "A data integrity error occurred. Please contact support.",
            _ => "An unexpected error occurred. Please try again or contact support if the problem persists."
        };
    }

    /// <summary>
    /// Gets the error code for an exception
    /// </summary>
    public static string GetErrorCode(this Exception exception)
    {
        return exception switch
        {
            Exceptions.ApplicationException appEx => appEx.ErrorCode,
            ArgumentNullException => "NULL_ARGUMENT",
            ArgumentException => "INVALID_ARGUMENT",
            InvalidOperationException => "INVALID_OPERATION",
            NotSupportedException => "NOT_SUPPORTED",
            UnauthorizedAccessException => "ACCESS_DENIED",
            FileNotFoundException => "FILE_NOT_FOUND",
            DirectoryNotFoundException => "DIRECTORY_NOT_FOUND",
            IOException => "IO_ERROR",
            System.TimeoutException => "TIMEOUT",
            TaskCanceledException => "OPERATION_CANCELED",
            _ => "UNKNOWN_ERROR"
        };
    }

    /// <summary>
    /// Determines if an exception is retryable
    /// </summary>
    public static bool IsRetryable(this Exception exception)
    {
        return exception switch
        {
            System.TimeoutException => true,
            ExternalServiceException => true,
            ResourceUnavailableException => true,
            IOException => true,
            TaskCanceledException => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets the retry delay for a retryable exception
    /// </summary>
    public static TimeSpan GetRetryDelay(this Exception exception, int attemptNumber)
    {
        if (!exception.IsRetryable())
            return TimeSpan.Zero;

        // Exponential backoff with jitter
        var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, attemptNumber - 1));
        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
        return baseDelay.Add(jitter);
    }

    /// <summary>
    /// Logs an exception with appropriate level and context
    /// </summary>
    public static void LogException(this ILogger logger, Exception exception, string operation, object? context = null)
    {
        var logLevel = exception switch
        {
            _ when exception.ShouldLogAsError() => LogLevel.Error,
            _ when exception.ShouldLogAsWarning() => LogLevel.Warning,
            _ => LogLevel.Information
        };

        var message = $"Exception in {operation}: {exception.GetUserFriendlyMessage()}";
        var errorCode = exception.GetErrorCode();

        if (context != null)
        {
            logger.Log(logLevel, exception, "{Message} [ErrorCode: {ErrorCode}] [Context: {@Context}]", 
                message, errorCode, context);
        }
        else
        {
            logger.Log(logLevel, exception, "{Message} [ErrorCode: {ErrorCode}]", 
                message, errorCode);
        }
    }

    /// <summary>
    /// Creates a standardized error response for an exception
    /// </summary>
    public static object CreateErrorResponse(this Exception exception, string? traceId = null)
    {
        var response = new
        {
            Error = new
            {
                Code = exception.GetErrorCode(),
                Message = exception.GetUserFriendlyMessage(),
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            }
        };

        // Add details for application exceptions
        if (exception is Exceptions.ApplicationException appEx && appEx.Details != null)
        {
            return new
            {
                response.Error,
                Details = appEx.Details
            };
        }

        return response;
    }
}
