using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Getir.MerchantPortal.Middleware;

/// <summary>
/// Validates that authenticated users have a valid session with JWT token
/// Prevents authentication loop when cookie exists but session is expired
/// </summary>
public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionValidationMiddleware> _logger;

    public SessionValidationMiddleware(
        RequestDelegate next,
        ILogger<SessionValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip validation for login/logout pages and static files
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.Contains("/auth/login") || 
            path.Contains("/auth/logout") ||
            path.Contains("/css/") ||
            path.Contains("/js/") ||
            path.Contains("/lib/") ||
            path.Contains("/sounds/"))
        {
            await _next(context);
            return;
        }

        // Check if user is authenticated via cookie
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Validate that session has JWT token
            var jwtToken = context.Session.GetString("JwtToken");
            
            if (string.IsNullOrEmpty(jwtToken))
            {
                // Cookie exists but session is invalid (expired/cleared)
                _logger.LogWarning("Authenticated user has no JWT token in session. Signing out user.");
                
                // Clear cookie authentication
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                
                // Clear any remaining session data
                context.Session.Clear();
                
                // Redirect to login with return URL
                var returnUrl = context.Request.Path + context.Request.QueryString;
                context.Response.Redirect($"/Auth/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                return;
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method to register the middleware
/// </summary>
public static class SessionValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionValidationMiddleware>();
    }
}

