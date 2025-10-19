using Getir.MerchantPortal.Models;
using Getir.MerchantPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

[Authorize]
public class StockController : Controller
{
    private readonly IStockService _stockService;
    private readonly IProductService _productService;
    private readonly ILogger<StockController> _logger;

    public StockController(IStockService stockService, IProductService productService, ILogger<StockController> logger)
    {
        _stockService = stockService;
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Stock alerts page
    /// </summary>
    public async Task<IActionResult> Alerts()
    {
        var alerts = await _stockService.GetStockAlertsAsync();
        return View(alerts ?? new List<StockAlertResponse>());
    }

    /// <summary>
    /// Stock history page for a product
    /// </summary>
    public async Task<IActionResult> History(Guid productId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            TempData["Error"] = "Ürün bulunamadı";
            return RedirectToAction("Index", "Products");
        }

        var history = await _stockService.GetStockHistoryAsync(productId, fromDate, toDate);

        ViewBag.Product = product;
        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;

        return View(history ?? new List<StockHistoryResponse>());
    }

    /// <summary>
    /// Bulk update stock - POST endpoint
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateStockRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Geçersiz veri" });
            }

            var success = await _stockService.BulkUpdateStockLevelsAsync(request);

            if (success)
            {
                return Json(new { success = true, message = "Stok seviyeleri başarıyla güncellendi" });
            }

            return Json(new { success = false, message = "Stok güncellemesi başarısız oldu" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating stock");
            return Json(new { success = false, message = "Bir hata oluştu" });
        }
    }

    /// <summary>
    /// Update single product stock - POST endpoint
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStock([FromBody] UpdateStockRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Geçersiz veri" });
            }

            var success = await _stockService.UpdateStockLevelAsync(request);

            if (success)
            {
                return Json(new { success = true, message = "Stok seviyesi başarıyla güncellendi" });
            }

            return Json(new { success = false, message = "Stok güncellemesi başarısız oldu" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for product {ProductId}", request.ProductId);
            return Json(new { success = false, message = "Bir hata oluştu" });
        }
    }

    /// <summary>
    /// Resolve stock alert - POST endpoint
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResolveAlert(Guid alertId, string resolutionNotes)
    {
        try
        {
            var success = await _stockService.ResolveStockAlertAsync(alertId, resolutionNotes);

            if (success)
            {
                TempData["Success"] = "Uyarı başarıyla çözüldü";
                return RedirectToAction(nameof(Alerts));
            }

            TempData["Error"] = "Uyarı çözümlenemedi";
            return RedirectToAction(nameof(Alerts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving alert {AlertId}", alertId);
            TempData["Error"] = "Bir hata oluştu";
            return RedirectToAction(nameof(Alerts));
        }
    }

    /// <summary>
    /// Manually check stock levels - POST endpoint
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckLevels()
    {
        try
        {
            var success = await _stockService.CheckStockLevelsAsync();

            if (success)
            {
                return Json(new { success = true, message = "Stok seviyeleri kontrol edildi" });
            }

            return Json(new { success = false, message = "Kontrol başarısız oldu" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock levels");
            return Json(new { success = false, message = "Bir hata oluştu" });
        }
    }

    /// <summary>
    /// Export stock to CSV
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportToCSV()
    {
        try
        {
            var merchantIdStr = HttpContext.Session.GetString("MerchantId");
            if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
            {
                return BadRequest("Merchant not found");
            }

            var csvData = await _stockService.ExportStockToCsvAsync(merchantId);
            var fileName = $"Stock_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(csvData, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting stock to CSV");
            return StatusCode(500, "Export error");
        }
    }

    /// <summary>
    /// Import stock from CSV
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ImportFromCSV(IFormFile file)
    {
        try
        {
            var merchantIdStr = HttpContext.Session.GetString("MerchantId");
            if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
            {
                return Json(new { success = false, message = "Merchant not found" });
            }

            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Dosya seçilmedi" });
            }

            using var stream = file.OpenReadStream();
            var result = await _stockService.ImportStockFromCsvAsync(merchantId, stream);

            return Json(new
            {
                success = result.SuccessCount > 0,
                totalRows = result.TotalRows,
                successCount = result.SuccessCount,
                errorCount = result.ErrorCount,
                errors = result.Errors,
                message = $"{result.SuccessCount}/{result.TotalRows} ürün güncellendi"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing stock from CSV");
            return Json(new { success = false, message = "Import error" });
        }
    }

    /// <summary>
    /// Get low stock products
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLowStockProducts(int threshold = 10)
    {
        try
        {
            var merchantIdStr = HttpContext.Session.GetString("MerchantId");
            if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
            {
                return Json(new { success = false, message = "Merchant not found" });
            }

            var products = await _stockService.GetLowStockProductsAsync(merchantId, threshold);

            return Json(new { success = true, data = products });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock products");
            return Json(new { success = false, message = "Error loading data" });
        }
    }
}

