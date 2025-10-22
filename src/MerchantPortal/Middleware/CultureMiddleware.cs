using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace Getir.MerchantPortal.Middleware;

/// <summary>
/// Custom culture middleware for better localization handling
/// </summary>
public class CultureMiddleware
{
    private readonly RequestDelegate _next;

    public CultureMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get culture from cookie
        var cultureCookie = context.Request.Cookies["MerchantPortal.Culture"];
        
        if (!string.IsNullOrEmpty(cultureCookie))
        {
            try
            {
                var culture = CookieRequestCultureProvider.ParseCookieValue(cultureCookie);
                if (culture != null)
                {
                    // Set both culture and UI culture
                    var cultureInfo = new CultureInfo(culture.Cultures.FirstOrDefault().Value ?? "tr-TR");
                    var uiCultureInfo = new CultureInfo(culture.UICultures.FirstOrDefault().Value ?? "tr-TR");
                    
                    CultureInfo.CurrentCulture = cultureInfo;
                    CultureInfo.CurrentUICulture = uiCultureInfo;
                    
                    // Also set thread cultures
                    Thread.CurrentThread.CurrentCulture = cultureInfo;
                    Thread.CurrentThread.CurrentUICulture = uiCultureInfo;
                }
            }
            catch (Exception ex)
            {
                // Log error but don't break the request
                Console.WriteLine($"Culture parsing error: {ex.Message}");
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method to register the middleware
/// </summary>
public static class CultureMiddlewareExtensions
{
    public static IApplicationBuilder UseCultureMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CultureMiddleware>();
    }
}
