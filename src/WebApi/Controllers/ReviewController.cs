using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Değerlendirme yönetimi için değerlendirme controller'ı
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Reviews")]
public class ReviewController : BaseController
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Yeni değerlendirme oluştur
    /// </summary>
    /// <param name="request">Değerlendirme oluşturma talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan değerlendirme</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateReview(
        [FromBody] CreateReviewRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _reviewService.CreateReviewAsync(request, userId, ct);
        if (result.Success)
        {
            return Created($"/api/v1/reviews/{result.Value!.Id}", result.Value);
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Mevcut bir değerlendirmeyi güncelle
    /// </summary>
    /// <param name="reviewId">Değerlendirme ID</param>
    /// <param name="request">Değerlendirme güncelleme isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenen değerlendirme</returns>
    [HttpPut("{reviewId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReview(
        [FromRoute] Guid reviewId,
        [FromBody] UpdateReviewRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _reviewService.UpdateReviewAsync(reviewId, request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Değerlendirmeyi sil
    /// </summary>
    /// <param name="reviewId">Değerlendirme ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpDelete("{reviewId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(
        [FromRoute] Guid reviewId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _reviewService.DeleteReviewAsync(reviewId, userId, ct);
        if (result.Success)
        {
            return NoContent();
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Belirli bir değerlendirmeyi getir
    /// </summary>
    /// <param name="reviewId">Değerlendirme ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Değerlendirme detayları</returns>
    [HttpGet("{reviewId:guid}")]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReview(
        [FromRoute] Guid reviewId,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetReviewAsync(reviewId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Filtrelerle değerlendirmeleri getir
    /// </summary>
    /// <param name="query">Değerlendirme arama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış değerlendirmeler</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviews(
        [FromQuery] ReviewSearchQuery query,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetReviewsAsync(query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Belirli bir mağaza veya kurye için değerlendirmeleri getir
    /// </summary>
    /// <param name="entityId">Varlık ID</param>
    /// <param name="entityType">Varlık türü</param>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış değerlendirmeler</returns>
    [HttpGet("entity/{entityId:guid}/{entityType}")]
    [ProducesResponseType(typeof(PagedResult<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewsByEntity(
        [FromRoute] Guid entityId,
        [FromRoute] string entityType,
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetReviewsByRevieweeAsync(entityId, entityType, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Belirli bir kullanıcıya ait değerlendirmeleri getir
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış değerlendirmeler</returns>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(PagedResult<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewsByUser(
        [FromRoute] Guid userId,
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetReviewsByReviewerAsync(userId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Siparişe ait değerlendirmeleri getir
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sipariş değerlendirmeleri</returns>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewsByOrder(
        [FromRoute] Guid orderId,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetReviewsByOrderAsync(orderId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Değerlendirme istatistiklerini getir
    /// </summary>
    /// <param name="entityId">Varlık ID</param>
    /// <param name="entityType">Varlık türü</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Değerlendirme istatistikleri</returns>
    [HttpGet("statistics/{entityId:guid}/{entityType}")]
    [ProducesResponseType(typeof(ReviewStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewStatistics(
        [FromRoute] Guid entityId,
        [FromRoute] string entityType,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetReviewStatsAsync(entityId, entityType, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Bir değerlendirmeyi rapor et
    /// </summary>
    /// <param name="reviewId">Değerlendirme ID</param>
    /// <param name="request">Raporlama isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("{reviewId:guid}/report")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReportReview(
        [FromRoute] Guid reviewId,
        [FromBody] ReportReviewRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _reviewService.ReportReviewAsync(reviewId, request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Bir değerlendirmeyi beğen
    /// </summary>
    /// <param name="reviewId">Değerlendirme ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("{reviewId:guid}/like")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LikeReview(
        [FromRoute] Guid reviewId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _reviewService.LikeReviewAsync(reviewId, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Bir değerlendirmeyi beğenmekten vazgeç
    /// </summary>
    /// <param name="reviewId">Değerlendirme ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpDelete("{reviewId:guid}/like")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlikeReview(
        [FromRoute] Guid reviewId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _reviewService.UnlikeReviewAsync(reviewId, userId, ct);
        return ToActionResult(result);
    }
}
