using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Review controller for review management
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
    /// Create a new review
    /// </summary>
    /// <param name="request">Create review request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created review</returns>
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
            return Created($"/api/v1/reviews/{result.Value.Id}", result.Value);
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Update an existing review
    /// </summary>
    /// <param name="reviewId">Review ID</param>
    /// <param name="request">Update review request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated review</returns>
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
    /// Delete a review
    /// </summary>
    /// <param name="reviewId">Review ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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
    /// Get a specific review
    /// </summary>
    /// <param name="reviewId">Review ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Review details</returns>
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
    /// Get reviews with filters
    /// </summary>
    /// <param name="query">Review query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged reviews</returns>
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
    /// Get reviews for a specific merchant or courier
    /// </summary>
    /// <param name="entityId">Entity ID</param>
    /// <param name="entityType">Entity type</param>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged reviews</returns>
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
    /// Get reviews by a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged reviews</returns>
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
    /// Get reviews by order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Order reviews</returns>
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
    /// Get review statistics
    /// </summary>
    /// <param name="entityId">Entity ID</param>
    /// <param name="entityType">Entity type</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Review statistics</returns>
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
    /// Report a review
    /// </summary>
    /// <param name="reviewId">Review ID</param>
    /// <param name="request">Report review request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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

        return BadRequest("Report review functionality not implemented yet");
    }

    /// <summary>
    /// Like a review
    /// </summary>
    /// <param name="reviewId">Review ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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

        return BadRequest("Like review functionality not implemented yet");
    }

    /// <summary>
    /// Unlike a review
    /// </summary>
    /// <param name="reviewId">Review ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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

        return BadRequest("Unlike review functionality not implemented yet");
    }
}
