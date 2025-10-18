using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public interface IMerchantService
{
    /// <summary>
    /// Mağazayı getirir.
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Mağaza</returns>
    Task<MerchantResponse?> GetMyMerchantAsync(CancellationToken ct = default);
    /// <summary>
    /// Mağazayı günceller.
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="request">Mağaza güncelleme isteği</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Mağaza</returns>
    Task<MerchantResponse?> UpdateMerchantAsync(Guid merchantId, UpdateMerchantRequest request, CancellationToken ct = default);
    /// <summary>
    /// Mağaza dashboard'ını getirir.
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Mağaza dashboard'ı</returns>
    Task<MerchantDashboardResponse?> GetDashboardAsync(Guid merchantId, CancellationToken ct = default);
    /// <summary>
    /// Son siparişleri getirir.
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="limit">Limit</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Son siparişler</returns>
    Task<List<RecentOrderResponse>?> GetRecentOrdersAsync(Guid merchantId, int limit = 10, CancellationToken ct = default);
    /// <summary>
    /// En çok satılan ürünleri getirir.
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="limit">Limit</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>En çok satılan ürünler</returns>
    Task<List<TopProductResponse>?> GetTopProductsAsync(Guid merchantId, int limit = 10, CancellationToken ct = default);
}

