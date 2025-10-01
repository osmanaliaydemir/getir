namespace Getir.Application.DTO;

public record CreateMerchantRequest(
    string Name,
    string? Description,
    Guid CategoryId,
    string Address,
    decimal Latitude,
    decimal Longitude,
    string PhoneNumber,
    string? Email,
    decimal MinimumOrderAmount,
    decimal DeliveryFee);

public record UpdateMerchantRequest(
    string Name,
    string? Description,
    string Address,
    string PhoneNumber,
    string? Email,
    decimal MinimumOrderAmount,
    decimal DeliveryFee);

public record MerchantResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid CategoryId,
    string CategoryName,
    string? LogoUrl,
    string Address,
    decimal Latitude,
    decimal Longitude,
    decimal MinimumOrderAmount,
    decimal DeliveryFee,
    int AverageDeliveryTime,
    decimal? Rating,
    bool IsActive,
    bool IsOpen);
