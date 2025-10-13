using Getir.MerchantPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly ILogger<SettingsController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SettingsController(
        ILogger<SettingsController> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Notifications()
    {
        // Get current settings from localStorage (client-side)
        // For now, just render the view
        return View();
    }

    [HttpPost]
    public IActionResult SaveNotificationPreferences([FromBody] NotificationPreferences preferences)
    {
        try
        {
            // For now, preferences are stored in browser localStorage
            // Future: Save to database via UserNotificationPreferences entity
            
            _logger.LogInformation("Notification preferences updated for user");
            
            return Json(new { success = true, message = "Bildirim tercihleri kaydedildi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving notification preferences");
            return Json(new { success = false, message = "Tercihler kaydedilemedi: " + ex.Message });
        }
    }
}

public class NotificationPreferences
{
    public bool SoundEnabled { get; set; } = true;
    public bool DesktopNotifications { get; set; } = true;
    public bool EmailNotifications { get; set; } = false;
    public bool NewOrderNotifications { get; set; } = true;
    public bool StatusChangeNotifications { get; set; } = true;
    public bool CancellationNotifications { get; set; } = true;
    public bool DoNotDisturbEnabled { get; set; } = false;
    public TimeSpan? DoNotDisturbStart { get; set; }
    public TimeSpan? DoNotDisturbEnd { get; set; }
    public string NotificationSound { get; set; } = "default";
}

