using Getir.MerchantPortal.Models;
using Getir.MerchantPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IMerchantService _merchantService;
    private readonly IStockService _stockService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IMerchantService merchantService,
        IStockService stockService,
        ILogger<DashboardController> logger)
    {
        _merchantService = merchantService;
        _stockService = stockService;
        _logger = logger;
    }

    /// <summary>
    /// Dashboard sayfasını gösterir.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var dashboard = await _merchantService.GetDashboardAsync(merchantId);
        var recentOrders = await _merchantService.GetRecentOrdersAsync(merchantId, 5);
        var topProducts = await _merchantService.GetTopProductsAsync(merchantId, 5);
        var stockSummary = await _stockService.GetStockSummaryAsync();

        ViewBag.Dashboard = dashboard;
        ViewBag.RecentOrders = recentOrders ?? new();
        ViewBag.TopProducts = topProducts ?? new();
        ViewBag.StockSummary = stockSummary;

        return View();
    }

    /// <summary>
    /// Stok uyarılarını getirir.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStockAlerts()
    {
        try
        {
            var alerts = await _stockService.GetStockAlertsAsync();
            return Json(new { success = true, data = alerts ?? new List<StockAlertResponse>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock alerts");
            return Json(new { success = false, message = "Error loading stock alerts" });
        }
    }
}

