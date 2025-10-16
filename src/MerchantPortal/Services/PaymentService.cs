using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public class PaymentService : IPaymentService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IApiClient apiClient, ILogger<PaymentService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Nakit özeti getirir.
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Nakit özeti</returns>
    public async Task<MerchantCashSummaryResponse?> GetCashSummaryAsync(
        Guid merchantId, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        CancellationToken ct = default)
    {
        try
        {
            var endpoint = $"api/v1/payment/merchant/summary?merchantId={merchantId}";
            
            if (startDate.HasValue)
                endpoint += $"&startDate={startDate.Value:yyyy-MM-dd}";
            
            if (endDate.HasValue)
                endpoint += $"&endDate={endDate.Value:yyyy-MM-dd}";

            var response = await _apiClient.GetAsync<ApiResponse<MerchantCashSummaryResponse>>(endpoint, ct);
            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cash summary for merchant {MerchantId}", merchantId);
            return null;
        }
    }

    /// <summary>
    /// Ödemeleri getirir.
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Ödemeler</returns>
    public async Task<PagedResult<SettlementResponse>?> GetSettlementsAsync(
        Guid merchantId, 
        int page = 1, 
        int pageSize = 20, 
        CancellationToken ct = default)
    {
        try
        {
            var endpoint = $"api/v1/payment/merchant/settlements?merchantId={merchantId}&page={page}&pageSize={pageSize}";
            var response = await _apiClient.GetAsync<ApiResponse<PagedResult<SettlementResponse>>>(endpoint, ct);
            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settlements for merchant {MerchantId}", merchantId);
            return null;
        }
    }

    /// <summary>
    /// Ödeme istatistiklerini getirir.
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Ödeme istatistikleri</returns>
    public async Task<PaymentStatisticsResponse?> GetPaymentStatisticsAsync(
        Guid merchantId, 
        CancellationToken ct = default)
    {
        try
        {
            // This might need a custom endpoint in backend
            // For now, we'll calculate from cash summary
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek + 1);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var todaySummary = await GetCashSummaryAsync(merchantId, today, today.AddDays(1), ct);
            var weekSummary = await GetCashSummaryAsync(merchantId, weekStart, today.AddDays(1), ct);
            var monthSummary = await GetCashSummaryAsync(merchantId, monthStart, today.AddDays(1), ct);

            var stats = new PaymentStatisticsResponse
            {
                TodayRevenue = todaySummary?.TotalAmount ?? 0,
                TodayPayments = todaySummary?.TotalOrders ?? 0,
                WeekRevenue = weekSummary?.TotalAmount ?? 0,
                WeekPayments = weekSummary?.TotalOrders ?? 0,
                MonthRevenue = monthSummary?.TotalAmount ?? 0,
                MonthPayments = monthSummary?.TotalOrders ?? 0,
                PendingSettlement = monthSummary?.NetAmount ?? 0,
                TotalCommission = monthSummary?.TotalCommission ?? 0
            };

            // Payment method breakdown from month summary
            if (monthSummary?.Payments != null)
            {
                stats.PaymentMethodBreakdown = monthSummary.Payments
                    .GroupBy(p => p.PaymentMethod)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment statistics for merchant {MerchantId}", merchantId);
            return null;
        }
    }
}

