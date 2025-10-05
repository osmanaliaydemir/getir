using Getir.Application.Common;

namespace Getir.Application.DTO;

// ProductOptionGroup DTOs
public record CreateProductOptionGroupRequest(
    Guid ProductId,
    string Name,
    string? Description,
    bool IsRequired,
    int MinSelection,
    int MaxSelection,
    int DisplayOrder);

public record UpdateProductOptionGroupRequest(
    string Name,
    string? Description,
    bool IsRequired,
    int MinSelection,
    int MaxSelection,
    int DisplayOrder,
    bool IsActive);

public record ProductOptionGroupResponse(
    Guid Id,
    Guid ProductId,
    string Name,
    string? Description,
    bool IsRequired,
    int MinSelection,
    int MaxSelection,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<ProductOptionResponse> Options);

// ProductOption DTOs
public record CreateProductOptionRequest(
    Guid ProductOptionGroupId,
    string Name,
    string? Description,
    decimal ExtraPrice,
    bool IsDefault,
    int DisplayOrder);

public record UpdateProductOptionRequest(
    Guid Id,
    string Name,
    string? Description,
    decimal ExtraPrice,
    bool IsDefault,
    int DisplayOrder,
    bool IsActive);

public record ProductOptionResponse(
    Guid Id,
    Guid ProductOptionGroupId,
    string Name,
    string? Description,
    decimal ExtraPrice,
    bool IsDefault,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// Bulk operations
public record BulkCreateProductOptionsRequest(
    Guid ProductOptionGroupId,
    List<CreateProductOptionRequest> Options);

public record BulkUpdateProductOptionsRequest(
    List<UpdateProductOptionRequest> Options);

// Market Product Variant DTOs
public record CreateMarketProductVariantRequest(
    Guid ProductId,
    string Name,
    string? Description,
    string? SKU,
    decimal Price,
    decimal? DiscountedPrice,
    int StockQuantity,
    int DisplayOrder,
    string? Size,
    string? Color,
    string? Flavor,
    string? Material,
    string? Weight,
    string? Volume);

public record UpdateMarketProductVariantRequest(
    string Name,
    string? Description,
    string? SKU,
    decimal Price,
    decimal? DiscountedPrice,
    int StockQuantity,
    int DisplayOrder,
    string? Size,
    string? Color,
    string? Flavor,
    string? Material,
    string? Weight,
    string? Volume,
    bool IsAvailable);

public record MarketProductVariantResponse(
    Guid Id,
    Guid ProductId,
    string Name,
    string? Description,
    string? SKU,
    decimal Price,
    decimal? DiscountedPrice,
    int StockQuantity,
    bool IsAvailable,
    int DisplayOrder,
    string? Size,
    string? Color,
    string? Flavor,
    string? Material,
    string? Weight,
    string? Volume,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// Restaurant Product Option DTOs
public record CreateRestaurantProductOptionGroupRequest(
    Guid ProductId,
    string Name,
    string? Description,
    bool IsRequired,
    bool IsMultipleSelection,
    int MaxSelections,
    int DisplayOrder);

public record UpdateRestaurantProductOptionGroupRequest(
    string Name,
    string? Description,
    bool IsRequired,
    bool IsMultipleSelection,
    int MaxSelections,
    int DisplayOrder);

public record RestaurantProductOptionGroupResponse(
    Guid Id,
    Guid ProductId,
    string Name,
    string? Description,
    bool IsRequired,
    bool IsMultipleSelection,
    int MaxSelections,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<RestaurantProductOptionResponse> Options);

public record CreateRestaurantProductOptionRequest(
    Guid ProductId,
    Guid? OptionGroupId,
    string Name,
    string? Description,
    decimal Price,
    bool IsRequired,
    int DisplayOrder);

public record UpdateRestaurantProductOptionRequest(
    string Name,
    string? Description,
    decimal Price,
    bool IsRequired,
    int DisplayOrder,
    bool IsAvailable);

public record RestaurantProductOptionResponse(
    Guid Id,
    Guid ProductId,
    Guid? OptionGroupId,
    string Name,
    string? Description,
    decimal Price,
    bool IsRequired,
    bool IsAvailable,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// Order Line Option DTOs
public record CreateOrderLineOptionRequest(
    Guid ProductOptionId,
    string OptionName,
    decimal ExtraPrice);

public record OrderLineOptionResponse(
    Guid Id,
    Guid ProductOptionId,
    string OptionName,
    decimal ExtraPrice,
    DateTime CreatedAt);

// Product with options response
public record ProductWithOptionsResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    decimal? DiscountedPrice,
    bool IsAvailable,
    List<ProductOptionGroupResponse> OptionGroups,
    List<MarketProductVariantResponse>? Variants = null);

// Variant selection for orders
public record ProductVariantSelectionRequest(
    Guid ProductId,
    Guid? VariantId,
    List<Guid> SelectedOptionIds);

public record ProductVariantSelectionResponse(
    Guid ProductId,
    string ProductName,
    Guid? VariantId,
    string? VariantName,
    decimal BasePrice,
    decimal OptionsPrice,
    decimal TotalPrice,
    List<ProductOptionResponse> SelectedOptions);