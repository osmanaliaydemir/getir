using WebApp.Models;

namespace WebApp.Services;

public class ProductService
{
    private readonly ApiClient _apiClient;

    public ProductService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<ProductResponse>> GetPopularProductsAsync(int limit = 10)
    {
        var response = await _apiClient.GetAsync<List<ProductResponse>>($"api/v1/product/popular?limit={limit}");
        return response.IsSuccess ? response.Data ?? new List<ProductResponse>() : new List<ProductResponse>();
    }

    public async Task<ProductResponse?> GetProductByIdAsync(Guid productId)
    {
        var response = await _apiClient.GetAsync<ProductResponse>($"api/v1/product/{productId}");
        return response.IsSuccess ? response.Data : null;
    }

    public async Task<List<ProductResponse>> GetSimilarProductsAsync(Guid productId, int limit = 4)
    {
        var response = await _apiClient.GetAsync<List<ProductResponse>>($"api/v1/product/{productId}/similar?limit={limit}");
        return response.IsSuccess ? response.Data ?? new List<ProductResponse>() : new List<ProductResponse>();
    }
}
