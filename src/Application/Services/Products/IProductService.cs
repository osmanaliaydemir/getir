using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Products;

/// <summary>
/// Ürün servisi: ürün CRUD işlemleri, merchant bazlı yönetim, stok/müsaitlik, arama, istatistikler.
/// </summary>
public interface IProductService
{
    /// <summary>Merchant'a ait ürünleri sayfalama ile getirir (cache).</summary>
    Task<Result<PagedResult<ProductResponse>>> GetProductsByMerchantAsync(Guid merchantId, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Ürünü ID ile getirir (cache).</summary>
    Task<Result<ProductResponse>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>Yeni ürün oluşturur (merchant kontrolü).</summary>
    Task<Result<ProductResponse>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    /// <summary>Ürünü günceller (cache invalidation).</summary>
    Task<Result<ProductResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    /// <summary>Ürünü siler (soft delete, cache invalidation).</summary>
    Task<Result> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
    // Merchant-specific methods
    /// <summary>Merchant sahibinin ürünlerini getirir (ownership kontrolü).</summary>
    Task<Result<PagedResult<ProductResponse>>> GetMyProductsAsync(Guid merchantOwnerId, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Merchant sahibi için ürün oluşturur (ownership kontrolü).</summary>
    Task<Result<ProductResponse>> CreateMyProductAsync(CreateProductRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Merchant sahibi ürününü günceller (ownership kontrolü).</summary>
    Task<Result<ProductResponse>> UpdateMyProductAsync(Guid id, UpdateProductRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Merchant sahibi ürününü siler (ownership kontrolü, soft delete).</summary>
    Task<Result> DeleteMyProductAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Ürün stok miktarını günceller (ownership kontrolü).</summary>
    Task<Result> UpdateProductStockAsync(Guid id, int newStockQuantity, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Ürün müsaitliğini açar/kapatır (ownership kontrolü).</summary>
    Task<Result> ToggleProductAvailabilityAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Toplu ürün sırası güncelleme (ownership kontrolü).</summary>
    Task<Result> BulkUpdateProductOrderAsync(List<UpdateProductOrderRequest> requests, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    // Additional merchant product methods
    /// <summary>Merchant ürün istatistiklerini getirir.</summary>
    Task<Result<ProductStatisticsResponse>> GetMyProductStatisticsAsync(Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Toplu ürün durum güncelleme.</summary>
    Task<Result<BulkUpdateProductStatusResponse>> BulkUpdateMyProductStatusAsync(BulkUpdateProductStatusRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    // Additional methods for user search
    /// <summary>Ürünleri arar (isim/açıklama, aktif/müsait filtresi).</summary>
    Task<Result<PagedResult<ProductResponse>>> SearchProductsAsync(string searchQuery, PaginationQuery query, CancellationToken cancellationToken = default);
}
