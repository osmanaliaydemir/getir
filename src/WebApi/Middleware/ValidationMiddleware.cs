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
            
            var response = new
            {
                success = false,
                message = "Validation failed",
                errors = ex.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage,
                    attemptedValue = e.AttemptedValue
                }).ToList()
            };

            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}
