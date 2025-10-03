using Getir.Application.Common;

namespace Getir.Application.DTO;

// Review DTOs
public record CreateReviewRequest(
    Guid RevieweeId,
    string RevieweeType, // "Merchant" or "Courier"
    Guid OrderId,
    int Rating,
    string Comment,
    List<string>? Tags = null);

public record UpdateReviewRequest(
    int Rating,
    string Comment,
    List<string>? Tags = null);

public record ReviewResponse(
    Guid Id,
    Guid ReviewerId,
    string ReviewerName,
    Guid RevieweeId,
    string RevieweeName,
    string RevieweeType,
    Guid OrderId,
    int Rating,
    string Comment,
    List<string> Tags,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsApproved,
    int HelpfulCount,
    bool UserHasVotedHelpful);

public record ReviewStatsResponse(
    decimal AverageRating,
    int TotalReviews,
    int FiveStarCount,
    int FourStarCount,
    int ThreeStarCount,
    int TwoStarCount,
    int OneStarCount,
    decimal RecentAverageRating,
    int RecentReviewCount,
    decimal PositiveReviewRate);

// Rating DTOs
public record RatingResponse(
    Guid EntityId,
    string EntityType,
    decimal AverageRating,
    int TotalReviews,
    RatingBreakdownResponse Breakdown,
    RecentPerformanceResponse RecentPerformance,
    QualityMetricsResponse QualityMetrics,
    DateTime LastUpdated);

public record RatingBreakdownResponse(
    int FiveStarCount,
    int FourStarCount,
    int ThreeStarCount,
    int TwoStarCount,
    int OneStarCount);

public record RecentPerformanceResponse(
    decimal AverageRating,
    int ReviewCount,
    DateTime PeriodStart,
    DateTime PeriodEnd);

public record QualityMetricsResponse(
    decimal ResponseRate,
    decimal PositiveReviewRate,
    int TotalOrders);

// Review moderation DTOs
public record ReviewModerationRequest(
    bool IsApproved,
    string? ModerationNotes = null);

public record ReviewModerationResponse(
    Guid ReviewId,
    bool IsApproved,
    string? ModerationNotes,
    string ModeratorName,
    DateTime ModeratedAt);

public record PendingReviewResponse(
    Guid Id,
    string ReviewerName,
    string RevieweeName,
    string RevieweeType,
    int Rating,
    string Comment,
    List<string> Tags,
    DateTime CreatedAt,
    bool IsApproved);

// Review helpful votes
public record ReviewHelpfulRequest(
    bool IsHelpful);

public record ReviewHelpfulResponse(
    Guid ReviewId,
    int HelpfulCount,
    int NotHelpfulCount,
    bool UserVote);

// Review search and filtering
public record ReviewSearchQuery(
    Guid? RevieweeId = null,
    string? RevieweeType = null,
    int? MinRating = null,
    int? MaxRating = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    bool? IsApproved = null,
    string? Tag = null,
    int Page = 1,
    int PageSize = 20);

public record ReviewSearchResponse(
    PagedResult<ReviewResponse> Reviews,
    ReviewStatsResponse Stats,
    List<string> AvailableTags);

// Rating calculation DTOs
public record RatingCalculationRequest(
    Guid EntityId,
    string EntityType,
    DateTime? FromDate = null,
    DateTime? ToDate = null);

public record RatingCalculationResponse(
    Guid EntityId,
    string EntityType,
    decimal AverageRating,
    int TotalReviews,
    RatingBreakdownResponse Breakdown,
    DateTime CalculatedAt);

// Review analytics DTOs
public record ReviewAnalyticsResponse(
    Guid EntityId,
    string EntityType,
    List<RatingHistoryResponse> RatingHistory,
    List<ReviewTrendResponse> ReviewTrends,
    List<TagFrequencyResponse> TagFrequencies,
    ComparisonResponse Comparison);

public record RatingHistoryResponse(
    DateTime Date,
    decimal AverageRating,
    int TotalReviews,
    int NewReviews);

public record ReviewTrendResponse(
    DateTime Date,
    int ReviewCount,
    decimal AverageRating);

public record TagFrequencyResponse(
    string Tag,
    int Count,
    bool IsPositive,
    decimal Percentage);

public record ComparisonResponse(
    decimal EntityRating,
    decimal CategoryAverage,
    decimal PlatformAverage,
    int CategoryRank,
    int PlatformRank);

public record ReviewQuery(
    Guid? RevieweeId = null,
    string? RevieweeType = null,
    int? MinRating = null,
    int? MaxRating = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 20);

public record ReportReviewRequest(
    string Reason,
    string? Details = null);

public record ReviewStatisticsResponse(
    decimal AverageRating,
    int TotalReviews,
    Dictionary<int, int> RatingDistribution,
    decimal PositiveReviewRate);