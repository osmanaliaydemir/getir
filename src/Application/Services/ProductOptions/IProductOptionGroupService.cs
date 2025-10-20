using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.ProductOptions;

/// <summary>
/// Ürün opsiyon grubu servisi: ürün seçenekleri gruplarının yönetimi (zorunluluk, min/max seçim, sıralama).
/// </summary>
public interface IProductOptionGroupService
{
    /// <summary>Ürüne ait opsiyon gruplarını sayfalama ile getirir.</summary>
    Task<Result<PagedResult<ProductOptionGroupResponse>>> GetProductOptionGroupsAsync(Guid productId, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Opsiyon grubunu ID ile getirir (opsiyonlar dahil).</summary>
    Task<Result<ProductOptionGroupResponse>> GetProductOptionGroupAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>Yeni opsiyon grubu oluşturur (ownership kontrolü, zorunluluk/seçim kuralları).</summary>
    Task<Result<ProductOptionGroupResponse>> CreateProductOptionGroupAsync(CreateProductOptionGroupRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Opsiyon grubunu günceller (ownership kontrolü).</summary>
    Task<Result<ProductOptionGroupResponse>> UpdateProductOptionGroupAsync(Guid id, UpdateProductOptionGroupRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Opsiyon grubunu siler (ownership kontrolü, soft delete).</summary>
    Task<Result> DeleteProductOptionGroupAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Opsiyon gruplarının sırasını yeniden düzenler (ownership kontrolü).</summary>
    Task<Result> ReorderProductOptionGroupsAsync(Guid productId, List<Guid> orderedGroupIds, Guid merchantOwnerId, CancellationToken cancellationToken = default);
}
