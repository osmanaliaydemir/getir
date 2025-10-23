using WebApp.Models;

namespace WebApp.Services
{
    public class MerchantService
    {
        private readonly ApiClient _apiClient;

        public MerchantService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<MerchantResponse>> GetMerchantsAsync(int page = 1, int pageSize = 20)
        {
            var response = await _apiClient.GetAsync<List<MerchantResponse>>($"api/v1/merchant?page={page}&pageSize={pageSize}");
            return response.IsSuccess ? response.Data ?? new List<MerchantResponse>() : new List<MerchantResponse>();
        }

        public async Task<MerchantResponse?> GetMerchantByIdAsync(Guid merchantId)
        {
            var response = await _apiClient.GetAsync<MerchantResponse>($"api/v1/merchant/{merchantId}");
            return response.IsSuccess ? response.Data : null;
        }

        public async Task<List<MerchantResponse>> GetMerchantsByCategoryAsync(string category, int limit = 10)
        {
            var response = await _apiClient.GetAsync<List<MerchantResponse>>($"api/v1/merchant/category/{category}?limit={limit}");
            return response.IsSuccess ? response.Data ?? new List<MerchantResponse>() : new List<MerchantResponse>();
        }

        public async Task<List<ProductResponse>> GetMerchantProductsAsync(Guid merchantId, int page = 1, int pageSize = 20)
        {
            var response = await _apiClient.GetAsync<List<ProductResponse>>($"api/v1/merchant/{merchantId}/products?page={page}&pageSize={pageSize}");
            return response.IsSuccess ? response.Data ?? new List<ProductResponse>() : new List<ProductResponse>();
        }

        public async Task<List<MerchantResponse>> SearchMerchantsAsync(string query, int limit = 10)
        {
            var response = await _apiClient.GetAsync<List<MerchantResponse>>($"api/v1/merchant/search?q={Uri.EscapeDataString(query)}&limit={limit}");
            return response.IsSuccess ? response.Data ?? new List<MerchantResponse>() : new List<MerchantResponse>();
        }
    }
}
