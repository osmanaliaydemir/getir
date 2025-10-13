using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public interface ICategoryService
{
    Task<List<ProductCategoryResponse>?> GetCategoriesAsync(CancellationToken ct = default);
    Task<List<ProductCategoryResponse>?> GetMyCategoriesAsync(CancellationToken ct = default);
    Task<ProductCategoryResponse?> GetCategoryByIdAsync(Guid categoryId, CancellationToken ct = default);
    Task<ProductCategoryResponse?> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<ProductCategoryResponse?> UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken ct = default);
    Task<bool> DeleteCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task<List<CategoryTreeNode>?> GetCategoryTreeAsync(CancellationToken ct = default);
}

