namespace Getir.Application.DTO;

/// <summary>
/// Request DTO for creating a new product
/// </summary>
/// <param name="MerchantId">The unique identifier of the merchant</param>
/// <param name="ProductCategoryId">The unique identifier of the product category (optional)</param>
/// <param name="Name">The name of the product</param>
/// <param name="Description">The description of the product (optional)</param>
/// <param name="Price">The price of the product</param>
/// <param name="DiscountedPrice">The discounted price of the product (optional)</param>
/// <param name="StockQuantity">The stock quantity of the product</param>
/// <param name="Unit">The unit of measurement for the product (optional)</param>
public record CreateProductRequest(
    Guid MerchantId,
    Guid? ProductCategoryId,
    string Name,
    string? Description,
    decimal Price,
    decimal? DiscountedPrice,
    int StockQuantity,
    string? Unit);

/// <summary>
/// Request DTO for updating an existing product
/// </summary>
/// <param name="Name">The name of the product</param>
/// <param name="Description">The description of the product (optional)</param>
/// <param name="Price">The price of the product</param>
/// <param name="DiscountedPrice">The discounted price of the product (optional)</param>
/// <param name="StockQuantity">The stock quantity of the product</param>
/// <param name="Unit">The unit of measurement for the product (optional)</param>
/// <param name="IsAvailable">Whether the product is available for purchase</param>
public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    decimal? DiscountedPrice,
    int StockQuantity,
    string? Unit,
    bool IsAvailable);

/// <summary>
/// Response DTO for product information
/// </summary>
public record ProductResponse : BaseStatusEntityResponse
{
    public Guid MerchantId { get; init; }
    public string MerchantName { get; init; } = string.Empty;
    public Guid? ProductCategoryId { get; init; }
    public string? ProductCategoryName { get; init; }
    public string? ImageUrl { get; init; }
    public decimal Price { get; init; }
    public decimal? DiscountedPrice { get; init; }
    public int StockQuantity { get; init; }
    public string? Unit { get; init; }
    public bool IsAvailable { get; init; }
}

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

public record BulkUpdateProductStatusRequest(
    List<Guid> ProductIds,
    bool IsAvailable);

public record BulkUpdateProductStatusResponse(
    int TotalUpdated,
    int SuccessCount,
    int FailureCount,
    List<Guid> UpdatedProductIds,
    List<string> Errors);

public record ProductStatisticsResponse(
    int TotalProducts,
    int ActiveProducts,
    int InactiveProducts,
    int OutOfStockProducts,
    int LowStockProducts,
    decimal TotalInventoryValue,
    DateTime LastUpdated);