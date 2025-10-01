namespace Getir.Application.DTO;

public record CreateProductRequest(
    Guid MerchantId,
    Guid? ProductCategoryId,
    string Name,
    string? Description,
    decimal Price,
    decimal? DiscountedPrice,
    int StockQuantity,
    string? Unit);

public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    decimal? DiscountedPrice,
    int StockQuantity,
    string? Unit,
    bool IsAvailable);

public record ProductResponse(
    Guid Id,
    Guid MerchantId,
    string MerchantName,
    Guid? ProductCategoryId,
    string? ProductCategoryName,
    string Name,
    string? Description,
    string? ImageUrl,
    decimal Price,
    decimal? DiscountedPrice,
    int StockQuantity,
    string? Unit,
    bool IsAvailable);

// Merchant-specific DTOs
public record UpdateProductStockRequest(
    Guid ProductId,
    int NewStockQuantity);

public record UpdateProductOrderRequest(
    Guid ProductId,
    int NewDisplayOrder);

public record BulkUpdateProductOrderRequest(
    List<UpdateProductOrderRequest> Products);

public record ToggleProductAvailabilityRequest(
    Guid ProductId,
    bool IsAvailable);