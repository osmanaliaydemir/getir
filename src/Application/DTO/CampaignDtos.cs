namespace Getir.Application.DTO;

public record CreateCampaignRequest(
    string Title,
    string? Description,
    string? ImageUrl,
    Guid? MerchantId,
    string DiscountType,
    decimal DiscountValue,
    DateTime StartDate,
    DateTime EndDate,
    int DisplayOrder);

public record CampaignResponse(
    Guid Id,
    string Title,
    string? Description,
    string? ImageUrl,
    Guid? MerchantId,
    string? MerchantName,
    string DiscountType,
    decimal DiscountValue,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    int DisplayOrder);
