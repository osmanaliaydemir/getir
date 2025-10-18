using FluentValidation;
using Getir.Application.Common;
using Getir.Application.Common.Exceptions;
using Getir.Application.Common.Extensions;
using System.Net;
using System.Text.Json;

namespace Getir.WebApi.Middleware;

/// <summary>
/// Global exception handler that standardizes all error responses to ApiResponse format
/// This ensures consistency across the entire API
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (FluentValidation.ValidationException validationException)
        {
            await HandleValidationExceptionAsync(context, validationException);
        }
        catch (Getir.Application.Common.Exceptions.ApplicationException applicationException)
        {
            await HandleApplicationExceptionAsync(context, applicationException);
        }
        catch (Exception exception)
        {
            await HandleGenericExceptionAsync(context, exception);
        }
    }

    private async Task HandleValidationExceptionAsync(HttpContext context, FluentValidation.ValidationException exception)
    {
        _logger.LogWarning(exception, "Validation error occurred");

        // Check if response has already started
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Cannot write validation error response because response has already started");
            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        // Group validation errors by property
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        // Use ApiResponse format with metadata for validation errors
        var response = new
        {
            isSuccess = false,
            data = (object?)null,
            error = "One or more validation errors occurred",
            errorCode = "VALIDATION_ERROR",
            metadata = new Dictionary<string, object>
            {
                ["validationErrors"] = errors,
                ["traceId"] = context.TraceIdentifier,
                ["requestId"] = context.Items["RequestId"]?.ToString() ?? ""
            }
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private async Task HandleApplicationExceptionAsync(HttpContext context, Getir.Application.Common.Exceptions.ApplicationException exception)
    {
        // Log with appropriate level based on exception type
        if (exception.ShouldLogAsError())
        {
            _logger.LogError(exception, "Application error occurred: {ErrorCode}", exception.ErrorCode);
        }
        else if (exception.ShouldLogAsWarning())
        {
            _logger.LogWarning(exception, "Application warning: {ErrorCode}", exception.ErrorCode);
        }
        else
        {
            _logger.LogInformation(exception, "Application info: {ErrorCode}", exception.ErrorCode);
        }

        // Check if response has already started
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Cannot write application error response because response has already started");
            return;
        }

        context.Response.StatusCode = (int)exception.ToHttpStatusCode();
        context.Response.ContentType = "application/json";

        // Use standard ApiResponse format
        var response = ApiResponse.Fail(exception.Message, exception.ErrorCode);
        
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private async Task HandleGenericExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        // Check if response has already started
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Cannot write error response because response has already started. Exception: {Exception}", exception.Message);
            return;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var errorCode = "INTERNAL_SERVER_ERROR";
        var errorMessage = "An unexpected error occurred";

        // Customize based on exception type
        switch (exception)
        {
            case ArgumentNullException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorCode = "BAD_REQUEST";
                errorMessage = exception.Message;
                break;
            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorCode = "UNAUTHORIZED";
                errorMessage = "You are not authorized to access this resource";
                break;
            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                errorCode = "NOT_FOUND";
                errorMessage = exception.Message;
                break;
            case InvalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorCode = "INVALID_OPERATION";
                errorMessage = exception.Message;
                break;
        }

        // Use standard ApiResponse format
        var response = ApiResponse.Fail(errorMessage, errorCode);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

