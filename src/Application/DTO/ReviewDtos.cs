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
    string RevieweeType,
    Guid OrderId,
    int Rating,
    string Comment,
    bool IsApproved,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<string> Tags,
    bool HasVoted,
    int HelpfulCount,
    int TotalOrders);

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
    string Action, // "approve", "reject", "flag"
    string? Notes = null);

public record ReviewModerationResponse(
    Guid ReviewId,
    string Action,
    string? Notes,
    Guid ModeratorId,
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
    Guid UserId,
    bool IsHelpful,
    int HelpfulCount,
    int NotHelpfulCount,
    DateTime VotedAt);

// Review search and filtering
public record ReviewSearchQuery(
    Guid? EntityId = null,
    string? EntityType = null,
    int? MinRating = null,
    int? MaxRating = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? SearchTerm = null,
    bool HasTags = false,
    List<string>? Tags = null,
    string? SortBy = null,
    bool SortDescending = true,
    int Page = 1,
    int PageSize = 20);

public record ReviewSearchResponse(
    List<ReviewResponse> Reviews,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    string? SearchTerm,
    Dictionary<string, object>? Filters);

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
    int TotalReviews,
    decimal AverageRating,
    int HelpfulVotes,
    int TotalVotes,
    int FiveStarCount,
    int OneStarCount,
    Dictionary<int, int> RatingDistribution,
    List<string> TopTags,
    DateTime GeneratedAt);

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