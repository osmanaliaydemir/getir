using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Getir.WebApi.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/reviews")
            .WithTags("Reviews");

        // Review CRUD operations
        group.MapPost("/", CreateReview)
            .WithName("CreateReview")
            .WithSummary("Create a new review")
            .RequireAuthorization();

        group.MapPut("/{reviewId:guid}", UpdateReview)
            .WithName("UpdateReview")
            .WithSummary("Update an existing review")
            .RequireAuthorization();

        group.MapDelete("/{reviewId:guid}", DeleteReview)
            .WithName("DeleteReview")
            .WithSummary("Delete a review")
            .RequireAuthorization();

        group.MapGet("/{reviewId:guid}", GetReview)
            .WithName("GetReview")
            .WithSummary("Get a specific review");

        // Review queries
        group.MapGet("/", GetReviews)
            .WithName("GetReviews")
            .WithSummary("Get reviews with filters");

        group.MapGet("/entity/{entityId:guid}/{entityType}", GetReviewsByEntity)
            .WithName("GetReviewsByEntity")
            .WithSummary("Get reviews for a specific merchant or courier");

        group.MapGet("/user/{userId:guid}", GetReviewsByUser)
            .WithName("GetReviewsByUser")
            .WithSummary("Get reviews by a specific user");

        group.MapGet("/order/{orderId:guid}", GetReviewsByOrder)
            .WithName("GetReviewsByOrder")
            .WithSummary("Get reviews for a specific order");

        // Rating operations
        group.MapGet("/rating/{entityId:guid}/{entityType}", GetRating)
            .WithName("GetRating")
            .WithSummary("Get rating information for an entity");

        group.MapGet("/stats/{entityId:guid}/{entityType}", GetReviewStats)
            .WithName("GetReviewStats")
            .WithSummary("Get review statistics for an entity");

        group.MapPost("/calculate", CalculateRating)
            .WithName("CalculateRating")
            .WithSummary("Calculate rating for an entity")
            .RequireAuthorization();

        // Review moderation (Admin only)
        group.MapGet("/pending", GetPendingReviews)
            .WithName("GetPendingReviews")
            .WithSummary("Get pending reviews for moderation")
            .RequireAuthorization("Admin");

        group.MapPut("/{reviewId:guid}/moderate", ModerateReview)
            .WithName("ModerateReview")
            .WithSummary("Moderate a review")
            .RequireAuthorization("Admin");

        group.MapPut("/bulk/moderate", BulkModerateReviews)
            .WithName("BulkModerateReviews")
            .WithSummary("Bulk moderate reviews")
            .RequireAuthorization("Admin");

        // Review helpful votes
        group.MapPost("/{reviewId:guid}/helpful", VoteHelpful)
            .WithName("VoteHelpful")
            .WithSummary("Vote if a review is helpful")
            .RequireAuthorization();

        group.MapDelete("/{reviewId:guid}/helpful", RemoveHelpfulVote)
            .WithName("RemoveHelpfulVote")
            .WithSummary("Remove helpful vote")
            .RequireAuthorization();

        // Review analytics
        group.MapGet("/analytics/{entityId:guid}/{entityType}", GetReviewAnalytics)
            .WithName("GetReviewAnalytics")
            .WithSummary("Get review analytics for an entity");

        group.MapPost("/search", SearchReviews)
            .WithName("SearchReviews")
            .WithSummary("Advanced search for reviews");

        // Review validation
        group.MapGet("/can-review/{revieweeId:guid}/{revieweeType}/{orderId:guid}", CanUserReview)
            .WithName("CanUserReview")
            .WithSummary("Check if user can review an entity")
            .RequireAuthorization();

        group.MapGet("/has-reviewed/{orderId:guid}", HasUserReviewedOrder)
            .WithName("HasUserReviewedOrder")
            .WithSummary("Check if user has reviewed an order")
            .RequireAuthorization();

        // Review tags
        group.MapGet("/tags/{revieweeType}", GetAvailableTags)
            .WithName("GetAvailableTags")
            .WithSummary("Get available review tags");

        group.MapGet("/tags/frequency/{entityId:guid}/{entityType}", GetTagFrequencies)
            .WithName("GetTagFrequencies")
            .WithSummary("Get tag frequency for an entity");
    }

    // Review CRUD operations
    private static async Task<IResult> CreateReview(
        [FromBody] CreateReviewRequest request,
        [FromServices] IReviewService reviewService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var result = await reviewService.CreateReviewAsync(request, userId, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> UpdateReview(
        Guid reviewId,
        [FromBody] UpdateReviewRequest request,
        [FromServices] IReviewService reviewService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var result = await reviewService.UpdateReviewAsync(reviewId, request, userId, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> DeleteReview(
        Guid reviewId,
        [FromServices] IReviewService reviewService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var result = await reviewService.DeleteReviewAsync(reviewId, userId, ct);
        return result.Success ? Results.NoContent() : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> GetReview(
        Guid reviewId,
        [FromServices] IReviewService reviewService,
        CancellationToken ct)
    {
        var result = await reviewService.GetReviewAsync(reviewId, ct);
        return result.Success ? Results.Ok(result.Value) : Results.NotFound(result.Error);
    }

    // Review queries
    private static async Task<IResult> GetReviews(
        [AsParameters] ReviewSearchQuery query,
        [FromServices] IReviewService reviewService,
        CancellationToken ct)
    {
        var result = await reviewService.GetReviewsAsync(query, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> GetReviewsByEntity(
        Guid entityId,
        string entityType,
        [AsParameters] PaginationQuery query,
        [FromServices] IReviewService reviewService,
        CancellationToken ct)
    {
        var result = await reviewService.GetReviewsByRevieweeAsync(entityId, entityType, query, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> GetReviewsByUser(
        Guid userId,
        [AsParameters] PaginationQuery query,
        [FromServices] IReviewService reviewService,
        CancellationToken ct)
    {
        var result = await reviewService.GetReviewsByReviewerAsync(userId, query, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> GetReviewsByOrder(
        Guid orderId,
        [FromServices] IReviewService reviewService,
        CancellationToken ct)
    {
        var result = await reviewService.GetReviewsByOrderAsync(orderId, ct);
        return result.ToIResult();
    }

    // Rating operations
    private static async Task<IResult> GetRating(
        Guid entityId,
        string entityType,
        [FromServices] IReviewService reviewService,
        CancellationToken ct)
    {
        var result = await reviewService.GetRatingAsync(entityId, entityType, ct);
        return result.Success ? Results.Ok(result.Value) : Results.NotFound(result.Error);
    }

    private static async Task<IResult> GetReviewStats(
        Guid entityId,
        string entityType,
        [FromServices] IReviewService reviewService,
        CancellationToken ct)
    {
        var result = await reviewService.GetReviewStatsAsync(entityId, entityType, ct);
        return result.Success ? Results.Ok(result.Value) : Results.NotFound(result.Error);
    }

    private static async Task<IResult> CalculateRating(
        [FromBody] RatingCalculationRequest request,
        [FromServices] IReviewService reviewService,
        CancellationToken ct)
    {
        var result = await reviewService.CalculateRatingAsync(request, ct);
        return result.ToIResult();
    }

    // Review moderation
    private static async Task<IResult> GetPendingReviews(
        [AsParameters] PaginationQuery query,
        [FromServices] IReviewService reviewService,
        CancellationToken ct)
    {
        var result = await reviewService.GetPendingReviewsAsync(query, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> ModerateReview(
        Guid reviewId,
        [FromBody] ReviewModerationRequest request,
        [FromServices] IReviewService reviewService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var moderatorId = GetUserId(user);
        var result = await reviewService.ModerateReviewAsync(reviewId, request, moderatorId, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> BulkModerateReviews(
        [FromBody] BulkModerationRequest request,
        [FromServices] IReviewService reviewService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var moderatorId = GetUserId(user);
        var moderationRequest = new ReviewModerationRequest(request.IsApproved, request.ModerationNotes);
        var result = await reviewService.BulkModerateReviewsAsync(request.ReviewIds, moderationRequest, moderatorId, ct);
        return result.ToIResult();
    }

    // Review helpful votes
    private static async Task<IResult> VoteHelpful(
        Guid reviewId,
        [FromBody] ReviewHelpfulRequest request,
        [FromServices] IReviewService reviewService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var result = await reviewService.VoteHelpfulAsync(reviewId, request, userId, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> RemoveHelpfulVote(
        Guid reviewId,
        [FromServices] IReviewService reviewService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var result = await reviewService.RemoveHelpfulVoteAsync(reviewId, userId, ct);
        return result.Success ? Results.NoContent() : Results.BadRequest(result.Error);
    }

    // Review analytics
    private static async Task<IResult> GetReviewAnalytics(
        Guid entityId,
        string entityType,
        [FromServices] IReviewService reviewService,
        CancellationToken ct)
    {
        var result = await reviewService.GetReviewAnalyticsAsync(entityId, entityType, ct);
        return result.Success ? Results.Ok(result.Value) : Results.NotFound(result.Error);
    }

    private static async Task<IResult> SearchReviews(
        [FromBody] ReviewSearchQuery query,
        [FromServices] IReviewService reviewService,
        CancellationToken ct)
    {
        var result = await reviewService.SearchReviewsAsync(query, ct);
        return result.ToIResult();
    }

    // Review validation
    private static async Task<IResult> CanUserReview(
        Guid revieweeId,
        string revieweeType,
        Guid orderId,
        [FromServices] IReviewService reviewService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var result = await reviewService.CanUserReviewAsync(userId, revieweeId, revieweeType, orderId, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> HasUserReviewedOrder(
        Guid orderId,
        [FromServices] IReviewService reviewService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = GetUserId(user);
        var result = await reviewService.HasUserReviewedOrderAsync(userId, orderId, ct);
        return result.ToIResult();
    }

    // Review tags
    private static async Task<IResult> GetAvailableTags(
        string revieweeType,
        [FromServices] IReviewService reviewService,
        CancellationToken ct)
    {
        var result = await reviewService.GetAvailableTagsAsync(revieweeType, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> GetTagFrequencies(
        Guid entityId,
        string entityType,
        [FromServices] IReviewService reviewService,
        CancellationToken ct)
    {
        var result = await reviewService.GetTagFrequenciesAsync(entityId, entityType, ct);
        return result.ToIResult();
    }

    // Helper methods
    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;
    }
}

// Additional DTOs for endpoints
public record BulkModerationRequest(
    List<Guid> ReviewIds,
    bool IsApproved,
    string? ModerationNotes = null);
