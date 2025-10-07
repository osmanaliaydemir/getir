// System namespaces
using System.Net;
using System.Text.Json;

// Third-party namespaces
using FluentValidation;

// Application namespaces
using Getir.Application.Common.Exceptions;
using Getir.Application.Common.Extensions;

namespace Getir.WebApi.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
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
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleValidationExceptionAsync(HttpContext context, FluentValidation.ValidationException exception)
    {
        _logger.LogWarning(exception, "Validation error occurred");

        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "One or more validation errors occurred.",
            status = context.Response.StatusCode,
            errorCode = "VALIDATION_ERROR",
            errors,
            traceId = context.TraceIdentifier,
            requestId = context.Items["RequestId"]?.ToString()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
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

        context.Response.StatusCode = (int)exception.ToHttpStatusCode();
        context.Response.ContentType = "application/json";

        var problemDetails = exception.CreateErrorResponse(context.TraceIdentifier);

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        // Response'un zaten başlamış olup olmadığını kontrol et
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
        }

        // Response başlamışsa yazma, sadece logla
        if (!context.Response.HasStarted)
        {
            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "An error occurred while processing your request.",
                status = context.Response.StatusCode,
                errorCode = "INTERNAL_SERVER_ERROR",
                detail = exception.Message,
                traceId = context.TraceIdentifier,
                requestId = context.Items["RequestId"]?.ToString()
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
        else
        {
            // Response zaten başlamışsa sadece logla
            _logger.LogWarning("Cannot write error response because response has already started. Exception: {Exception}", exception.Message);
        }
    }
}
