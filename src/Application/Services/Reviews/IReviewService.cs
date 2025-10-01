using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Reviews;

public interface IReviewService
{
    // Review CRUD operations
    Task<Result<ReviewResponse>> CreateReviewAsync(CreateReviewRequest request, Guid reviewerId, CancellationToken cancellationToken = default);
    Task<Result<ReviewResponse>> UpdateReviewAsync(Guid reviewId, UpdateReviewRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default);
    Task<Result<ReviewResponse>> GetReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);
    
    // Review queries
    Task<Result<PagedResult<ReviewResponse>>> GetReviewsAsync(ReviewSearchQuery query, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ReviewResponse>>> GetReviewsByRevieweeAsync(Guid revieweeId, string revieweeType, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ReviewResponse>>> GetReviewsByReviewerAsync(Guid reviewerId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ReviewResponse>>> GetReviewsByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    
    // Rating operations
    Task<Result<RatingResponse>> GetRatingAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default);
    Task<Result<ReviewStatsResponse>> GetReviewStatsAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default);
    Task<Result<RatingCalculationResponse>> CalculateRatingAsync(RatingCalculationRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateRatingAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default);
    
    // Review moderation
    Task<Result<PagedResult<PendingReviewResponse>>> GetPendingReviewsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result<ReviewModerationResponse>> ModerateReviewAsync(Guid reviewId, ReviewModerationRequest request, Guid moderatorId, CancellationToken cancellationToken = default);
    Task<Result> BulkModerateReviewsAsync(List<Guid> reviewIds, ReviewModerationRequest request, Guid moderatorId, CancellationToken cancellationToken = default);
    
    // Review helpful votes
    Task<Result<ReviewHelpfulResponse>> VoteHelpfulAsync(Guid reviewId, ReviewHelpfulRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<Result> RemoveHelpfulVoteAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default);
    
    // Review analytics
    Task<Result<ReviewAnalyticsResponse>> GetReviewAnalyticsAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default);
    Task<Result<ReviewSearchResponse>> SearchReviewsAsync(ReviewSearchQuery query, CancellationToken cancellationToken = default);
    
    // Review validation
    Task<Result<bool>> CanUserReviewAsync(Guid userId, Guid revieweeId, string revieweeType, Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<bool>> HasUserReviewedOrderAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default);
    
    // Review tags
    Task<Result<List<string>>> GetAvailableTagsAsync(string revieweeType, CancellationToken cancellationToken = default);
    Task<Result<List<TagFrequencyResponse>>> GetTagFrequenciesAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default);
}
