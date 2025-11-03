using WebApp.Models;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace WebApp.Services
{
    public interface IMerchantService
    {
        Task<List<MerchantResponse>> GetMerchantsAsync(int page = 1, int pageSize = 20);
        Task<MerchantResponse?> GetMerchantByIdAsync(Guid merchantId);
        Task<List<MerchantResponse>> GetMerchantsByCategoryAsync(string category, int limit = 10);
        Task<List<ProductResponse>> GetMerchantProductsAsync(Guid merchantId, int page = 1, int pageSize = 20);
        Task<List<MerchantResponse>> SearchMerchantsAsync(string query, int limit = 10);
    }

    public class MerchantService : IMerchantService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<MerchantService> _logger;
        private readonly IAdvancedCacheService _cache;

        public MerchantService(ApiClient apiClient, ILogger<MerchantService> logger, IAdvancedCacheService cache)
        {
            _apiClient = apiClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<MerchantResponse>> GetMerchantsAsync(int page = 1, int pageSize = 20)
        {
            try
            {
                var url = $"api/v1/merchant?page={page}&pageSize={pageSize}";
                var cacheKey = $"cache:merchants:list:{page}:{pageSize}";

                var cached = await _cache.GetAsync<List<MerchantResponse>>(cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                var response = await _apiClient.GetAsync<PagedResult<MerchantResponse>>(url);

                if (!response.IsSuccess)
                {
                    _logger.LogWarning("API Error in GetMerchantsAsync: {Error}", response.Error);
                }

                var result = response.IsSuccess && response.Data != null ? response.Data.Items
                             ?? new List<MerchantResponse>()
                             : new List<MerchantResponse>();

                if (result.Count > 0)
                {
                    await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));
                }

                return result;
            }
            catch (Exception)
            {
                return new List<MerchantResponse>();
            }
        }

        public async Task<MerchantResponse?> GetMerchantByIdAsync(Guid merchantId)
        {
            var cacheKey = $"cache:merchants:byid:{merchantId}";
            var cached = await _cache.GetAsync<MerchantResponse>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var response = await _apiClient.GetAsync<MerchantResponse>($"api/v1/merchant/{merchantId}");
            var data = response.IsSuccess ? response.Data : null;
            if (data != null)
            {
                await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(10));
            }
            return data;
        }

        public async Task<List<MerchantResponse>> GetMerchantsByCategoryAsync(string category, int limit = 10)
        {
            var cacheKey = $"cache:merchants:category:{category}:{limit}";
            var cached = await _cache.GetAsync<List<MerchantResponse>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var response = await _apiClient.GetAsync<PagedResult<MerchantResponse>>($"api/v1/merchant/category/{category}?limit={limit}");
            var data = response.IsSuccess && response.Data != null
                ? response.Data.Items ?? new List<MerchantResponse>()
                : new List<MerchantResponse>();

            if (data.Count > 0)
            {
                await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(5));
            }
            return data;
        }

        public async Task<List<ProductResponse>> GetMerchantProductsAsync(Guid merchantId, int page = 1, int pageSize = 20)
        {
            _logger.LogDebug("MerchantService.GetMerchantProductsAsync called - MerchantId: {MerchantId}, Page: {Page}, PageSize: {PageSize}", merchantId, page, pageSize);

            try
            {
                var url = $"api/v1/merchant/{merchantId}/products?page={page}&pageSize={pageSize}";
                var cacheKey = $"cache:merchants:{merchantId}:products:{page}:{pageSize}";

                var cached = await _cache.GetAsync<List<ProductResponse>>(cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                var response = await _apiClient.GetAsync<PagedResult<ProductResponse>>(url);

                _logger.LogDebug("API Response - Success: {IsSuccess}, Items Count: {Count}", response.IsSuccess, response.Data?.Items?.Count ?? 0);

                if (!response.IsSuccess)
                {
                    _logger.LogWarning("API Error for GetMerchantProductsAsync: {Error}", response.Error);
                }

                var result = response.IsSuccess && response.Data != null
                    ? response.Data.Items ?? new List<ProductResponse>()
                    : new List<ProductResponse>();
                
                if (result.Count > 0)
                {
                    await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));
                }

                _logger.LogDebug("Returning {Count} products for merchant {MerchantId}", result.Count, merchantId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetMerchantProductsAsync for merchant {MerchantId}", merchantId);
                return new List<ProductResponse>();
            }
        }

        public async Task<List<MerchantResponse>> SearchMerchantsAsync(string query, int limit = 10)
        {
            var response = await _apiClient.GetAsync<PagedResult<MerchantResponse>>($"api/v1/merchant/search?q={Uri.EscapeDataString(query)}&limit={limit}");
            return response.IsSuccess && response.Data != null
                ? response.Data.Items ?? new List<MerchantResponse>()
                : new List<MerchantResponse>();
        }
    }
}
