using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

/// <summary>
/// Product review servisi implementasyonu
/// </summary>
public class ProductReviewService : IProductReviewService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<ProductReviewService> _logger;

    public ProductReviewService(IApiClient apiClient, ILogger<ProductReviewService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Merchant'ın tüm ürünlerine gelen yorumları getirir.
    /// </summary>
    public async Task<PagedResult<ProductReviewResponse>?> GetMerchantProductReviewsAsync(
        Guid merchantId, 
        int page = 1, 
        int pageSize = 20,
        int? rating = null,
        bool? isApproved = null,
        CancellationToken ct = default)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (rating.HasValue)
                queryParams.Add($"rating={rating.Value}");

            if (isApproved.HasValue)
                queryParams.Add($"isApproved={isApproved.Value}");

            var query = string.Join("&", queryParams);
            
            var response = await _apiClient.GetAsync<ApiResponse<PagedResult<ProductReviewResponse>>>(
                $"api/v1/productreview/merchant/{merchantId}?{query}",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchant product reviews for {MerchantId}", merchantId);
            return null;
        }
    }

    /// <summary>
    /// Belirli bir ürünün yorumlarını getirir.
    /// </summary>
    public async Task<PagedResult<ProductReviewResponse>?> GetProductReviewsAsync(
        Guid productId, 
        int page = 1, 
        int pageSize = 20,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<PagedResult<ProductReviewResponse>>>(
                $"api/v1/productreview/product/{productId}?page={page}&pageSize={pageSize}",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product reviews for {ProductId}", productId);
            return null;
        }
    }

    /// <summary>
    /// Review detayını getirir.
    /// </summary>
    public async Task<ProductReviewResponse?> GetReviewByIdAsync(Guid reviewId, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<ProductReviewResponse>>(
                $"api/v1/productreview/{reviewId}",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review {ReviewId}", reviewId);
            return null;
        }
    }

    /// <summary>
    /// Ürün review istatistiklerini getirir.
    /// </summary>
    public async Task<ProductReviewStatsResponse?> GetProductReviewStatsAsync(Guid productId, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<ProductReviewStatsResponse>>(
                $"api/v1/productreview/product/{productId}/stats",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product review stats for {ProductId}", productId);
            return null;
        }
    }

    /// <summary>
    /// Merchant'ın tüm ürünleri için review istatistiklerini getirir.
    /// </summary>
    public async Task<ProductReviewStatsResponse?> GetMerchantReviewStatsAsync(Guid merchantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<ProductReviewStatsResponse>>(
                $"api/v1/productreview/merchant/{merchantId}/stats",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchant review stats for {MerchantId}", merchantId);
            return null;
        }
    }

    /// <summary>
    /// Review'a merchant yanıtı ekler.
    /// </summary>
    public async Task<bool> RespondToReviewAsync(Guid reviewId, string response, CancellationToken ct = default)
    {
        try
        {
            var request = new RespondToReviewRequest { Response = response };
            
            var apiResponse = await _apiClient.PutAsync<ApiResponse<ProductReviewResponse>>(
                $"api/v1/productreview/{reviewId}/respond",
                request,
                ct);

            return apiResponse?.isSuccess == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to review {ReviewId}", reviewId);
            return false;
        }
    }

    /// <summary>
    /// Review'ı onaylar.
    /// </summary>
    public async Task<bool> ApproveReviewAsync(Guid reviewId, CancellationToken ct = default)
    {
        try
        {
            var apiResponse = await _apiClient.PutAsync<ApiResponse<object>>(
                $"api/v1/productreview/{reviewId}/approve",
                null,
                ct);

            return apiResponse?.isSuccess == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving review {ReviewId}", reviewId);
            return false;
        }
    }

    /// <summary>
    /// Review'ı reddeder.
    /// </summary>
    public async Task<bool> RejectReviewAsync(Guid reviewId, string reason, CancellationToken ct = default)
    {
        try
        {
            var apiResponse = await _apiClient.PutAsync<ApiResponse<object>>(
                $"api/v1/productreview/{reviewId}/reject",
                new { Reason = reason },
                ct);

            return apiResponse?.isSuccess == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting review {ReviewId}", reviewId);
            return false;
        }
    }
}

