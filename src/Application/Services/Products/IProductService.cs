using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Products;

public interface IProductService
{
    Task<Result<PagedResult<ProductResponse>>> GetProductsByMerchantAsync(Guid merchantId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result<ProductResponse>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ProductResponse>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
    // Merchant-specific methods
    Task<Result<PagedResult<ProductResponse>>> GetMyProductsAsync(Guid merchantOwnerId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result<ProductResponse>> CreateMyProductAsync(CreateProductRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result<ProductResponse>> UpdateMyProductAsync(Guid id, UpdateProductRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result> DeleteMyProductAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result> UpdateProductStockAsync(Guid id, int newStockQuantity, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result> ToggleProductAvailabilityAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result> BulkUpdateProductOrderAsync(List<UpdateProductOrderRequest> requests, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    // Additional merchant product methods
    Task<Result<ProductStatisticsResponse>> GetMyProductStatisticsAsync(Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result<BulkUpdateProductStatusResponse>> BulkUpdateMyProductStatusAsync(BulkUpdateProductStatusRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    // Additional methods for user search
    Task<Result<PagedResult<ProductResponse>>> SearchProductsAsync(string searchQuery, PaginationQuery query, CancellationToken cancellationToken = default);
}
