using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Net;

namespace WebApp.Services;

public class GlobalErrorHandler : IErrorHandler
{
    private readonly ILogger<GlobalErrorHandler> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalErrorHandler(ILogger<GlobalErrorHandler> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async Task HandleErrorAsync(Exception exception)
    {
        try
        {
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            // Log additional context if available
            if (exception is HttpRequestException httpEx)
            {
                _logger.LogError("HTTP Request Exception - Status: {StatusCode}, Message: {Message}", 
                    httpEx.Data["StatusCode"], httpEx.Message);
            }

            // In production, you might want to send notifications to monitoring services
            if (!_environment.IsDevelopment())
            {
                await SendErrorNotificationAsync(exception);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error occurred while handling global error");
        }
    }

    public async Task HandleBlazorErrorAsync(Exception exception)
    {
        try
        {
            _logger.LogError(exception, "Blazor unhandled exception: {Message}", exception.Message);

            // Handle specific Blazor errors
            if (exception is InvalidOperationException invalidOpEx)
            {
                _logger.LogWarning("Invalid operation in Blazor: {Message}", invalidOpEx.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error occurred while handling Blazor error");
        }

        await Task.CompletedTask;
    }

    private async Task SendErrorNotificationAsync(Exception exception)
    {
        try
        {
            // Here you can integrate with external monitoring services like:
            // - Application Insights
            // - Sentry
            // - Custom notification system
            
            _logger.LogInformation("Error notification would be sent here for: {ExceptionType}", 
                exception.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send error notification");
        }

        await Task.CompletedTask;
    }
}

public interface IErrorHandler
{
    Task HandleErrorAsync(Exception exception);
    Task HandleBlazorErrorAsync(Exception exception);
}
