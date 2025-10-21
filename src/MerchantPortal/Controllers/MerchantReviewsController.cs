using Getir.MerchantPortal.Models;
using Getir.MerchantPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

[Authorize(Roles = "MerchantOwner")]
public class MerchantReviewsController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<MerchantReviewsController> _logger;

    public MerchantReviewsController(IReviewService reviewService, ILogger<MerchantReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    /// <summary>
    /// Merchant'ın aldığı değerlendirmeleri göster
    /// </summary>
    public async Task<IActionResult> Index(ReviewFilterModel? filter = null)
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
            return RedirectToAction("Login", "Auth");
        }

        filter ??= new ReviewFilterModel();
        filter.RevieweeType = "Merchant";

        var reviews = await _reviewService.GetMerchantReviewsAsync(merchantId, filter);
        var stats = await _reviewService.GetMerchantReviewStatsAsync(merchantId);

        ViewBag.Reviews = reviews;
        ViewBag.Stats = stats;
        ViewBag.Filter = filter;

        return View();
    }

    /// <summary>
    /// Merchant'ın kuryelerini değerlendirmeleri göster
    /// </summary>
    public async Task<IActionResult> CourierReviews(ReviewFilterModel? filter = null)
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
            return RedirectToAction("Login", "Auth");
        }

        filter ??= new ReviewFilterModel();
        filter.RevieweeType = "Courier";

        var reviews = await _reviewService.GetCourierReviewsAsync(merchantId, filter);
        var stats = await _reviewService.GetCourierReviewStatsAsync(merchantId);

        ViewBag.Reviews = reviews;
        ViewBag.Stats = stats;
        ViewBag.Filter = filter;

        return View();
    }

    /// <summary>
    /// Genel review dashboard
    /// </summary>
    public async Task<IActionResult> Dashboard()
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var dashboard = await _reviewService.GetReviewDashboardAsync(merchantId);

        return View(dashboard);
    }

    /// <summary>
    /// Review'a yanıt ver
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RespondToReview(Guid reviewId, ReviewResponseRequest request)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Geçersiz veri" });
        }

        var success = await _reviewService.RespondToReviewAsync(reviewId, request.Response);
        
        return Json(new { success, message = success ? "Yanıt başarıyla eklendi" : "Yanıt eklenirken hata oluştu" });
    }

    /// <summary>
    /// Review'ı beğen/beğenme
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ToggleLike(Guid reviewId, bool isLiked)
    {
        var success = await _reviewService.LikeReviewAsync(reviewId, isLiked);
        
        return Json(new { success, message = success ? "İşlem başarılı" : "İşlem başarısız" });
    }

    /// <summary>
    /// Review'ı rapor et
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ReportReview(Guid reviewId, ReviewReportRequest request)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Geçersiz veri" });
        }

        var success = await _reviewService.ReportReviewAsync(reviewId, request.Reason);
        
        return Json(new { success, message = success ? "Rapor başarıyla gönderildi" : "Rapor gönderilirken hata oluştu" });
    }
}
