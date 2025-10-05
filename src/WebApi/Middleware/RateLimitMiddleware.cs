using Getir.Application.DTO;
using Getir.Application.Services.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Getir.WebApi.Middleware;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RateLimitMiddleware> _logger;

    public RateLimitMiddleware(RequestDelegate next, IServiceProvider serviceProvider, ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Skip rate limiting for certain paths
            if (ShouldSkipRateLimit(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Create a scope for scoped services
            using var scope = _serviceProvider.CreateScope();
            var rateLimitService = scope.ServiceProvider.GetRequiredService<IRateLimitService>();

            // Extract request information
            var request = ExtractRequestInfo(context);
            
            // Check rate limit
            var rateLimitResponse = await rateLimitService.CheckRateLimitAsync(request);
            
            // Log the request
            await rateLimitService.LogRequestAsync(request, rateLimitResponse);

            // Add rate limit headers to response
            AddRateLimitHeaders(context.Response, rateLimitResponse);

            // Handle rate limit exceeded
            if (rateLimitResponse.IsLimitExceeded)
            {
                await HandleRateLimitExceeded(context, rateLimitResponse);
                return;
            }

            // Handle throttling
            if (rateLimitResponse.ThrottleDelayMs.HasValue && rateLimitResponse.ThrottleDelayMs.Value > 0)
            {
                await Task.Delay(rateLimitResponse.ThrottleDelayMs.Value);
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RateLimitMiddleware");
            await _next(context);
        }
    }

    private bool ShouldSkipRateLimit(PathString path)
    {
        var skipPaths = new[]
        {
            "/health",
            "/metrics",
            "/swagger",
            "/favicon.ico"
        };

        return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase));
    }

    private RateLimitCheckRequest ExtractRequestInfo(HttpContext context)
    {
        var userId = context.User?.Identity?.IsAuthenticated == true 
            ? context.User.FindFirst("sub")?.Value 
            : null;

        var userRole = context.User?.FindFirst("role")?.Value;

        var ipAddress = GetClientIpAddress(context);

        return new RateLimitCheckRequest
        {
            Endpoint = context.Request.Path.Value,
            HttpMethod = context.Request.Method,
            UserId = userId,
            UserRole = userRole,
            IpAddress = ipAddress,
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            RequestId = context.TraceIdentifier,
            SessionId = context.Session?.Id
        };
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP first (for load balancers/proxies)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private void AddRateLimitHeaders(HttpResponse response, RateLimitCheckResponse rateLimitResponse)
    {
        foreach (var header in rateLimitResponse.Headers)
        {
            response.Headers[header.Key] = header.Value;
        }

        // Add additional standard headers
        response.Headers["X-RateLimit-Policy"] = "Getir API Rate Limiting";
        response.Headers["X-RateLimit-Version"] = "1.0";
    }

    private async Task HandleRateLimitExceeded(HttpContext context, RateLimitCheckResponse rateLimitResponse)
    {
        context.Response.StatusCode = rateLimitResponse.Action switch
        {
            Getir.Domain.Enums.RateLimitAction.Block => 429, // Too Many Requests
            Getir.Domain.Enums.RateLimitAction.Throttle => 429, // Too Many Requests
            _ => 429
        };

        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            error = "Rate limit exceeded",
            message = rateLimitResponse.Reason ?? "Too many requests",
            retryAfter = rateLimitResponse.RetryAfter?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            blockedUntil = rateLimitResponse.BlockedUntil,
            requestLimit = rateLimitResponse.RequestLimit,
            requestCount = rateLimitResponse.RequestCount,
            period = rateLimitResponse.Period.ToString(),
            action = rateLimitResponse.Action.ToString(),
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);

        _logger.LogWarning("Rate limit exceeded for {Endpoint} {Method} from {IpAddress}. User: {UserId}, Count: {Count}/{Limit}",
            rateLimitResponse.Headers.GetValueOrDefault("X-RateLimit-Endpoint", "unknown"),
            context.Request.Method,
            context.Connection.RemoteIpAddress,
            context.User?.Identity?.Name ?? "anonymous",
            rateLimitResponse.RequestCount,
            rateLimitResponse.RequestLimit);
    }
}
