using Getir.Application.Services.Internationalization;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace Getir.WebApi.Middleware;

public class LocalizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LocalizationMiddleware> _logger;

    public LocalizationMiddleware(RequestDelegate next, ILogger<LocalizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserLanguageService userLanguageService)
    {
        try
        {
            // Get language from various sources in order of priority
            var languageCode = await GetLanguageFromRequestAsync(context, userLanguageService);

            // Set culture for the current request
            SetCulture(languageCode);

            // Add language to response headers
            context.Response.Headers.Add("X-Language", languageCode.GetCultureCode());

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LocalizationMiddleware");
            await _next(context);
        }
    }

    private async Task<LanguageCode> GetLanguageFromRequestAsync(HttpContext context, IUserLanguageService userLanguageService)
    {
        // 1. Check Accept-Language header
        var acceptLanguage = context.Request.Headers["Accept-Language"].FirstOrDefault();
        if (!string.IsNullOrEmpty(acceptLanguage))
        {
            var languageFromHeader = GetLanguageFromAcceptLanguage(acceptLanguage);
            if (languageFromHeader != LanguageCode.Turkish) // If not default, return it
            {
                return languageFromHeader;
            }
        }

        // 2. Check query parameter
        var queryLanguage = context.Request.Query["lang"].FirstOrDefault();
        if (!string.IsNullOrEmpty(queryLanguage))
        {
            if (Enum.TryParse<LanguageCode>(queryLanguage, true, out var languageFromQuery))
            {
                return languageFromQuery;
            }
        }

        // 3. Check user preference (if user is authenticated)
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("userId");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                var userLanguageResult = await userLanguageService.GetUserPreferredLanguageAsync(userId);
                if (userLanguageResult.IsSuccess)
                {
                    return userLanguageResult.Data;
                }
            }
        }

        // 4. Check cookie
        var cookieLanguage = context.Request.Cookies["language"];
        if (!string.IsNullOrEmpty(cookieLanguage))
        {
            if (Enum.TryParse<LanguageCode>(cookieLanguage, true, out var languageFromCookie))
            {
                return languageFromCookie;
            }
        }

        // 5. Default to Turkish
        return LanguageCode.Turkish;
    }

    private static LanguageCode GetLanguageFromAcceptLanguage(string acceptLanguage)
    {
        // Parse Accept-Language header (e.g., "en-US,en;q=0.9,tr;q=0.8")
        var languages = acceptLanguage.Split(',')
            .Select(lang => lang.Split(';')[0].Trim().ToLower())
            .ToList();

        foreach (var lang in languages)
        {
            if (lang.StartsWith("tr"))
                return LanguageCode.Turkish;
            if (lang.StartsWith("en"))
                return LanguageCode.English;
            if (lang.StartsWith("ar"))
                return LanguageCode.Arabic;
        }

        return LanguageCode.Turkish; // Default
    }

    private static void SetCulture(LanguageCode languageCode)
    {
        var culture = new CultureInfo(languageCode.GetCultureCode());
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}
