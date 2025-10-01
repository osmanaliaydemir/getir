namespace Getir.Application.DTO;

public record CreateCouponRequest(
    string Code,
    string Title,
    string? Description,
    string DiscountType, // Percentage, FixedAmount
    decimal DiscountValue,
    decimal MinimumOrderAmount,
    decimal? MaximumDiscountAmount,
    DateTime StartDate,
    DateTime EndDate,
    int? UsageLimit);

public record ValidateCouponRequest(
    string Code,
    decimal OrderAmount);

public record CouponResponse(
    Guid Id,
    string Code,
    string Title,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    decimal MinimumOrderAmount,
    decimal? MaximumDiscountAmount,
    DateTime StartDate,
    DateTime EndDate,
    int? UsageLimit,
    int UsageCount,
    bool IsActive);

public record CouponValidationResponse(
    bool IsValid,
    string? ErrorMessage,
    decimal DiscountAmount,
    string CouponCode);
