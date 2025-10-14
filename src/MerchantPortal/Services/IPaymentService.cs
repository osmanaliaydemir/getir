using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public interface IPaymentService
{
    /// <summary>
    /// Nakit özeti getirir.
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Nakit özeti</returns>
    Task<MerchantCashSummaryResponse?> GetCashSummaryAsync(Guid merchantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default);
    /// <summary>
    /// Ödemeleri getirir.
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Ödemeler</returns>
    Task<PagedResult<SettlementResponse>?> GetSettlementsAsync(Guid merchantId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    /// <summary>
    /// Ödeme istatistiklerini getirir.
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Ödeme istatistikleri</returns>
    Task<PaymentStatisticsResponse?> GetPaymentStatisticsAsync(Guid merchantId, CancellationToken ct = default);
}

