using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.ProductOptions;

/// <summary>
/// Market ürün varyant servisi: ürün varyantları (beden/renk/tat) yönetimi, stok güncelleme, toplu işlemler.
/// </summary>
public interface IMarketProductVariantService
{
    /// <summary>Ürüne ait varyantları sayfalama ile getirir.</summary>
    Task<Result<PagedResult<MarketProductVariantResponse>>> GetProductVariantsAsync(Guid productId, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Varyantı ID ile getirir.</summary>
    Task<Result<MarketProductVariantResponse>> GetProductVariantAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>Yeni varyant oluşturur (ownership kontrolü, SKU benzersizlik).</summary>
    Task<Result<MarketProductVariantResponse>> CreateProductVariantAsync(CreateMarketProductVariantRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Varyantı günceller (ownership kontrolü, SKU kontrolü).</summary>
    Task<Result<MarketProductVariantResponse>> UpdateProductVariantAsync(Guid id, UpdateMarketProductVariantRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Varyantı siler (ownership kontrolü).</summary>
    Task<Result> DeleteProductVariantAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Varyant stok miktarını günceller (ownership kontrolü, müsaitlik otomatik ayarlanır).</summary>
    Task<Result> UpdateVariantStockAsync(Guid id, int newStockQuantity, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Toplu varyant stok güncelleme (ownership kontrolü).</summary>
    Task<Result> BulkUpdateVariantStockAsync(List<UpdateVariantStockRequest> requests, Guid merchantOwnerId, CancellationToken cancellationToken = default);
}
public record UpdateVariantStockRequest(Guid VariantId, int NewStockQuantity);
