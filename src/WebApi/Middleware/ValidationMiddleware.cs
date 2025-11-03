using FluentValidation;
using System.Text.Json;

namespace Getir.WebApi.Middleware;

public class ValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationMiddleware> _logger;

    public ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
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
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed: {Errors}", ex.Errors);
            
            var grouped = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var response = new
            {
                isSuccess = false,
                data = (object?)null,
                error = "One or more validation errors occurred",
                errorCode = "VALIDATION_ERROR",
                metadata = new Dictionary<string, object>
                {
                    ["validationErrors"] = grouped,
                    ["traceId"] = context.TraceIdentifier,
                    ["requestId"] = context.Items["RequestId"]?.ToString() ?? string.Empty
                }
            };

            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}
