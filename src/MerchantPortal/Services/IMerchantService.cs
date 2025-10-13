using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public interface IMerchantService
{
    Task<MerchantResponse?> GetMyMerchantAsync(CancellationToken ct = default);
    Task<MerchantResponse?> UpdateMerchantAsync(Guid merchantId, UpdateMerchantRequest request, CancellationToken ct = default);
    Task<MerchantDashboardResponse?> GetDashboardAsync(Guid merchantId, CancellationToken ct = default);
    Task<List<RecentOrderResponse>?> GetRecentOrdersAsync(Guid merchantId, int limit = 10, CancellationToken ct = default);
    Task<List<TopProductResponse>?> GetTopProductsAsync(Guid merchantId, int limit = 10, CancellationToken ct = default);
}

