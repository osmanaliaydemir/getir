namespace Getir.Application.DTO;

public record AddToCartRequest(
    Guid MerchantId,
    Guid ProductId,
    int Quantity,
    string? Notes);

public record UpdateCartItemRequest(
    int Quantity,
    string? Notes);

public record CartItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductImage,
    decimal UnitPrice,
    decimal? DiscountedPrice,
    int Quantity,
    decimal TotalPrice,
    string? Notes);

public record CartResponse(
    Guid MerchantId,
    string MerchantName,
    List<CartItemResponse> Items,
    decimal SubTotal,
    decimal DeliveryFee,
    decimal Total,
    int ItemCount);

public record ApplyCouponRequest(
    string CouponCode);
