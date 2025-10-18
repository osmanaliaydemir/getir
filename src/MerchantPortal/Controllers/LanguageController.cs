using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

/// <summary>
/// Language/Culture Controller
/// Handles language switching and culture preferences
/// </summary>
public class LanguageController : Controller
{
    /// <summary>
    /// Set user's preferred culture
    /// </summary>
    /// <param name="culture">Culture code (tr-TR, en-US, ar-SA)</param>
    /// <param name="returnUrl">URL to return after language change</param>
    [HttpPost]
    public IActionResult SetLanguage(string culture, string? returnUrl = null)
    {
        // Validate culture
        var supportedCultures = new[] { "tr-TR", "en-US", "ar-SA" };
        if (!supportedCultures.Contains(culture))
        {
            culture = "tr-TR"; // Fallback to Turkish
        }

        // Set culture cookie
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true
            }
        );

        // Redirect to return URL or homepage
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    /// <summary>
    /// Get current culture (for AJAX requests)
    /// </summary>
    [HttpGet]
    public IActionResult GetCurrentCulture()
    {
        var currentCulture = System.Globalization.CultureInfo.CurrentUICulture.Name;
        var isRtl = currentCulture.StartsWith("ar");

        return Json(new
        {
            culture = currentCulture,
            isRtl = isRtl,
            displayName = System.Globalization.CultureInfo.CurrentUICulture.DisplayName
        });
    }
}

