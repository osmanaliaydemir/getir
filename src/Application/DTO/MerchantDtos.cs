namespace Getir.Application.DTO;

public record CreateMerchantRequest(
    string Name,
    string? Description,
    Guid ServiceCategoryId,
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
    decimal Latitude,
    decimal Longitude,
    string PhoneNumber,
    string? Email,
    decimal MinimumOrderAmount,
    decimal DeliveryFee,
    int AverageDeliveryTime,
    bool IsActive,
    bool IsBusy,
    string? LogoUrl,
    string? CoverImageUrl);

public record MerchantResponse : BaseRatedEntityResponse
{
    public Guid OwnerId { get; init; }
    public string OwnerName { get; init; } = string.Empty;
    public Guid ServiceCategoryId { get; init; }
    public string ServiceCategoryName { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? CoverImageUrl { get; init; }
    public string Address { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public decimal MinimumOrderAmount { get; init; }
    public decimal DeliveryFee { get; init; }
    public int AverageDeliveryTime { get; init; }
    public bool IsBusy { get; init; }
    public bool IsOpen { get; init; }
}
