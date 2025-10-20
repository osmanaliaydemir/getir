using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.ProductReviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Ürün yorumlarını yönetmek için controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Product Reviews")]
public class ProductReviewController : BaseController
{
    private readonly IProductReviewService _productReviewService;

    public ProductReviewController(IProductReviewService productReviewService)
    {
        _productReviewService = productReviewService;
    }

    /// <summary>
    /// Ürün için yeni review oluştur (sadece sipariş veren kullanıcılar)
    /// </summary>
    /// <param name="request">Review oluşturma talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan review</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ProductReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateProductReview(
        [FromBody] CreateProductReviewRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _productReviewService.CreateProductReviewAsync(request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kendi review'unu güncelle
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <param name="request">Güncelleme talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenen review</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ProductReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProductReview(
        [FromRoute] Guid id,
        [FromBody] UpdateProductReviewRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _productReviewService.UpdateProductReviewAsync(id, request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kendi review'unu sil
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteProductReview(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _productReviewService.DeleteProductReviewAsync(id, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Ürünün tüm review'larını getir
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Review listesi</returns>
    [HttpGet("product/{productId:guid}")]
    [ProducesResponseType(typeof(PagedResult<ProductReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductReviews(
        [FromRoute] Guid productId,
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _productReviewService.GetProductReviewsAsync(productId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kullanıcının tüm review'larını getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Review listesi</returns>
    [HttpGet("my-reviews")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<ProductReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyReviews(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _productReviewService.GetUserReviewsAsync(userId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Review'a faydalı/faydasız oy ver
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <param name="isHelpful">true = faydalı, false = faydasız</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("{id:guid}/vote")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VoteReviewHelpful(
        [FromRoute] Guid id,
        [FromQuery] bool isHelpful,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _productReviewService.VoteReviewHelpfulAsync(id, userId, isHelpful, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Ürün rating'ini manuel olarak yeniden hesapla (Admin only)
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("product/{productId:guid}/recalculate-rating")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecalculateProductRating(
        [FromRoute] Guid productId,
        CancellationToken ct = default)
    {
        var result = await _productReviewService.RecalculateProductRatingAsync(productId, ct);
        return ToActionResult(result);
    }
}

