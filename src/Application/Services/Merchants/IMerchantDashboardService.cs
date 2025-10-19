using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Merchants;

public interface IMerchantDashboardService
{
    Task<Result<MerchantDashboardResponse>> GetDashboardAsync(Guid merchantId, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result<List<RecentOrderResponse>>> GetRecentOrdersAsync(Guid merchantId, Guid merchantOwnerId, int limit = 10, CancellationToken cancellationToken = default);
    Task<Result<List<TopProductResponse>>> GetTopProductsAsync(Guid merchantId, Guid merchantOwnerId, int limit = 10, CancellationToken cancellationToken = default);
    Task<Result<MerchantPerformanceMetrics>> GetPerformanceMetricsAsync(Guid merchantId, Guid merchantOwnerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}
