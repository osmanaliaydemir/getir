using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.ProductCategories;

public interface IProductCategoryService
{
    // Merchant kendi kategorilerini yönetir
    Task<Result<List<ProductCategoryResponse>>> GetMerchantCategoriesAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<Result<List<ProductCategoryTreeResponse>>> GetMerchantCategoryTreeAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<Result<ProductCategoryResponse>> GetProductCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ProductCategoryResponse>> CreateProductCategoryAsync(CreateProductCategoryRequest request, Guid merchantId, CancellationToken cancellationToken = default);
    Task<Result<ProductCategoryResponse>> UpdateProductCategoryAsync(Guid id, UpdateProductCategoryRequest request, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<Result> DeleteProductCategoryAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);
}

