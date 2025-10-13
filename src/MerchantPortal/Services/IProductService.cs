using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public interface IProductService
{
    Task<PagedResult<ProductResponse>?> GetProductsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<ProductResponse?> GetProductByIdAsync(Guid productId, CancellationToken ct = default);
    Task<ProductResponse?> CreateProductAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductResponse?> UpdateProductAsync(Guid productId, UpdateProductRequest request, CancellationToken ct = default);
    Task<bool> DeleteProductAsync(Guid productId, CancellationToken ct = default);
    Task<List<ProductCategoryResponse>?> GetCategoriesAsync(CancellationToken ct = default);
}

