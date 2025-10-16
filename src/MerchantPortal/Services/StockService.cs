using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public class StockService : IStockService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<StockService> _logger;

    public StockService(IApiClient apiClient, ILogger<StockService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Stok uyarılarını getirir.
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Stok uyarıları</returns>
    public async Task<List<StockAlertResponse>?> GetStockAlertsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<List<StockAlertResponse>>>(
                "api/StockManagement/alerts",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock alerts");
            return null;
        }
    }

    /// <summary>
    /// Stok özetini getirir.
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Stok özeti</returns>
    public async Task<StockSummaryResponse?> GetStockSummaryAsync(CancellationToken ct = default)
    {
        try
        {
            // We'll calculate this from alerts and products
            var alerts = await GetStockAlertsAsync(ct);
            
            if (alerts == null)
                return null;

            return new StockSummaryResponse
            {
                ActiveAlerts = alerts.Count(a => !a.IsResolved),
                LowStockItems = alerts.Count(a => a.AlertType == "LowStock" && !a.IsResolved),
                OutOfStockItems = alerts.Count(a => a.AlertType == "OutOfStock" && !a.IsResolved),
                OverstockItems = alerts.Count(a => a.AlertType == "Overstock" && !a.IsResolved)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock summary");
            return null;
        }
    }

    /// <summary>
    /// Stok geçmişini getirir.
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="fromDate">Başlangıç tarihi</param>
    /// <param name="toDate">Bitiş tarihi</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Stok geçmişi</returns>
    public async Task<List<StockHistoryResponse>?> GetStockHistoryAsync(
        Guid productId, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken ct = default)
    {
        try
        {
            var endpoint = $"api/StockManagement/history/{productId}";
            var queryParams = new List<string>();

            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            
            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

            if (queryParams.Any())
                endpoint += "?" + string.Join("&", queryParams);

            var response = await _apiClient.GetAsync<ApiResponse<List<StockHistoryResponse>>>(
                endpoint,
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock history for product {ProductId}", productId);
            return null;
        }
    }

    /// <summary>
    /// Stok seviyesini günceller.
    /// </summary>
    /// <param name="request">Stok seviyesi güncelleme isteği</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Başarılı olup olmadığı</returns>
    public async Task<bool> UpdateStockLevelAsync(UpdateStockRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.PutAsync<ApiResponse<object>>(
                "api/StockManagement/update",
                request,
                ct);

            return response?.isSuccess == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock level for product {ProductId}", request.ProductId);
            return false;
        }
    }

    /// <summary>
    /// Stok seviyelerini bulk günceller.
    /// </summary>
    /// <param name="request">Stok seviyeleri bulk güncelleme isteği</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Başarılı olup olmadığı</returns>
    public async Task<bool> BulkUpdateStockLevelsAsync(BulkUpdateStockRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.PutAsync<ApiResponse<object>>(
                "api/StockManagement/bulk-update",
                request,
                ct);

            return response?.isSuccess == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating stock levels");
            return false;
        }
    }

    /// <summary>
    /// Stok uyarısını çözümler.
    /// </summary>
    /// <param name="alertId">Stok uyarısı ID</param>
    /// <param name="resolutionNotes">Çözüm notları</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Başarılı olup olmadığı</returns>
    public async Task<bool> ResolveStockAlertAsync(Guid alertId, string resolutionNotes, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.PutAsync<ApiResponse<object>>(
                $"api/StockAlert/{alertId}/resolve",
                new { ResolutionNotes = resolutionNotes },
                ct);

            return response?.isSuccess == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving stock alert {AlertId}", alertId);
            return false;
        }
    }

    /// <summary>
    /// Stok seviyelerini kontrol eder.
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Başarılı olup olmadığı</returns>
    public async Task<bool> CheckStockLevelsAsync(CancellationToken ct = default)
    {
        try
        {
            // This endpoint requires merchantId in route, but backend gets it from JWT
            // We'll call the StockAlert endpoint to create low stock alerts
            var response = await _apiClient.PostAsync<ApiResponse<object>>(
                "api/StockAlert/create-low-stock",
                null,
                ct);

            return response?.isSuccess == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock levels");
            return false;
        }
    }
}

