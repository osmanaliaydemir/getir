using System.Linq.Expressions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.Reviews;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISignalRService? _signalRService;

    public ReviewService(IUnitOfWork unitOfWork, ISignalRService? signalRService = null)
    {
        _unitOfWork = unitOfWork;
        _signalRService = signalRService;
    }

    public async Task<Result<ReviewResponse>> CreateReviewAsync(
        CreateReviewRequest request, 
        Guid reviewerId, 
        CancellationToken cancellationToken = default)
    {
        // Validate that user can review
        var canReview = await CanUserReviewAsync(reviewerId, request.RevieweeId, request.RevieweeType, request.OrderId, cancellationToken);
        if (!canReview.Success || !canReview.Value)
        {
            return Result.Fail<ReviewResponse>("User cannot review this entity", "CANNOT_REVIEW");
        }

        // Check if user already reviewed this order
        var hasReviewed = await HasUserReviewedOrderAsync(reviewerId, request.OrderId, cancellationToken);
        if (!hasReviewed.Success || hasReviewed.Value)
        {
            return Result.Fail<ReviewResponse>("User has already reviewed this order", "ALREADY_REVIEWED");
        }

        // Validate order exists and is completed
        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.Status == "Delivered", cancellationToken: cancellationToken);
        
        if (order == null)
        {
            return Result.Fail<ReviewResponse>("Order not found or not completed", "ORDER_NOT_FOUND");
        }

        // Create review
        var review = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = reviewerId,
            RevieweeId = request.RevieweeId,
            RevieweeType = request.RevieweeType,
            OrderId = request.OrderId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow,
            IsApproved = true // Auto-approve for now
        };

        await _unitOfWork.Repository<Review>().AddAsync(review, cancellationToken);

        // Add review tags
        if (request.Tags != null && request.Tags.Any())
        {
            foreach (var tag in request.Tags)
            {
                var reviewTag = new ReviewTag
                {
                    Id = Guid.NewGuid(),
                    ReviewId = review.Id,
                    Tag = tag,
                    IsPositive = request.Rating >= 4 // 4-5 stars = positive tags
                };
                await _unitOfWork.Repository<ReviewTag>().AddAsync(reviewTag, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Update rating for the entity
        await UpdateRatingAsync(request.RevieweeId, request.RevieweeType, cancellationToken);

        // Send real-time notification
        if (_signalRService != null)
        {
            var notification = new RealtimeNotificationEvent(
                Guid.NewGuid(),
                "New Review Received",
                $"You received a {request.Rating}-star review",
                "review",
                DateTime.UtcNow,
                false,
                new Dictionary<string, object>
                {
                    ["reviewId"] = review.Id,
                    ["rating"] = request.Rating,
                    ["revieweeType"] = request.RevieweeType
                });

            await _signalRService.SendNotificationToRoleAsync(request.RevieweeType.ToLower(), notification);
        }

        // Get created review with navigation properties
        var createdReview = await GetReviewAsync(review.Id, cancellationToken);
        return createdReview;
    }

    public async Task<Result<ReviewResponse>> UpdateReviewAsync(
        Guid reviewId, 
        UpdateReviewRequest request, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var review = await _unitOfWork.ReadRepository<Review>()
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.ReviewerId == userId, cancellationToken: cancellationToken);

        if (review == null)
        {
            return Result.Fail<ReviewResponse>("Review not found", "NOT_FOUND");
        }

        var oldRating = review.Rating;
        review.Rating = request.Rating;
        review.Comment = request.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        // Update review tags
        var existingTags = await _unitOfWork.ReadRepository<ReviewTag>()
            .ListAsync(rt => rt.ReviewId == reviewId, cancellationToken: cancellationToken);

        foreach (var tag in existingTags)
        {
            await _unitOfWork.Repository<ReviewTag>().DeleteAsync(tag, cancellationToken);
        }

        if (request.Tags != null && request.Tags.Any())
        {
            foreach (var tag in request.Tags)
            {
                var reviewTag = new ReviewTag
                {
                    Id = Guid.NewGuid(),
                    ReviewId = review.Id,
                    Tag = tag,
                    IsPositive = request.Rating >= 4
                };
                await _unitOfWork.Repository<ReviewTag>().AddAsync(reviewTag, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Update rating if rating changed
        if (oldRating != request.Rating)
        {
            await UpdateRatingAsync(review.RevieweeId, review.RevieweeType, cancellationToken);
        }

        return await GetReviewAsync(reviewId, cancellationToken);
    }

    public async Task<Result> DeleteReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default)
    {
        var review = await _unitOfWork.ReadRepository<Review>()
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.ReviewerId == userId, cancellationToken: cancellationToken);

        if (review == null)
        {
            return Result.Fail("Review not found", "NOT_FOUND");
        }

        // Soft delete
        review.IsDeleted = true;
        review.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Update rating
        await UpdateRatingAsync(review.RevieweeId, review.RevieweeType, cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<ReviewResponse>> GetReviewAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _unitOfWork.ReadRepository<Review>()
            .FirstOrDefaultAsync(
                r => r.Id == reviewId && !r.IsDeleted,
                include: "Reviewer,Reviewee,ReviewTags,ReviewHelpfuls",
                cancellationToken: cancellationToken);

        if (review == null)
        {
            return Result.Fail<ReviewResponse>("Review not found", "NOT_FOUND");
        }

        var tags = review.ReviewTags.Select(rt => rt.Tag).ToList();
        var helpfulCount = review.ReviewHelpfuls.Count(rh => rh.IsHelpful);
        
        var response = new ReviewResponse(
            review.Id,
            review.ReviewerId,
            review.Reviewer.FirstName + " " + review.Reviewer.LastName,
            review.RevieweeId,
            review.Reviewee.FirstName + " " + review.Reviewee.LastName,
            review.RevieweeType,
            review.OrderId,
            review.Rating,
            review.Comment,
            tags,
            review.CreatedAt,
            review.UpdatedAt,
            review.IsApproved,
            helpfulCount,
            false // TODO: Check if current user has voted helpful
        );

        return Result.Ok(response);
    }

    public async Task<Result<PagedResult<ReviewResponse>>> GetReviewsAsync(ReviewSearchQuery query, CancellationToken cancellationToken = default)
    {
        var filter = BuildReviewFilter(query);
        
        var reviews = await _unitOfWork.ReadRepository<Review>()
            .GetPagedAsync(
                filter: filter,
                page: query.Page,
                pageSize: query.PageSize,
                orderBy: r => r.CreatedAt,
                ascending: false,
                include: "Reviewer,Reviewee,ReviewTags,ReviewHelpfuls",
                cancellationToken: cancellationToken);
        
        var totalCount = await _unitOfWork.ReadRepository<Review>()
            .CountAsync(filter, cancellationToken);

        var reviewResponses = reviews.Select(r => new ReviewResponse(
            r.Id,
            r.ReviewerId,
            r.Reviewer.FirstName + " " + r.Reviewer.LastName,
            r.RevieweeId,
            r.Reviewee.FirstName + " " + r.Reviewee.LastName,
            r.RevieweeType,
            r.OrderId,
            r.Rating,
            r.Comment,
            r.ReviewTags.Select(rt => rt.Tag).ToList(),
            r.CreatedAt,
            r.UpdatedAt,
            r.IsApproved,
            r.ReviewHelpfuls.Count(rh => rh.IsHelpful),
            false // TODO: Check if current user has voted helpful
        )).ToList();

        var pagedResult = PagedResult<ReviewResponse>.Create(
            reviewResponses,
            totalCount,
            query.Page,
            query.PageSize);

        return Result.Ok(pagedResult);
    }

    public async Task<Result<PagedResult<ReviewResponse>>> GetReviewsByRevieweeAsync(Guid revieweeId, string revieweeType, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var searchQuery = new ReviewSearchQuery
        {
            RevieweeId = revieweeId,
            RevieweeType = revieweeType,
            IsApproved = true,
            Page = query.Page,
            PageSize = query.PageSize
        };

        return await GetReviewsAsync(searchQuery, cancellationToken);
    }

    public async Task<Result<PagedResult<ReviewResponse>>> GetReviewsByReviewerAsync(Guid reviewerId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var searchQuery = new ReviewSearchQuery
        {
            RevieweeId = reviewerId, // This should be ReviewerId, but we need to adjust the filter
            IsApproved = true,
            Page = query.Page,
            PageSize = query.PageSize
        };

        // Adjust filter for reviewer
        Expression<Func<Review, bool>> filter = r => r.ReviewerId == reviewerId && !r.IsDeleted && r.IsApproved;

        var reviews = await _unitOfWork.ReadRepository<Review>()
            .GetPagedAsync(
                filter: filter,
                page: query.Page,
                pageSize: query.PageSize,
                orderBy: r => r.CreatedAt,
                ascending: false,
                include: "Reviewer,Reviewee,ReviewTags,ReviewHelpfuls",
                cancellationToken: cancellationToken);
        
        var totalCount = await _unitOfWork.ReadRepository<Review>()
            .CountAsync(filter, cancellationToken);

        var reviewResponses = reviews.Select(r => new ReviewResponse(
            r.Id,
            r.ReviewerId,
            r.Reviewer.FirstName + " " + r.Reviewer.LastName,
            r.RevieweeId,
            r.Reviewee.FirstName + " " + r.Reviewee.LastName,
            r.RevieweeType,
            r.OrderId,
            r.Rating,
            r.Comment,
            r.ReviewTags.Select(rt => rt.Tag).ToList(),
            r.CreatedAt,
            r.UpdatedAt,
            r.IsApproved,
            r.ReviewHelpfuls.Count(rh => rh.IsHelpful),
            false
        )).ToList();

        var pagedResult = PagedResult<ReviewResponse>.Create(
            reviewResponses,
            totalCount,
            query.Page,
            query.PageSize);

        return Result.Ok(pagedResult);
    }

    public async Task<Result<PagedResult<ReviewResponse>>> GetReviewsByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var searchQuery = new ReviewSearchQuery
        {
            RevieweeId = orderId, // This should be OrderId, need to adjust
            IsApproved = true,
            Page = 1,
            PageSize = 10
        };

        // Adjust filter for order
        Expression<Func<Review, bool>> filter = r => r.OrderId == orderId && !r.IsDeleted && r.IsApproved;

        var reviews = await _unitOfWork.ReadRepository<Review>()
            .GetPagedAsync(
                filter: filter,
                page: 1,
                pageSize: 10,
                orderBy: r => r.CreatedAt,
                ascending: false,
                include: "Reviewer,Reviewee,ReviewTags,ReviewHelpfuls",
                cancellationToken: cancellationToken);
        
        var totalCount = await _unitOfWork.ReadRepository<Review>()
            .CountAsync(filter, cancellationToken);

        var reviewResponses = reviews.Select(r => new ReviewResponse(
            r.Id,
            r.ReviewerId,
            r.Reviewer.FirstName + " " + r.Reviewer.LastName,
            r.RevieweeId,
            r.Reviewee.FirstName + " " + r.Reviewee.LastName,
            r.RevieweeType,
            r.OrderId,
            r.Rating,
            r.Comment,
            r.ReviewTags.Select(rt => rt.Tag).ToList(),
            r.CreatedAt,
            r.UpdatedAt,
            r.IsApproved,
            r.ReviewHelpfuls.Count(rh => rh.IsHelpful),
            false
        )).ToList();

        var pagedResult = PagedResult<ReviewResponse>.Create(
            reviewResponses,
            totalCount,
            1,
            10);

        return Result.Ok(pagedResult);
    }

    public async Task<Result<RatingResponse>> GetRatingAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default)
    {
        var rating = await _unitOfWork.ReadRepository<Rating>()
            .FirstOrDefaultAsync(r => r.EntityId == entityId && r.EntityType == entityType, cancellationToken: cancellationToken);

        if (rating == null)
        {
            // Create default rating if not exists
            rating = new Rating
            {
                Id = Guid.NewGuid(),
                EntityId = entityId,
                EntityType = entityType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<Rating>().AddAsync(rating, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var breakdown = new RatingBreakdownResponse(
            rating.FiveStarCount,
            rating.FourStarCount,
            rating.ThreeStarCount,
            rating.TwoStarCount,
            rating.OneStarCount);

        var recentPerformance = new RecentPerformanceResponse(
            rating.RecentAverageRating,
            rating.RecentReviewCount,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow);

        var qualityMetrics = new QualityMetricsResponse(
            rating.ResponseRate,
            rating.PositiveReviewRate,
            0); // TODO: Calculate total orders

        var response = new RatingResponse(
            rating.EntityId,
            rating.EntityType,
            rating.AverageRating,
            rating.TotalReviews,
            breakdown,
            recentPerformance,
            qualityMetrics,
            rating.UpdatedAt);

        return Result.Ok(response);
    }

    public async Task<Result<ReviewStatsResponse>> GetReviewStatsAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default)
    {
        var rating = await _unitOfWork.ReadRepository<Rating>()
            .FirstOrDefaultAsync(r => r.EntityId == entityId && r.EntityType == entityType, cancellationToken: cancellationToken);

        if (rating == null)
        {
            return Result.Fail<ReviewStatsResponse>("Rating not found", "NOT_FOUND");
        }

        var stats = new ReviewStatsResponse(
            rating.AverageRating,
            rating.TotalReviews,
            rating.FiveStarCount,
            rating.FourStarCount,
            rating.ThreeStarCount,
            rating.TwoStarCount,
            rating.OneStarCount,
            rating.RecentAverageRating,
            rating.RecentReviewCount,
            rating.PositiveReviewRate);

        return Result.Ok(stats);
    }

    public async Task<Result<RatingCalculationResponse>> CalculateRatingAsync(RatingCalculationRequest request, CancellationToken cancellationToken = default)
    {
        var fromDate = request.FromDate ?? DateTime.UtcNow.AddYears(-1);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        var reviews = await _unitOfWork.ReadRepository<Review>()
            .ListAsync(
                r => r.RevieweeId == request.EntityId && 
                     r.RevieweeType == request.EntityType && 
                     !r.IsDeleted && 
                     r.IsApproved &&
                     r.CreatedAt >= fromDate &&
                     r.CreatedAt <= toDate,
                cancellationToken: cancellationToken);

        if (!reviews.Any())
        {
            var emptyBreakdown = new RatingBreakdownResponse(0, 0, 0, 0, 0);
            return Result.Ok(new RatingCalculationResponse(
                request.EntityId,
                request.EntityType,
                0m,
                0,
                emptyBreakdown,
                DateTime.UtcNow));
        }

            var averageRating = (decimal)reviews.Average(r => r.Rating);
        var totalReviews = reviews.Count;
        
        var breakdown = new RatingBreakdownResponse(
            reviews.Count(r => r.Rating == 5),
            reviews.Count(r => r.Rating == 4),
            reviews.Count(r => r.Rating == 3),
            reviews.Count(r => r.Rating == 2),
            reviews.Count(r => r.Rating == 1));

        var response = new RatingCalculationResponse(
            request.EntityId,
            request.EntityType,
            Math.Round(averageRating, 2),
            totalReviews,
            breakdown,
            DateTime.UtcNow);

        return Result.Ok(response);
    }

    public async Task<Result> UpdateRatingAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default)
    {
        var reviews = await _unitOfWork.ReadRepository<Review>()
            .ListAsync(
                r => r.RevieweeId == entityId && r.RevieweeType == entityType && !r.IsDeleted && r.IsApproved,
                cancellationToken: cancellationToken);

        var rating = await _unitOfWork.ReadRepository<Rating>()
            .FirstOrDefaultAsync(r => r.EntityId == entityId && r.EntityType == entityType, cancellationToken: cancellationToken);

        if (rating == null)
        {
            rating = new Rating
            {
                Id = Guid.NewGuid(),
                EntityId = entityId,
                EntityType = entityType,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<Rating>().AddAsync(rating, cancellationToken);
        }

        if (reviews.Any())
        {
            rating.AverageRating = Math.Round((decimal)reviews.Average(r => r.Rating), 2);
            rating.TotalReviews = reviews.Count;
            rating.FiveStarCount = reviews.Count(r => r.Rating == 5);
            rating.FourStarCount = reviews.Count(r => r.Rating == 4);
            rating.ThreeStarCount = reviews.Count(r => r.Rating == 3);
            rating.TwoStarCount = reviews.Count(r => r.Rating == 2);
            rating.OneStarCount = reviews.Count(r => r.Rating == 1);
            rating.LastReviewDate = reviews.Max(r => r.CreatedAt);

            // Calculate recent performance (last 30 days)
            var recentReviews = reviews.Where(r => r.CreatedAt >= DateTime.UtcNow.AddDays(-30)).ToList();
            rating.RecentReviewCount = recentReviews.Count;
            rating.RecentAverageRating = recentReviews.Any() ? Math.Round((decimal)recentReviews.Average(r => r.Rating), 2) : 0m;

            // Calculate positive review rate
            var positiveReviews = reviews.Count(r => r.Rating >= 4);
            rating.PositiveReviewRate = Math.Round((decimal)positiveReviews / reviews.Count * 100, 2);
        }
        else
        {
            // Reset all values if no reviews
            rating.AverageRating = 0m;
            rating.TotalReviews = 0;
            rating.FiveStarCount = 0;
            rating.FourStarCount = 0;
            rating.ThreeStarCount = 0;
            rating.TwoStarCount = 0;
            rating.OneStarCount = 0;
            rating.LastReviewDate = null;
            rating.RecentReviewCount = 0;
            rating.RecentAverageRating = 0m;
            rating.PositiveReviewRate = 0m;
        }

        rating.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    // Helper methods
    private Expression<Func<Review, bool>> BuildReviewFilter(ReviewSearchQuery query)
    {
        return r => !r.IsDeleted &&
                    (!query.RevieweeId.HasValue || r.RevieweeId == query.RevieweeId.Value) &&
                    (string.IsNullOrEmpty(query.RevieweeType) || r.RevieweeType == query.RevieweeType) &&
                    (!query.MinRating.HasValue || r.Rating >= query.MinRating.Value) &&
                    (!query.MaxRating.HasValue || r.Rating <= query.MaxRating.Value) &&
                    (!query.IsApproved.HasValue || r.IsApproved == query.IsApproved.Value) &&
                    (!query.FromDate.HasValue || r.CreatedAt >= query.FromDate.Value) &&
                    (!query.ToDate.HasValue || r.CreatedAt <= query.ToDate.Value);
    }

    // Placeholder methods for remaining interface members
    public async Task<Result<PagedResult<PendingReviewResponse>>> GetPendingReviewsAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        // TODO: Implement pending reviews
        return Result.Ok(PagedResult<PendingReviewResponse>.Create(new List<PendingReviewResponse>(), 0, 1, 10));
    }

    public async Task<Result<ReviewModerationResponse>> ModerateReviewAsync(Guid reviewId, ReviewModerationRequest request, Guid moderatorId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement review moderation
        return Result.Fail<ReviewModerationResponse>("Not implemented", "NOT_IMPLEMENTED");
    }

    public async Task<Result> BulkModerateReviewsAsync(List<Guid> reviewIds, ReviewModerationRequest request, Guid moderatorId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement bulk moderation
        return Result.Fail("Not implemented", "NOT_IMPLEMENTED");
    }

    public async Task<Result<ReviewHelpfulResponse>> VoteHelpfulAsync(Guid reviewId, ReviewHelpfulRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement helpful votes
        return Result.Fail<ReviewHelpfulResponse>("Not implemented", "NOT_IMPLEMENTED");
    }

    public async Task<Result> RemoveHelpfulVoteAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement remove helpful vote
        return Result.Fail("Not implemented", "NOT_IMPLEMENTED");
    }

    public async Task<Result<ReviewAnalyticsResponse>> GetReviewAnalyticsAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default)
    {
        // TODO: Implement review analytics
        return Result.Fail<ReviewAnalyticsResponse>("Not implemented", "NOT_IMPLEMENTED");
    }

    public async Task<Result<ReviewSearchResponse>> SearchReviewsAsync(ReviewSearchQuery query, CancellationToken cancellationToken = default)
    {
        // TODO: Implement advanced search
        return Result.Fail<ReviewSearchResponse>("Not implemented", "NOT_IMPLEMENTED");
    }

    public async Task<Result<bool>> CanUserReviewAsync(Guid userId, Guid revieweeId, string revieweeType, Guid orderId, CancellationToken cancellationToken = default)
    {
        // Check if user has completed order
        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(
                o => o.Id == orderId && 
                     o.UserId == userId && 
                     o.Status == "Delivered",
                cancellationToken: cancellationToken);

        return Result.Ok(order != null);
    }

    public async Task<Result<bool>> HasUserReviewedOrderAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var review = await _unitOfWork.ReadRepository<Review>()
            .FirstOrDefaultAsync(
                r => r.ReviewerId == userId && 
                     r.OrderId == orderId && 
                     !r.IsDeleted,
                cancellationToken: cancellationToken);

        return Result.Ok(review != null);
    }

    public async Task<Result<List<string>>> GetAvailableTagsAsync(string revieweeType, CancellationToken cancellationToken = default)
    {
        // Return predefined tags based on reviewee type
        var tags = revieweeType.ToLower() switch
        {
            "merchant" => new List<string> { "Food Quality", "Delivery Speed", "Service", "Packaging", "Value for Money", "Taste", "Freshness", "Portion Size" },
            "courier" => new List<string> { "Delivery Speed", "Service", "Communication", "Care", "Punctuality", "Friendliness", "Professionalism" },
            _ => new List<string>()
        };

        return Result.Ok(tags);
    }

    public async Task<Result<List<TagFrequencyResponse>>> GetTagFrequenciesAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default)
    {
        // TODO: Implement tag frequency analysis
        return Result.Ok(new List<TagFrequencyResponse>());
    }
}
