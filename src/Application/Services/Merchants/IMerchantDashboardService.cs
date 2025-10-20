using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Merchants;

/// <summary>
/// Merchant dashboard servisi: dashboard verileri, son siparişler, en çok satılan ürünler, performans metrikleri.
/// </summary>
public interface IMerchantDashboardService
{
    /// <summary>Merchant dashboard verilerini getirir (ownership kontrolü).</summary>
    Task<Result<MerchantDashboardResponse>> GetDashboardAsync(Guid merchantId, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Son siparişleri getirir (ownership kontrolü).</summary>
    Task<Result<List<RecentOrderResponse>>> GetRecentOrdersAsync(Guid merchantId, Guid merchantOwnerId, int limit = 10, CancellationToken cancellationToken = default);
    /// <summary>En çok satılan ürünleri getirir (son 30 gün, ownership kontrolü).</summary>
    Task<Result<List<TopProductResponse>>> GetTopProductsAsync(Guid merchantId, Guid merchantOwnerId, int limit = 10, CancellationToken cancellationToken = default);
    /// <summary>Performans metriklerini hesaplar (ownership kontrolü).</summary>
    Task<Result<MerchantPerformanceMetrics>> GetPerformanceMetricsAsync(Guid merchantId, Guid merchantOwnerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}
