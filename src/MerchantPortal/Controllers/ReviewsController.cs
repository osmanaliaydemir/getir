using Getir.MerchantPortal.Models;
using Getir.MerchantPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly IProductReviewService _reviewService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(
        IProductReviewService reviewService,
        ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    /// <summary>
    /// Reviews ana sayfası - Merchant'ın tüm ürün yorumlarını gösterir
    /// </summary>
    public async Task<IActionResult> Index(int page = 1, int? rating = null, bool? approved = null)
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
            return RedirectToAction("Login", "Auth");
        }

        // Get reviews with pagination
        var reviews = await _reviewService.GetMerchantProductReviewsAsync(
            merchantId, 
            page, 
            20, 
            rating, 
            approved);

        // Get review stats
        var stats = await _reviewService.GetMerchantReviewStatsAsync(merchantId);

        ViewBag.Reviews = reviews;
        ViewBag.Stats = stats;
        ViewBag.CurrentPage = page;
        ViewBag.SelectedRating = rating;
        ViewBag.SelectedApproval = approved;
        ViewBag.Title = "Ürün Yorumları";

        return View();
    }

    /// <summary>
    /// Review details modal için data
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetReviewDetails(Guid reviewId)
    {
        try
        {
            var review = await _reviewService.GetReviewByIdAsync(reviewId);
            
            if (review == null)
            {
                return Json(new { success = false, message = "Review not found" });
            }

            return Json(new { success = true, data = review });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review details {ReviewId}", reviewId);
            return Json(new { success = false, message = "Error loading review details" });
        }
    }

    /// <summary>
    /// Review'a yanıt ver
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RespondToReview(Guid reviewId, string response)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                return Json(new { success = false, message = "Yanıt boş olamaz" });
            }

            var success = await _reviewService.RespondToReviewAsync(reviewId, response);

            if (success)
            {
                return Json(new { success = true, message = "Yanıtınız başarıyla eklendi" });
            }

            return Json(new { success = false, message = "Yanıt eklenirken hata oluştu" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to review {ReviewId}", reviewId);
            return Json(new { success = false, message = "İşlem sırasında hata oluştu" });
        }
    }

    /// <summary>
    /// Review'ı onayla
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveReview(Guid reviewId)
    {
        try
        {
            var success = await _reviewService.ApproveReviewAsync(reviewId);

            if (success)
            {
                return Json(new { success = true, message = "Yorum onaylandı" });
            }

            return Json(new { success = false, message = "Onaylama sırasında hata oluştu" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving review {ReviewId}", reviewId);
            return Json(new { success = false, message = "İşlem sırasında hata oluştu" });
        }
    }

    /// <summary>
    /// Review'ı reddet
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectReview(Guid reviewId, string reason)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return Json(new { success = false, message = "Red nedeni belirtilmelidir" });
            }

            var success = await _reviewService.RejectReviewAsync(reviewId, reason);

            if (success)
            {
                return Json(new { success = true, message = "Yorum reddedildi" });
            }

            return Json(new { success = false, message = "Reddetme sırasında hata oluştu" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting review {ReviewId}", reviewId);
            return Json(new { success = false, message = "İşlem sırasında hata oluştu" });
        }
    }

    /// <summary>
    /// Belirli bir ürünün yorumlarını getir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProductReviews(Guid productId, int page = 1)
    {
        try
        {
            var reviews = await _reviewService.GetProductReviewsAsync(productId, page, 10);
            var stats = await _reviewService.GetProductReviewStatsAsync(productId);

            return Json(new 
            { 
                success = true, 
                reviews = reviews?.Items ?? new List<ProductReviewResponse>(),
                stats = stats,
                totalCount = reviews?.TotalCount ?? 0,
                currentPage = page,
                totalPages = reviews?.TotalPages ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product reviews for {ProductId}", productId);
            return Json(new { success = false, message = "Error loading reviews" });
        }
    }
}

