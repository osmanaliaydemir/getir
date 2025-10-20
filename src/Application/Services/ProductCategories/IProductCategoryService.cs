using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.ProductCategories;

/// <summary>
/// Ürün kategorisi servisi: merchant bazlı kategori yönetimi (CRUD, ağaç yapısı, cache).
/// </summary>
public interface IProductCategoryService
{
    /// <summary>Merchant'a ait kategorileri getirir (cache).</summary>
    Task<Result<List<ProductCategoryResponse>>> GetMerchantCategoriesAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Merchant'a ait kategori ağacını getirir (parent-child hiyerarşisi, cache).</summary>
    Task<Result<List<ProductCategoryTreeResponse>>> GetMerchantCategoryTreeAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Kategoriyi ID ile getirir (cache).</summary>
    Task<Result<ProductCategoryResponse>> GetProductCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>Yeni kategori oluşturur (parent kontrolü, merchant sahibi).</summary>
    Task<Result<ProductCategoryResponse>> CreateProductCategoryAsync(CreateProductCategoryRequest request, Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Kategoriyi günceller (ownership kontrolü, parent kontrolü, cache invalidation).</summary>
    Task<Result<ProductCategoryResponse>> UpdateProductCategoryAsync(Guid id, UpdateProductCategoryRequest request, Guid currentUserId, CancellationToken cancellationToken = default);
    /// <summary>Kategoriyi siler (ownership kontrolü, alt kategori/ürün kontrolü, soft delete, cache invalidation).</summary>
    Task<Result> DeleteProductCategoryAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);
}

