namespace Getir.Application.DTO;

public record CreateServiceCategoryRequest(
    string Name,
    string? Description,
    string? ImageUrl,
    string? IconUrl,
    int DisplayOrder);

public record UpdateServiceCategoryRequest(
    string Name,
    string? Description,
    string? ImageUrl,
    string? IconUrl,
    int DisplayOrder,
    bool IsActive);

public record ServiceCategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl,
    string? IconUrl,
    int DisplayOrder,
    bool IsActive,
    int MerchantCount);

