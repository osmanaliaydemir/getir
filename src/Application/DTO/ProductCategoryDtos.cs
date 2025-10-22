namespace Getir.Application.DTO;

public record CreateProductCategoryRequest(
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    string? ImageUrl,
    int DisplayOrder);

public record UpdateProductCategoryRequest(
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive);

public record ProductCategoryResponse(
    Guid Id,
    Guid? MerchantId,
    string MerchantName,
    Guid? ParentCategoryId,
    string? ParentCategoryName,
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    int SubCategoryCount,
    int ProductCount);

public record ProductCategoryTreeResponse(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    int ProductCount,
    List<ProductCategoryTreeResponse> SubCategories);

