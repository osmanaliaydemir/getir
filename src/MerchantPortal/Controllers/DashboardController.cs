using Getir.MerchantPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IMerchantService _merchantService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IMerchantService merchantService,
        ILogger<DashboardController> logger)
    {
        _merchantService = merchantService;
        _logger = logger;
    }

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

        ViewBag.Dashboard = dashboard;
        ViewBag.RecentOrders = recentOrders ?? new();
        ViewBag.TopProducts = topProducts ?? new();

        return View();
    }
}

