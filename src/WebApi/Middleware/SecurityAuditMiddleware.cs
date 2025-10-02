using Getir.Domain.Entities;
using Getir.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Getir.WebApi.Middleware;

public class SecurityAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityAuditMiddleware> _logger;

    public SecurityAuditMiddleware(RequestDelegate next, ILogger<SecurityAuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        var startTime = DateTime.UtcNow;
        var requestId = context.TraceIdentifier;
        var ipAddress = GetClientIpAddress(context);
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var userId = GetUserId(context);
        var userName = GetUserName(context);

        try
        {
            await _next(context);
            
            // Log successful requests for security-sensitive endpoints
            if (IsSecuritySensitiveEndpoint(context.Request.Path))
            {
                await LogSecurityEvent(dbContext, new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = userId ?? "anonymous",
                    UserName = userName ?? "anonymous",
                    Action = GetActionFromPath(context.Request.Path, context.Request.Method),
                    EntityType = GetEntityTypeFromPath(context.Request.Path),
                    EntityId = GetEntityIdFromPath(context.Request.Path) ?? "N/A",
                    Details = $"Request completed successfully. Status: {context.Response.StatusCode}",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    SessionId = context.Session?.Id,
                    RequestId = requestId,
                    IsSuccess = context.Response.StatusCode < 400,
                    Timestamp = startTime
                });
            }
        }
        catch (Exception ex)
        {
            // Log security events for failed requests
            if (IsSecuritySensitiveEndpoint(context.Request.Path))
            {
                await LogSecurityEvent(dbContext, new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = userId ?? "anonymous",
                    UserName = userName ?? "anonymous",
                    Action = GetActionFromPath(context.Request.Path, context.Request.Method),
                    EntityType = GetEntityTypeFromPath(context.Request.Path),
                    EntityId = GetEntityIdFromPath(context.Request.Path) ?? "N/A",
                    Details = $"Request failed with exception: {ex.Message}",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    SessionId = context.Session?.Id,
                    RequestId = requestId,
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    Timestamp = startTime
                });
            }
            
            throw;
        }
    }

    private async Task LogSecurityEvent(AppDbContext dbContext, AuditLog auditLog)
    {
        try
        {
            dbContext.AuditLogs.Add(auditLog);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log security audit event");
        }
    }

    private bool IsSecuritySensitiveEndpoint(PathString path)
    {
        var sensitivePaths = new[]
        {
            "/api/v1/auth",
            "/api/v1/admin",
            "/api/v1/merchants/onboarding",
            "/api/v1/orders"
        };

        return sensitivePaths.Any(sensitivePath => path.StartsWithSegments(sensitivePath));
    }

    private string GetActionFromPath(PathString path, string method)
    {
        if (path.StartsWithSegments("/api/v1/auth"))
        {
            if (path.Value.Contains("login")) return "LOGIN";
            if (path.Value.Contains("register")) return "REGISTER";
            if (path.Value.Contains("logout")) return "LOGOUT";
            if (path.Value.Contains("refresh")) return "REFRESH_TOKEN";
        }
        
        if (path.StartsWithSegments("/api/v1/admin"))
        {
            return method.ToUpper() + "_ADMIN";
        }

        return method.ToUpper();
    }

    private string GetEntityTypeFromPath(PathString path)
    {
        if (path.StartsWithSegments("/api/v1/auth")) return "AUTH";
        if (path.StartsWithSegments("/api/v1/admin")) return "ADMIN";
        if (path.StartsWithSegments("/api/v1/merchants")) return "MERCHANT";
        if (path.StartsWithSegments("/api/v1/orders")) return "ORDER";
        
        return "UNKNOWN";
    }

    private string? GetEntityIdFromPath(PathString path)
    {
        var segments = path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 4 && Guid.TryParse(segments[3], out _))
        {
            return segments[3];
        }
        return null;
    }

    private string? GetUserId(HttpContext context)
    {
        return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private string? GetUserName(HttpContext context)
    {
        return context.User?.FindFirst(ClaimTypes.Name)?.Value;
    }

    private string GetClientIpAddress(HttpContext context)
    {
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
