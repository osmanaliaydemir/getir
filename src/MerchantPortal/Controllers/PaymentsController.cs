using Getir.MerchantPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
            return RedirectToAction("Login", "Auth");
        }

        // Get payment statistics
        var stats = await _paymentService.GetPaymentStatisticsAsync(merchantId);
        ViewBag.Stats = stats;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> History(DateTime? startDate = null, DateTime? endDate = null)
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
            return RedirectToAction("Login", "Auth");
        }

        // Default to last 30 days
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var summary = await _paymentService.GetCashSummaryAsync(merchantId, startDate, endDate);

        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        ViewBag.MerchantId = merchantId;

        return View(summary);
    }

    [HttpGet]
    public async Task<IActionResult> Settlements(int page = 1)
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var settlements = await _paymentService.GetSettlementsAsync(merchantId, page, 20);
        
        return View(settlements);
    }

    [HttpGet]
    public async Task<IActionResult> Analytics()
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var stats = await _paymentService.GetPaymentStatisticsAsync(merchantId);
        ViewBag.MerchantId = merchantId;

        return View(stats);
    }
}

