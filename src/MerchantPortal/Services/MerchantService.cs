using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public class MerchantService : IMerchantService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<MerchantService> _logger;

    public MerchantService(IApiClient apiClient, ILogger<MerchantService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Mağazayı getirir.
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Mağaza</returns>
    public async Task<MerchantResponse?> GetMyMerchantAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<MerchantResponse>>(
                "api/v1/merchant/my-merchant",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my merchant info");
            return null;
        }
    }

    /// <summary>
    /// Mağazayı günceller.
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="request">Mağaza güncelleme isteği</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Mağaza</returns>
    public async Task<MerchantResponse?> UpdateMerchantAsync(Guid merchantId, UpdateMerchantRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.PutAsync<ApiResponse<MerchantResponse>>(
                $"api/v1/merchant/{merchantId}",
                request,
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating merchant {MerchantId}", merchantId);
            return null;
        }
    }

    /// <summary>
    /// Mağaza dashboard'ını getirir.
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Mağaza dashboard'ı</returns>
    public async Task<MerchantDashboardResponse?> GetDashboardAsync(Guid merchantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<MerchantDashboardResponse>>(
                $"api/v1/merchants/{merchantId}/merchantdashboard",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard for merchant {MerchantId}", merchantId);
            return null;
        }
    }

    /// <summary>
    /// Son siparişleri getirir.
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="limit">Limit</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Son siparişler</returns>
    public async Task<List<RecentOrderResponse>?> GetRecentOrdersAsync(Guid merchantId, int limit = 10, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<List<RecentOrderResponse>>>(
                $"api/v1/merchants/{merchantId}/merchantdashboard/recent-orders?limit={limit}",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent orders for merchant {MerchantId}", merchantId);
            return null;
        }
    }

    /// <summary>
    /// En çok satılan ürünleri getirir.
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="limit">Limit</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>En çok satılan ürünler</returns>
    public async Task<List<TopProductResponse>?> GetTopProductsAsync(Guid merchantId, int limit = 10, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<List<TopProductResponse>>>(
                $"api/v1/merchants/{merchantId}/merchantdashboard/top-products?limit={limit}",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top products for merchant {MerchantId}", merchantId);
            return null;
        }
    }
}

