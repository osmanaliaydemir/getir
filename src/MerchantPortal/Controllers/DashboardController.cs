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

    /// <summary>
    /// Satış trendi verilerini getirir (Chart.js için)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSalesChartData(int days = 30)
    {
        try
        {
            var merchantIdStr = HttpContext.Session.GetString("MerchantId");
            if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
            {
                return Json(new { success = false, message = "Merchant not found" });
            }

            // Generate sales trend data for last N days
            var labels = new List<string>();
            var revenueData = new List<decimal>();
            var orderData = new List<int>();

            for (int i = days - 1; i >= 0; i--)
            {
                var date = DateTime.Now.Date.AddDays(-i);
                labels.Add(date.ToString("dd MMM"));
                
                // Mock data - in real scenario, fetch from database
                var dayRevenue = new Random(date.GetHashCode()).Next(500, 5000);
                var dayOrders = new Random(date.GetHashCode() + 1).Next(5, 50);
                
                revenueData.Add(dayRevenue);
                orderData.Add(dayOrders);
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    labels,
                    datasets = new object[]
                    {
                        new
                        {
                            label = "Ciro (₺)",
                            data = revenueData,
                            borderColor = "#5D3EBC",
                            backgroundColor = "rgba(93, 62, 188, 0.1)",
                            tension = 0.4,
                            fill = true
                        },
                        new
                        {
                            label = "Sipariş Sayısı",
                            data = orderData,
                            borderColor = "#FFD300",
                            backgroundColor = "rgba(255, 211, 0, 0.1)",
                            tension = 0.4,
                            fill = true,
                            yAxisID = "y1"
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales chart data");
            return Json(new { success = false, message = "Error loading chart data" });
        }
    }

    /// <summary>
    /// Sipariş durumu dağılımını getirir (Chart.js için)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrdersChartData()
    {
        try
        {
            var merchantIdStr = HttpContext.Session.GetString("MerchantId");
            if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
            {
                return Json(new { success = false, message = "Merchant not found" });
            }

            var dashboard = await _merchantService.GetDashboardAsync(merchantId);

            return Json(new
            {
                success = true,
                data = new
                {
                    labels = new[] { "Bekleyen", "Hazırlanıyor", "Hazır", "Yolda", "Teslim", "İptal" },
                    datasets = new[]
                    {
                        new
                        {
                            label = "Siparişler",
                            data = new[]
                            {
                                dashboard?.PendingOrders ?? 0,
                                new Random().Next(5, 20), // Mock preparing
                                new Random().Next(2, 10), // Mock ready
                                new Random().Next(3, 15), // Mock on way
                                dashboard?.TodayOrders ?? 0,
                                new Random().Next(0, 5) // Mock cancelled
                            },
                            backgroundColor = new[]
                            {
                                "rgba(255, 193, 7, 0.8)",   // Warning - Pending
                                "rgba(13, 110, 253, 0.8)",  // Primary - Preparing
                                "rgba(25, 135, 84, 0.8)",   // Success - Ready
                                "rgba(23, 162, 184, 0.8)",  // Info - On Way
                                "rgba(40, 167, 69, 0.8)",   // Success - Delivered
                                "rgba(220, 53, 69, 0.8)"    // Danger - Cancelled
                            },
                            borderWidth = 0
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders chart data");
            return Json(new { success = false, message = "Error loading chart data" });
        }
    }

    /// <summary>
    /// Kategori dağılımını getirir (Chart.js için)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategoryChartData()
    {
        try
        {
            var merchantIdStr = HttpContext.Session.GetString("MerchantId");
            if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
            {
                return Json(new { success = false, message = "Merchant not found" });
            }

            // Mock category data - in real scenario, fetch from database
            var categories = new[]
            {
                new { Name = "Yiyecek", Revenue = 12500m, Color = "#FF6384" },
                new { Name = "İçecek", Revenue = 8300m, Color = "#36A2EB" },
                new { Name = "Atıştırmalık", Revenue = 5200m, Color = "#FFCE56" },
                new { Name = "Tatlı", Revenue = 6800m, Color = "#4BC0C0" },
                new { Name = "Diğer", Revenue = 3200m, Color = "#9966FF" }
            };

            return Json(new
            {
                success = true,
                data = new
                {
                    labels = categories.Select(c => c.Name).ToArray(),
                    datasets = new[]
                    {
                        new
                        {
                            label = "Ciro (₺)",
                            data = categories.Select(c => c.Revenue).ToArray(),
                            backgroundColor = categories.Select(c => c.Color).ToArray(),
                            borderWidth = 2,
                            borderColor = "#fff"
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category chart data");
            return Json(new { success = false, message = "Error loading chart data" });
        }
    }
}

