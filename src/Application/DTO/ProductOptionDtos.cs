namespace Getir.Application.DTO;

// ProductOptionGroup DTOs
public record ProductOptionGroupResponse(
    Guid Id,
    Guid ProductId,
    string Name,
    string Description,
    bool IsRequired,
    int MinSelection,
    int MaxSelection,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<ProductOptionResponse> Options);

public record CreateProductOptionGroupRequest(
    Guid ProductId,
    string Name,
    string Description,
    bool IsRequired = false,
    int MinSelection = 0,
    int MaxSelection = 1,
    int DisplayOrder = 0);

public record UpdateProductOptionGroupRequest(
    string Name,
    string Description,
    bool IsRequired,
    int MinSelection,
    int MaxSelection,
    int DisplayOrder,
    bool IsActive);

// ProductOption DTOs
public record ProductOptionResponse(
    Guid Id,
    Guid ProductOptionGroupId,
    string Name,
    string Description,
    decimal ExtraPrice,
    bool IsDefault,
    bool IsActive,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateProductOptionRequest(
    Guid ProductOptionGroupId,
    string Name,
    string Description,
    decimal ExtraPrice = 0,
    bool IsDefault = false,
    int DisplayOrder = 0);

public record UpdateProductOptionRequest(
    Guid Id,
    string Name,
    string Description,
    decimal ExtraPrice,
    bool IsDefault,
    int DisplayOrder,
    bool IsActive);

// OrderLineOption DTOs
public record OrderLineOptionResponse(
    Guid Id,
    Guid OrderLineId,
    Guid ProductOptionId,
    string OptionName,
    decimal ExtraPrice,
    DateTime CreatedAt);

public record CreateOrderLineOptionRequest(
    Guid ProductOptionId);

// Bulk operations
public record BulkCreateProductOptionsRequest(
    Guid ProductOptionGroupId,
    List<CreateProductOptionRequest> Options);

public record BulkUpdateProductOptionsRequest(
    List<UpdateProductOptionRequest> Options);
