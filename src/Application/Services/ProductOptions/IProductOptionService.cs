using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.ProductOptions;

/// <summary>
/// Ürün opsiyon servisi: ürün seçenekleri yönetimi (extra fiyat, varsayılan seçim, toplu işlemler).
/// </summary>
public interface IProductOptionService
{
    /// <summary>Opsiyon grubuna ait opsiyonları sayfalama ile getirir.</summary>
    Task<Result<PagedResult<ProductOptionResponse>>> GetProductOptionsAsync(Guid productOptionGroupId, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Opsiyonu ID ile getirir.</summary>
    Task<Result<ProductOptionResponse>> GetProductOptionAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>Yeni opsiyon oluşturur (ownership kontrolü, varsayılan seçim yönetimi).</summary>
    Task<Result<ProductOptionResponse>> CreateProductOptionAsync(CreateProductOptionRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Opsiyonu günceller (ownership kontrolü, varsayılan seçim yönetimi).</summary>
    Task<Result<ProductOptionResponse>> UpdateProductOptionAsync(Guid id, UpdateProductOptionRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Opsiyonu siler (ownership kontrolü).</summary>
    Task<Result> DeleteProductOptionAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Toplu opsiyon oluşturma (ownership kontrolü, varsayılan seçim tek olmalı).</summary>
    Task<Result> BulkCreateProductOptionsAsync(BulkCreateProductOptionsRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Toplu opsiyon güncelleme (ownership kontrolü).</summary>
    Task<Result> BulkUpdateProductOptionsAsync(BulkUpdateProductOptionsRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
}
