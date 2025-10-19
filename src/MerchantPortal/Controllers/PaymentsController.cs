using Getir.MerchantPortal.Models;
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

    /// <summary>
    /// Payment history page
    /// </summary>
    public IActionResult Index()
    {
        ViewBag.Title = "Ödeme Geçmişi";
        return View();
    }

    /// <summary>
    /// Reports page
    /// </summary>
    public IActionResult Reports()
    {
        ViewBag.Title = "Ödeme Raporları";
        return View();
    }

    /// <summary>
    /// Settlements page
    /// </summary>
    public IActionResult Settlements()
    {
        ViewBag.Title = "Mutabakat Raporları";
        return View();
    }

    /// <summary>
    /// Get payment history data (for DataTables)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetPaymentHistoryData([FromBody] PaymentFilterModel filter)
    {
        try
        {
            var merchantIdStr = HttpContext.Session.GetString("MerchantId");
            if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
            {
                return Json(new { success = false, message = "Merchant not found" });
            }

            var payments = await _paymentService.GetPaymentHistoryAsync(merchantId, filter);

            return Json(new
            {
                success = true,
                data = payments,
                recordsTotal = payments.Count,
                recordsFiltered = payments.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history data");
            return Json(new { success = false, message = "Error loading payment data" });
        }
    }

    /// <summary>
    /// Get payment method breakdown (for charts)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaymentMethodBreakdown(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
                return Json(new { success = false, message = "Merchant not found" });
            }

            var breakdown = await _paymentService.GetPaymentMethodBreakdownAsync(merchantId, startDate, endDate);

            return Json(new
            {
                success = true,
                data = new
                {
                    labels = breakdown.Select(b => b.DisplayName).ToArray(),
                    datasets = new[]
                    {
                        new
                        {
                            label = "Ciro (₺)",
                            data = breakdown.Select(b => b.TotalAmount).ToArray(),
                            backgroundColor = breakdown.Select(b => b.Color).ToArray(),
                            borderWidth = 0
                        }
                    }
                },
                breakdown
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment method breakdown");
            return Json(new { success = false, message = "Error loading breakdown data" });
        }
    }

    /// <summary>
    /// Get revenue analytics
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRevenueAnalytics()
    {
        try
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
                return Json(new { success = false, message = "Merchant not found" });
            }

            var analytics = await _paymentService.GetRevenueAnalyticsAsync(merchantId);

            return Json(new { success = true, data = analytics });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue analytics");
            return Json(new { success = false, message = "Error loading analytics" });
        }
    }

    /// <summary>
    /// Get settlement report
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSettlementReport(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var merchantIdStr = HttpContext.Session.GetString("MerchantId");
            if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
            {
                return Json(new { success = false, message = "Merchant not found" });
            }

            var start = startDate ?? DateTime.Now.AddMonths(-1).Date;
            var end = endDate ?? DateTime.Now.Date;

            var report = await _paymentService.GetSettlementReportAsync(merchantId, start, end);

            return Json(new { success = true, data = report });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settlement report");
            return Json(new { success = false, message = "Error loading settlement report" });
        }
    }

    /// <summary>
    /// Export payments to Excel
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ExportToExcel([FromBody] PaymentExportRequest request)
    {
        try
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
            {
                return BadRequest("Merchant not found");
            }

            var fileContent = await _paymentService.ExportToExcelAsync(merchantId, request);
            var fileName = $"Payments_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(fileContent, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to Excel");
            return StatusCode(500, "Error exporting data");
        }
    }

    /// <summary>
    /// Export payments to PDF
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ExportToPdf([FromBody] PaymentExportRequest request)
    {
        try
        {
            var merchantIdStr = HttpContext.Session.GetString("MerchantId");
            if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
            {
                return BadRequest("Merchant not found");
            }

            var fileContent = await _paymentService.ExportToPdfAsync(merchantId, request);
            var fileName = $"Payments_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(fileContent, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to PDF");
            return StatusCode(500, "Error exporting data");
        }
    }
}
