using WebApp.Models;
using System.Linq;

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
            try
            {
                var url = $"api/v1/merchant?page={page}&pageSize={pageSize}";
                var response = await _apiClient.GetAsync<PagedResult<MerchantResponse>>(url);

                if (!response.IsSuccess)
                {
                    Console.WriteLine($"❌ API Error: {response.Error}");
                }

                var result = response.IsSuccess && response.Data != null ? response.Data.Items
                             ?? new List<MerchantResponse>()
                             : new List<MerchantResponse>();

                return result;
            }
            catch (Exception)
            {
                return new List<MerchantResponse>();
            }
        }

        public async Task<MerchantResponse?> GetMerchantByIdAsync(Guid merchantId)
        {
            var response = await _apiClient.GetAsync<MerchantResponse>($"api/v1/merchant/{merchantId}");
            return response.IsSuccess ? response.Data : null;
        }

        public async Task<List<MerchantResponse>> GetMerchantsByCategoryAsync(string category, int limit = 10)
        {
            var response = await _apiClient.GetAsync<PagedResult<MerchantResponse>>($"api/v1/merchant/category/{category}?limit={limit}");
            return response.IsSuccess && response.Data != null
                ? response.Data.Items ?? new List<MerchantResponse>()
                : new List<MerchantResponse>();
        }

        public async Task<List<ProductResponse>> GetMerchantProductsAsync(Guid merchantId, int page = 1, int pageSize = 20)
        {
            Console.WriteLine($"🔍 MerchantService.GetMerchantProductsAsync çağrıldı - MerchantId: {merchantId}, Page: {page}, PageSize: {pageSize}");

            try
            {
                var url = $"api/v1/merchant/{merchantId}/products?page={page}&pageSize={pageSize}";
                Console.WriteLine($"📡 API URL: {url}");

                var response = await _apiClient.GetAsync<PagedResult<ProductResponse>>(url);

                Console.WriteLine($"📊 API Response - Success: {response.IsSuccess}");
                Console.WriteLine($"📊 API Response - Data Items Count: {response.Data?.Items?.Count ?? 0}");

                if (!response.IsSuccess)
                {
                    Console.WriteLine($"❌ API Error: {response.Error}");
                }

                var result = response.IsSuccess && response.Data != null
                    ? response.Data.Items ?? new List<ProductResponse>()
                    : new List<ProductResponse>();
                Console.WriteLine($"✅ Returning {result.Count} products");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Exception in GetMerchantProductsAsync: {ex.Message}");
                Console.WriteLine($"💥 Stack Trace: {ex.StackTrace}");
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
