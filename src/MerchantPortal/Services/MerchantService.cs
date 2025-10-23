using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public class MerchantService : IMerchantService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<MerchantService> _logger;

    /// <summary>
    /// MerchantService constructor
    /// </summary>
    /// <param name="apiClient">API client</param>
    /// <param name="logger">Logger instance</param>
    public MerchantService(IApiClient apiClient, ILogger<MerchantService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Mağazayı getir
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Mağaza bilgileri</returns>
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
    /// Mağazayı güncelle
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="request">Güncelleme isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenmiş mağaza bilgileri</returns>
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
    /// Mağaza dashboard'ını getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Dashboard verileri</returns>
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
    /// Son siparişleri getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="limit">Kayıt limiti</param>
    /// <param name="ct">İptal token'ı</param>
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
    /// En çok satılan ürünleri getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="limit">Kayıt limiti</param>
    /// <param name="ct">İptal token'ı</param>
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

    /// <summary>
    /// Satış trend verilerini getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="days">Gün sayısı</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Satış trend verileri</returns>
    public async Task<List<SalesTrendDataResponse>?> GetSalesTrendDataAsync(Guid merchantId, int days = 30, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<List<SalesTrendDataResponse>>>(
                $"api/v1/merchants/{merchantId}/analytics/sales-trend?days={days}",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales trend for merchant {MerchantId}", merchantId);
            return null;
        }
    }

    /// <summary>
    /// Sipariş durumu dağılımını getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sipariş durumu dağılımı</returns>
    public async Task<OrderStatusDistributionResponse?> GetOrderStatusDistributionAsync(Guid merchantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<OrderStatusDistributionResponse>>(
                $"api/v1/merchants/{merchantId}/analytics/order-distribution",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order distribution for merchant {MerchantId}", merchantId);
            return null;
        }
    }

    /// <summary>
    /// Kategori performansını getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kategori performans verileri</returns>
    public async Task<List<CategoryPerformanceResponse>?> GetCategoryPerformanceAsync(Guid merchantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<List<CategoryPerformanceResponse>>>(
                $"api/v1/merchants/{merchantId}/analytics/category-performance",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category performance for merchant {MerchantId}", merchantId);
            return null;
        }
    }
}

