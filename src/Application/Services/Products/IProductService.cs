using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Products;

public interface IProductService
{
    Task<Result<PagedResult<ProductResponse>>> GetProductsByMerchantAsync(
        Guid merchantId,
        PaginationQuery query,
        CancellationToken cancellationToken = default);
    
    Task<Result<ProductResponse>> GetProductByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    
    Task<Result<ProductResponse>> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default);
    
    Task<Result<ProductResponse>> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default);
    
    Task<Result> DeleteProductAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
