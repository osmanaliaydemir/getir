using System.Linq.Expressions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Reviews;

/// <summary>
/// Review servisi implementasyonu: değerlendirme yönetimi, rating hesaplama, moderasyon, SignalR entegrasyonu.
/// </summary>
public class ReviewService : BaseService, IReviewService
{
    private readonly ISignalRService? _signalRService;
    private readonly IBackgroundTaskService _backgroundTaskService;
    public ReviewService(IUnitOfWork unitOfWork, ILogger<ReviewService> logger, ILoggingService loggingService, ICacheService cacheService, IBackgroundTaskService backgroundTaskService, ISignalRService? signalRService = null)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _signalRService = signalRService;
        _backgroundTaskService = backgroundTaskService;
    }
    /// <summary>
    /// Yeni değerlendirme oluşturur (validasyon, duplicate kontrolü, tag ekleme, rating güncelleme, SignalR bildirim).
    /// </summary>
    public async Task<Result<ReviewResponse>> CreateReviewAsync(CreateReviewRequest request, Guid reviewerId, CancellationToken cancellationToken = default)
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
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.Status == OrderStatus.Delivered, cancellationToken: cancellationToken);

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

        // ============= CACHE INVALIDATION =============
        // Invalidate review lists for this entity
        await _cacheService.RemoveByPatternAsync(
            CacheKeys.AllReviewsByEntity(request.RevieweeType, request.RevieweeId),
            cancellationToken);

        // Invalidate rating cache
        await _cacheService.RemoveAsync(
            CacheKeys.RatingStats(request.RevieweeType, request.RevieweeId),
            cancellationToken);

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
    /// <summary>
    /// Değerlendirmeyi günceller (ownership kontrolü, tag yönetimi, rating güncelleme, cache invalidation).
    /// </summary>
    public async Task<Result<ReviewResponse>> UpdateReviewAsync(Guid reviewId, UpdateReviewRequest request, Guid userId, CancellationToken cancellationToken = default)
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

        // ============= CACHE INVALIDATION =============
        // Invalidate review lists for this entity
        await _cacheService.RemoveByPatternAsync(
            CacheKeys.AllReviewsByEntity(review.RevieweeType, review.RevieweeId),
            cancellationToken);

        // Invalidate rating cache
        await _cacheService.RemoveAsync(
            CacheKeys.RatingStats(review.RevieweeType, review.RevieweeId),
            cancellationToken);

        return await GetReviewAsync(reviewId, cancellationToken);
    }
    /// <summary>
    /// Değerlendirmeyi siler (ownership kontrolü, soft delete, rating güncelleme, cache invalidation).
    /// </summary>
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

        // ============= CACHE INVALIDATION =============
        // Invalidate review lists for this entity
        await _cacheService.RemoveByPatternAsync(
            CacheKeys.AllReviewsByEntity(review.RevieweeType, review.RevieweeId),
            cancellationToken);

        // Invalidate rating cache
        await _cacheService.RemoveAsync(
            CacheKeys.RatingStats(review.RevieweeType, review.RevieweeId),
            cancellationToken);

        return Result.Ok();
    }
    /// <summary>
    /// Değerlendirmeyi ID ile getirir (tag/helpful count dahil).
    /// </summary>
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
            review.RevieweeType,
            review.OrderId,
            review.Rating,
            review.Comment,
            review.IsApproved,
            review.CreatedAt,
            review.UpdatedAt,
            tags,
            false, // TODO: Check if current user has voted helpful
            helpfulCount,
            0
        );

        return Result.Ok(response);
    }
    /// <summary>
    /// Değerlendirmeleri arar (filtreleme ile, sayfalama).
    /// </summary>
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
            r.RevieweeType,
            r.OrderId,
            r.Rating,
            r.Comment,
            r.IsApproved,
            r.CreatedAt,
            r.UpdatedAt,
            r.ReviewTags.Select(rt => rt.Tag).ToList(),
            false, // TODO: Check if current user has voted helpful
            r.ReviewHelpfuls.Count(rh => rh.IsHelpful),
            0
        )).ToList();

        var pagedResult = PagedResult<ReviewResponse>.Create(
            reviewResponses,
            totalCount,
            query.Page,
            query.PageSize);

        return Result.Ok(pagedResult);
    }
    /// <summary>
    /// Entity için değerlendirmeleri getirir (cache, performance tracking).
    /// </summary>
    public async Task<Result<PagedResult<ReviewResponse>>> GetReviewsByRevieweeAsync(Guid revieweeId, string revieweeType, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetReviewsByRevieweeInternalAsync(revieweeId, revieweeType, query, cancellationToken),
            "GetReviewsByReviewee",
            new { RevieweeId = revieweeId, RevieweeType = revieweeType, Page = query.Page },
            cancellationToken);
    }
    
    private async Task<Result<PagedResult<ReviewResponse>>> GetReviewsByRevieweeInternalAsync(Guid revieweeId, string revieweeType, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.ReviewsByEntity(revieweeType, revieweeId, query.Page, query.PageSize);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var searchQuery = new ReviewSearchQuery
                    {
                        EntityId = revieweeId,
                        EntityType = revieweeType,
                        Page = query.Page,
                        PageSize = query.PageSize
                    };

                    return await GetReviewsAsync(searchQuery, cancellationToken);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.Medium), // 15 minutes TTL for reviews
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews by reviewee: {RevieweeId}, {RevieweeType}", revieweeId, revieweeType);
            return ServiceResult.HandleException<PagedResult<ReviewResponse>>(ex, _logger, "GetReviewsByReviewee");
        }
    }
    /// <summary>
    /// Kullanıcının yaptığı değerlendirmeleri getirir.
    /// </summary>
    public async Task<Result<PagedResult<ReviewResponse>>> GetReviewsByReviewerAsync(Guid reviewerId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
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
            r.RevieweeType,
            r.OrderId,
            r.Rating,
            r.Comment,
            r.IsApproved,
            r.CreatedAt,
            r.UpdatedAt,
            r.ReviewTags.Select(rt => rt.Tag).ToList(),
            false,
            r.ReviewHelpfuls.Count(rh => rh.IsHelpful),
            0
        )).ToList();

        var pagedResult = PagedResult<ReviewResponse>.Create(
            reviewResponses,
            totalCount,
            query.Page,
            query.PageSize);

        return Result.Ok(pagedResult);
    }
    /// <summary>
    /// Sipariş için değerlendirmeleri getirir.
    /// </summary>
    public async Task<Result<PagedResult<ReviewResponse>>> GetReviewsByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
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
            r.RevieweeType,
            r.OrderId,
            r.Rating,
            r.Comment,
            r.IsApproved,
            r.CreatedAt,
            r.UpdatedAt,
            r.ReviewTags.Select(rt => rt.Tag).ToList(),
            false,
            r.ReviewHelpfuls.Count(rh => rh.IsHelpful),
            0
        )).ToList();

        var pagedResult = PagedResult<ReviewResponse>.Create(
            reviewResponses,
            totalCount,
            1,
            10);

        return Result.Ok(pagedResult);
    }
    /// <summary>
    /// Entity rating'ini getirir (ortalama, breakdown, cache, yoksa oluşturur).
    /// </summary>
    public async Task<Result<RatingResponse>> GetRatingAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetRatingInternalAsync(entityId, entityType, cancellationToken),
            "GetRating",
            new { EntityId = entityId, EntityType = entityType },
            cancellationToken);
    }
    
    private async Task<Result<RatingResponse>> GetRatingInternalAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.RatingStats(entityType, entityId);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
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
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.Medium), // 15 minutes TTL for ratings
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rating: {EntityId}, {EntityType}", entityId, entityType);
            return ServiceResult.HandleException<RatingResponse>(ex, _logger, "GetRating");
        }
    }
    /// <summary>
    /// Review istatistiklerini getirir (ortalama, breakdown, son performans).
    /// </summary>
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
    /// <summary>
    /// Rating hesaplar (tarih aralığı ile, ortalama ve breakdown).
    /// </summary>
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
    /// <summary>
    /// Entity rating'ini günceller (ortalama, breakdown, son 30 gün performansı, pozitif oran).
    /// </summary>
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
                    (!query.EntityId.HasValue || r.RevieweeId == query.EntityId.Value) &&
                    (string.IsNullOrEmpty(query.EntityType) || r.RevieweeType == query.EntityType) &&
                    (!query.MinRating.HasValue || r.Rating >= query.MinRating.Value) &&
                    (!query.MaxRating.HasValue || r.Rating <= query.MaxRating.Value) &&
                    (!query.DateFrom.HasValue || r.CreatedAt >= query.DateFrom.Value) &&
                    (!query.DateTo.HasValue || r.CreatedAt <= query.DateTo.Value);
    }
    
    /// <summary>
    /// Moderasyon bekleyen değerlendirmeleri getirir.
    /// </summary>
    public async Task<Result<PagedResult<PendingReviewResponse>>> GetPendingReviewsAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get pending reviews (not moderated yet)
            var pendingReviews = await _unitOfWork.ReadRepository<Review>()
                .GetPagedAsync(
                    r => !r.IsModerated,
                    r => r.CreatedAt,
                    false, // descending order
                    query.Page,
                    query.PageSize,
                    "Reviewer,Reviewee",
                    cancellationToken);

            var totalCount = await _unitOfWork.ReadRepository<Review>()
                .CountAsync(r => !r.IsModerated, cancellationToken);

            var responses = pendingReviews.Select(r => new PendingReviewResponse(
                r.Id,
                "", // Reviewer name will be loaded separately
                "", // Reviewee name will be loaded separately
                r.RevieweeType,
                r.Rating,
                r.Comment,
                new List<string>(), // Tags will be loaded separately
                r.CreatedAt,
                r.IsApproved
            )).ToList();

            var pagedResult = PagedResult<PendingReviewResponse>.Create(
                responses,
                totalCount,
                query.Page,
                query.PageSize);

            return Result.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending reviews");
            return Result.Fail<PagedResult<PendingReviewResponse>>("Failed to get pending reviews", "GET_PENDING_REVIEWS_ERROR");
        }
    }
    /// <summary>
    /// Değerlendirmeyi modere eder (approve/reject, moderation log, SignalR bildirim).
    /// </summary>
    public async Task<Result<ReviewModerationResponse>> ModerateReviewAsync(Guid reviewId, ReviewModerationRequest request, Guid moderatorId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get review
            var review = await _unitOfWork.ReadRepository<Review>()
                .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken: cancellationToken);

            if (review == null)
            {
                return Result.Fail<ReviewModerationResponse>("Review not found", "REVIEW_NOT_FOUND");
            }

            // Update review status
            review.IsApproved = request.Action == "approve";
            review.ModeratedAt = DateTime.UtcNow;
            review.ModeratedBy = moderatorId;
            review.ModeratorNotes = request.Notes;

            _unitOfWork.Repository<Review>().Update(review);

            // Create moderation log
            var moderationLog = new ReviewModerationLog
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                ModeratorId = moderatorId,
                Action = request.Action,
                Notes = request.Notes,
                ModeratedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ReviewModerationLog>().AddAsync(moderationLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notification to reviewer
            if (_signalRService != null)
            {
                var notification = new RealtimeNotificationEvent(
                    Guid.NewGuid(),
                    "Review Moderated",
                    $"Your review has been {request.Action}d",
                    "review_moderation",
                    DateTime.UtcNow,
                    false,
                    new Dictionary<string, object> { { "ReviewId", reviewId }, { "Action", request.Action } }
                );

                await _signalRService.SendRealtimeNotificationAsync(notification);
            }

            var response = new ReviewModerationResponse(
                reviewId,
                request.Action,
                request.Notes,
                moderatorId,
                DateTime.UtcNow
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moderating review {ReviewId}", reviewId);
            return Result.Fail<ReviewModerationResponse>("Failed to moderate review", "MODERATION_ERROR");
        }
    }
    /// <summary>
    /// Toplu moderasyon (hata yönetimi, başarı/hata sayısı).
    /// </summary>
    public async Task<Result> BulkModerateReviewsAsync(List<Guid> reviewIds, ReviewModerationRequest request, Guid moderatorId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (reviewIds == null || !reviewIds.Any())
            {
                return Result.Fail("No review IDs provided", "NO_REVIEW_IDS");
            }

            var successCount = 0;
            var failureCount = 0;
            var errors = new List<string>();

            foreach (var reviewId in reviewIds)
            {
                try
                {
                    var result = await ModerateReviewAsync(reviewId, request, moderatorId, cancellationToken);
                    if (result.Success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failureCount++;
                        errors.Add($"Review {reviewId}: {result.Error}");
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    errors.Add($"Review {reviewId}: {ex.Message}");
                    _logger.LogError(ex, "Error in bulk moderation for review {ReviewId}", reviewId);
                }
            }

            _logger.LogInformation("Bulk moderation completed. Success: {SuccessCount}, Failures: {FailureCount}",
                successCount, failureCount);

            if (failureCount > 0)
            {
                return Result.Fail($"Bulk moderation completed with {failureCount} failures. Errors: {string.Join("; ", errors)}",
                    "BULK_MODERATION_PARTIAL_FAILURE");
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk moderation");
            return Result.Fail("Failed to perform bulk moderation", "BULK_MODERATION_ERROR");
        }
    }
    /// <summary>
    /// Faydalı/faydasız oy verir (duplicate kontrolü, count güncelleme).
    /// </summary>
    public async Task<Result<ReviewHelpfulResponse>> VoteHelpfulAsync(Guid reviewId, ReviewHelpfulRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if review exists
            var review = await _unitOfWork.ReadRepository<Review>()
                .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken: cancellationToken);

            if (review == null)
            {
                return Result.Fail<ReviewHelpfulResponse>("Review not found", "REVIEW_NOT_FOUND");
            }

            // Check if user already voted
            var existingVote = await _unitOfWork.ReadRepository<ReviewHelpful>()
                .FirstOrDefaultAsync(rh => rh.ReviewId == reviewId && rh.UserId == userId, cancellationToken: cancellationToken);

            if (existingVote != null)
            {
                return Result.Fail<ReviewHelpfulResponse>("User has already voted on this review", "ALREADY_VOTED");
            }

            // Create helpful vote
            var helpfulVote = new ReviewHelpful
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                UserId = userId,
                IsHelpful = request.IsHelpful,
                VotedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ReviewHelpful>().AddAsync(helpfulVote, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update review helpful count
            var helpfulCount = await _unitOfWork.ReadRepository<ReviewHelpful>()
                .CountAsync(rh => rh.ReviewId == reviewId && rh.IsHelpful, cancellationToken);

            var notHelpfulCount = await _unitOfWork.ReadRepository<ReviewHelpful>()
                .CountAsync(rh => rh.ReviewId == reviewId && !rh.IsHelpful, cancellationToken);

            var response = new ReviewHelpfulResponse(
                reviewId,
                userId,
                request.IsHelpful,
                helpfulCount,
                notHelpfulCount,
                DateTime.UtcNow
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voting helpful for review {ReviewId}", reviewId);
            return Result.Fail<ReviewHelpfulResponse>("Failed to vote helpful", "VOTE_HELPFUL_ERROR");
        }
    }
    /// <summary>
    /// Faydalı oyunu geri alır.
    /// </summary>
    public async Task<Result> RemoveHelpfulVoteAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find existing vote
            var existingVote = await _unitOfWork.ReadRepository<ReviewHelpful>()
                .FirstOrDefaultAsync(rh => rh.ReviewId == reviewId && rh.UserId == userId, cancellationToken: cancellationToken);

            if (existingVote == null)
            {
                return Result.Fail("Vote not found", "VOTE_NOT_FOUND");
            }

            // Remove the vote
            _unitOfWork.Repository<ReviewHelpful>().Delete(existingVote);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Removed helpful vote for review {ReviewId} by user {UserId}", reviewId, userId);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing helpful vote for review {ReviewId}", reviewId);
            return Result.Fail("Failed to remove helpful vote", "REMOVE_VOTE_ERROR");
        }
    }
    /// <summary>
    /// Review analizlerini getirir (rating dağılımı, faydalı oy sayıları, en yaygın tag'ler).
    /// </summary>
    public async Task<Result<ReviewAnalyticsResponse>> GetReviewAnalyticsAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all reviews for the entity
            var reviews = await _unitOfWork.ReadRepository<Review>()
                .ListAsync(r => r.RevieweeId == entityId && r.RevieweeType == entityType && r.IsApproved,
                    cancellationToken: cancellationToken);

            if (!reviews.Any())
            {
                return Result.Ok(new ReviewAnalyticsResponse(
                    entityId,
                    entityType,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    new Dictionary<int, int>(),
                    new List<string>(),
                    DateTime.UtcNow
                ));
            }

            // Calculate analytics
            var totalReviews = reviews.Count;
            var averageRating = reviews.Average(r => r.Rating);
            var ratingDistribution = reviews.GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            var helpfulVotes = await _unitOfWork.ReadRepository<ReviewHelpful>()
                .CountAsync(rh => reviews.Select(r => r.Id).Contains(rh.ReviewId) && rh.IsHelpful,
                    cancellationToken: cancellationToken);

            var totalVotes = await _unitOfWork.ReadRepository<ReviewHelpful>()
                .CountAsync(rh => reviews.Select(r => r.Id).Contains(rh.ReviewId),
                    cancellationToken: cancellationToken);

            // Get most common tags
            var reviewIds = reviews.Select(r => r.Id).ToList();
            var tags = await _unitOfWork.ReadRepository<ReviewTag>()
                .ListAsync(rt => reviewIds.Contains(rt.ReviewId), cancellationToken: cancellationToken);

            var topTags = tags.GroupBy(t => t.Tag)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();

            var response = new ReviewAnalyticsResponse(
                entityId,
                entityType,
                totalReviews,
                (decimal)Math.Round(averageRating, 2),
                helpfulVotes,
                totalVotes,
                ratingDistribution.GetValueOrDefault(5, 0),
                ratingDistribution.GetValueOrDefault(1, 0),
                ratingDistribution,
                topTags,
                DateTime.UtcNow
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review analytics for entity {EntityId}", entityId);
            return Result.Fail<ReviewAnalyticsResponse>("Failed to get review analytics", "ANALYTICS_ERROR");
        }
    }
    /// <summary>
    /// Değerlendirmeleri detaylı arar (filtreleme, sıralama, sayfalama).
    /// </summary>
    public async Task<Result<ReviewSearchResponse>> SearchReviewsAsync(ReviewSearchQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all approved reviews first
            var allReviews = await _unitOfWork.ReadRepository<Review>()
                .ListAsync(r => r.IsApproved, cancellationToken: cancellationToken);

            // Apply filters
            var filteredReviews = allReviews.AsQueryable();

            if (query.EntityId.HasValue)
            {
                filteredReviews = filteredReviews.Where(r => r.RevieweeId == query.EntityId.Value);
            }

            if (!string.IsNullOrEmpty(query.EntityType))
            {
                filteredReviews = filteredReviews.Where(r => r.RevieweeType == query.EntityType);
            }

            if (query.MinRating.HasValue)
            {
                filteredReviews = filteredReviews.Where(r => r.Rating >= query.MinRating.Value);
            }

            if (query.MaxRating.HasValue)
            {
                filteredReviews = filteredReviews.Where(r => r.Rating <= query.MaxRating.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                filteredReviews = filteredReviews.Where(r => r.Comment.Contains(query.SearchTerm));
            }

            if (query.DateFrom.HasValue)
            {
                filteredReviews = filteredReviews.Where(r => r.CreatedAt >= query.DateFrom.Value);
            }

            if (query.DateTo.HasValue)
            {
                filteredReviews = filteredReviews.Where(r => r.CreatedAt <= query.DateTo.Value);
            }

            // Apply sorting
            filteredReviews = query.SortBy?.ToLower() switch
            {
                "rating" => query.SortDescending ? filteredReviews.OrderByDescending(r => r.Rating) : filteredReviews.OrderBy(r => r.Rating),
                "date" => query.SortDescending ? filteredReviews.OrderByDescending(r => r.CreatedAt) : filteredReviews.OrderBy(r => r.CreatedAt),
                _ => filteredReviews.OrderByDescending(r => r.CreatedAt)
            };

            var reviewsList = filteredReviews.ToList();
            var totalCount = reviewsList.Count;

            // Apply pagination
            var pagedReviews = reviewsList
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            // Map to response
            var reviewResponses = pagedReviews.Select(r => new ReviewResponse(
                r.Id,
                r.ReviewerId,
                "", // Reviewer name will be loaded separately
                r.RevieweeId,
                r.RevieweeType,
                r.OrderId,
                r.Rating,
                r.Comment,
                r.IsApproved,
                r.CreatedAt,
                r.UpdatedAt,
                new List<string>(), // Tags will be loaded separately
                false, // HasVoted will be calculated separately
                0, // HelpfulCount will be calculated separately
                0 // TotalOrders will be calculated separately
            )).ToList();

            var response = new ReviewSearchResponse(
                reviewResponses,
                totalCount,
                query.Page,
                query.PageSize,
                (int)Math.Ceiling((double)totalCount / query.PageSize),
                query.SearchTerm,
                null // Filters will be implemented later
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching reviews");
            return Result.Fail<ReviewSearchResponse>("Failed to search reviews", "SEARCH_ERROR");
        }
    }
    /// <summary>
    /// Kullanıcının değerlendirme yapıp yapamayacağını kontrol eder (order tamamlanma kontrolü).
    /// </summary>
    public async Task<Result<bool>> CanUserReviewAsync(Guid userId, Guid revieweeId, string revieweeType, Guid orderId, CancellationToken cancellationToken = default)
    {
        // Check if user has completed order
        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(
                o => o.Id == orderId &&
                     o.UserId == userId &&
                     o.Status == OrderStatus.Delivered,
                cancellationToken: cancellationToken);

        return Result.Ok(order != null);
    }
    /// <summary>
    /// Kullanıcının order'ı daha önce değerlendirip değerlendirmediğini kontrol eder.
    /// </summary>
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
    /// <summary>
    /// Entity tipi için mevcut tag'leri getirir (merchant/courier için önceden tanımlı).
    /// </summary>
    public Task<Result<List<string>>> GetAvailableTagsAsync(string revieweeType, CancellationToken cancellationToken = default)
    {
        // Return predefined tags based on reviewee type
        var tags = revieweeType.ToLower() switch
        {
            "merchant" => new List<string> { "Food Quality", "Delivery Speed", "Service", "Packaging", "Value for Money", "Taste", "Freshness", "Portion Size" },
            "courier" => new List<string> { "Delivery Speed", "Service", "Communication", "Care", "Punctuality", "Friendliness", "Professionalism" },
            _ => new List<string>()
        };

        return Task.FromResult(Result.Ok(tags));
    }
    /// <summary>
    /// Tag frekanslarını getirir (en çok kullanılan tag'ler, yüzde hesaplaması).
    /// </summary>
    public async Task<Result<List<TagFrequencyResponse>>> GetTagFrequenciesAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all reviews for the entity
            var reviews = await _unitOfWork.ReadRepository<Review>()
                .ListAsync(r => r.RevieweeId == entityId && r.RevieweeType == entityType && r.IsApproved,
                    cancellationToken: cancellationToken);

            if (!reviews.Any())
            {
                return Result.Ok(new List<TagFrequencyResponse>());
            }

            // Get all tags for these reviews
            var reviewIds = reviews.Select(r => r.Id).ToList();
            var tags = await _unitOfWork.ReadRepository<ReviewTag>()
                .ListAsync(rt => reviewIds.Contains(rt.ReviewId), cancellationToken: cancellationToken);

            // Calculate tag frequencies
            var tagGroups = tags.GroupBy(t => t.Tag)
                .Select(g => new TagFrequencyResponse(
                    g.Key,
                    g.Count(),
                    g.All(t => t.IsPositive),
                    (decimal)Math.Round((double)g.Count() / tags.Count * 100, 2)
                ))
                .OrderByDescending(t => t.Count)
                .ToList();

            return Result.Ok(tagGroups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tag frequencies for entity {EntityId}", entityId);
            return Result.Fail<List<TagFrequencyResponse>>("Failed to get tag frequencies", "TAG_FREQUENCY_ERROR");
        }
    }

    #region Additional Review Methods
    /// <summary>
    /// Değerlendirmeyi raporlar (duplicate kontrolü, rapor kaydı).
    /// </summary>
    public async Task<Result> ReportReviewAsync(Guid reviewId, ReportReviewRequest request, Guid reporterId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if review exists
            var review = await _unitOfWork.ReadRepository<Review>()
                .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken: cancellationToken);

            if (review == null)
            {
                return Result.Fail("Review not found", "REVIEW_NOT_FOUND");
            }

            // Check if user already reported this review
            var existingReport = await _unitOfWork.ReadRepository<ReviewReport>()
                .FirstOrDefaultAsync(rr => rr.ReviewId == reviewId && rr.ReporterId == reporterId,
                    cancellationToken: cancellationToken);

            if (existingReport != null)
            {
                return Result.Fail("You have already reported this review", "ALREADY_REPORTED");
            }

            // Create review report
            var report = new ReviewReport
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                ReporterId = reporterId,
                Reason = request.Reason,
                Details = request.Details,
                ReportedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            await _unitOfWork.Repository<ReviewReport>().AddAsync(report, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review {ReviewId} reported by user {ReporterId} for reason: {Reason}",
                reviewId, reporterId, request.Reason);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting review {ReviewId}", reviewId);
            return Result.Fail("Failed to report review", "REPORT_REVIEW_ERROR");
        }
    }
    /// <summary>
    /// Değerlendirmeyi beğenir (duplicate kontrolü, performance tracking).
    /// </summary>
    public async Task<Result> LikeReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await LikeReviewInternalAsync(reviewId, userId, cancellationToken),
            "LikeReview",
            new { reviewId, userId },
            cancellationToken);
    }
    
    private async Task<Result> LikeReviewInternalAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if review exists
            var review = await _unitOfWork.ReadRepository<Review>()
                .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken: cancellationToken);

            if (review == null)
            {
                return Result.Fail("Review not found", "REVIEW_NOT_FOUND");
            }

            // Check if user already liked this review
            var existingLike = await _unitOfWork.ReadRepository<ReviewLike>()
                .FirstOrDefaultAsync(rl => rl.ReviewId == reviewId && rl.UserId == userId,
                    cancellationToken: cancellationToken);

            if (existingLike != null)
            {
                return Result.Fail("You have already liked this review", "ALREADY_LIKED");
            }

            // Create review like
            var like = new ReviewLike
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                UserId = userId,
                LikedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ReviewLike>().AddAsync(like, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking review {ReviewId}", reviewId);
            return Result.Fail("Failed to like review", "LIKE_REVIEW_ERROR");
        }
    }
    /// <summary>
    /// Beğeniyi geri alır (performance tracking).
    /// </summary>
    public async Task<Result> UnlikeReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await UnlikeReviewInternalAsync(reviewId, userId, cancellationToken),
            "UnlikeReview",
            new { reviewId, userId },
            cancellationToken);
    }
    
    private async Task<Result> UnlikeReviewInternalAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            // Find existing like
            var existingLike = await _unitOfWork.ReadRepository<ReviewLike>()
                .FirstOrDefaultAsync(rl => rl.ReviewId == reviewId && rl.UserId == userId,
                    cancellationToken: cancellationToken);

            if (existingLike == null)
            {
                return Result.Fail("You have not liked this review", "NOT_LIKED");
            }

            // Remove the like
            _unitOfWork.Repository<ReviewLike>().Delete(existingLike);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unliking review {ReviewId}", reviewId);
            return Result.Fail("Failed to unlike review", "UNLIKE_REVIEW_ERROR");
        }
    }
    
    #endregion
}
