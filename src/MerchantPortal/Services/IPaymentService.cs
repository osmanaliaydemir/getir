using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public interface IPaymentService
{
    Task<MerchantCashSummaryResponse?> GetCashSummaryAsync(Guid merchantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
    Task<PagedResult<SettlementResponse>?> GetSettlementsAsync(Guid merchantId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<PaymentStatisticsResponse?> GetPaymentStatisticsAsync(Guid merchantId, CancellationToken ct = default);
}

