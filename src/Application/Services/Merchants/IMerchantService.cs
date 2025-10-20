using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Merchants;

/// <summary>
/// Merchant yönetimi servisi: merchant CRUD işlemleri, kategori bazlı listeleme, arama.
/// </summary>
public interface IMerchantService
{
    /// <summary>Tüm merchantları sayfalama ile getirir (cache).</summary>
    Task<Result<PagedResult<MerchantResponse>>> GetMerchantsAsync(PaginationQuery query, CancellationToken cancellationToken = default);

    /// <summary>Merchant'ı ID ile getirir (cache).</summary>
    Task<Result<MerchantResponse>> GetMerchantByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Merchant'ı owner ID ile getirir (cache).</summary>
    Task<Result<MerchantResponse>> GetMerchantByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>Yeni merchant oluşturur (validasyon, cache invalidation, background task).</summary>
    Task<Result<MerchantResponse>> CreateMerchantAsync(CreateMerchantRequest request, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>Merchant günceller (ownership kontrolü, cache invalidation).</summary>
    Task<Result<MerchantResponse>> UpdateMerchantAsync(Guid id, UpdateMerchantRequest request, Guid currentUserId, CancellationToken cancellationToken = default);

    /// <summary>Merchant siler (soft delete, cache invalidation).</summary>
    Task<Result> DeleteMerchantAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Kategoriye göre merchantları sayfalama ile getirir (cache).</summary>
    Task<Result<PagedResult<MerchantResponse>>> GetMerchantsByCategoryTypeAsync(ServiceCategoryType categoryType, PaginationQuery query, CancellationToken cancellationToken = default);

    /// <summary>Kategoriye göre aktif ve açık merchantları getirir (cache).</summary>
    Task<Result<IEnumerable<MerchantResponse>>> GetActiveMerchantsByCategoryTypeAsync(ServiceCategoryType categoryType, CancellationToken cancellationToken = default);
    // Additional methods for user features
    /// <summary>Aktif merchantları sayfalama ile getirir.</summary>
    Task<Result<PagedResult<MerchantResponse>>> GetActiveMerchantsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Merchantlarda arama yapar (isim/açıklama).</summary>
    Task<Result<PagedResult<MerchantResponse>>> SearchMerchantsAsync(string searchQuery, PaginationQuery query, CancellationToken cancellationToken = default);
}
