namespace Getir.Application.DTO;

public record CreateProductReviewRequest(
    Guid ProductId,
    Guid OrderId,
    int Rating, // 1-5
    string? Comment,
    string? ImageUrls); // JSON array of image URLs

public record UpdateProductReviewRequest(
    int Rating,
    string? Comment,
    string? ImageUrls);

public record ProductReviewResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid UserId,
    string UserName,
    Guid OrderId,
    int Rating,
    string? Comment,
    string? ImageUrls,
    bool IsVerifiedPurchase,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int HelpfulCount,
    int NotHelpfulCount,
    bool IsApproved);

