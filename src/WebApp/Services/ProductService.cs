using WebApp.Models;

namespace WebApp.Services;

public interface IProductService
{
    Task<List<ProductResponse>> GetPopularProductsAsync(int limit = 10);
    Task<ProductResponse?> GetProductByIdAsync(Guid productId);
    Task<List<ProductResponse>> GetSimilarProductsAsync(Guid productId, int limit = 4);
    Task<List<ProductResponse>> GetFavoriteProductsAsync();
    Task<bool> AddToFavoritesAsync(Guid productId);
    Task<bool> RemoveFromFavoritesAsync(Guid productId);
    Task<bool> IsFavoriteAsync(Guid productId);
}

public class ProductService : IProductService
{
    private readonly ApiClient _apiClient;
    private readonly IAuthService _authService;
    private readonly IAdvancedCacheService _cache;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ApiClient apiClient, IAuthService authService, IAdvancedCacheService cache, ILogger<ProductService> logger)
    {
        _apiClient = apiClient;
        _authService = authService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<ProductResponse>> GetPopularProductsAsync(int limit = 10)
    {
        var cacheKey = $"cache:products:popular:{limit}";
        var cached = await _cache.GetAsync<List<ProductResponse>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var response = await _apiClient.GetAsync<List<ProductResponse>>($"api/v1/product/popular?limit={limit}");
        var data = response.IsSuccess ? response.Data ?? new List<ProductResponse>() : new List<ProductResponse>();
        if (data.Count > 0)
        {
            await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(5));
        }
        return data;
    }

    public async Task<ProductResponse?> GetProductByIdAsync(Guid productId)
    {
        var cacheKey = $"cache:products:byid:{productId}";
        var cached = await _cache.GetAsync<ProductResponse>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var response = await _apiClient.GetAsync<ProductResponse>($"api/v1/product/{productId}");
        var data = response.IsSuccess ? response.Data : null;
        if (data != null)
        {
            await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(10));
        }
        return data;
    }

    public async Task<List<ProductResponse>> GetSimilarProductsAsync(Guid productId, int limit = 4)
    {
        var cacheKey = $"cache:products:similar:{productId}:{limit}";
        var cached = await _cache.GetAsync<List<ProductResponse>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var response = await _apiClient.GetAsync<List<ProductResponse>>($"api/v1/product/{productId}/similar?limit={limit}");
        var data = response.IsSuccess ? response.Data ?? new List<ProductResponse>() : new List<ProductResponse>();
        if (data.Count > 0)
        {
            await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(10));
        }
        return data;
    }

    public async Task<List<ProductResponse>> GetFavoriteProductsAsync()
    {
        var token = await _authService.GetTokenAsync();
        var response = await _apiClient.GetAsync<List<ProductResponse>>("api/v1/user/favorites", token);
        return response.IsSuccess ? response.Data ?? new List<ProductResponse>() : new List<ProductResponse>();
    }

    public async Task<bool> AddToFavoritesAsync(Guid productId)
    {
        var token = await _authService.GetTokenAsync();
        var response = await _apiClient.PostAsync($"api/v1/user/favorites/{productId}", null, token);
        return response.IsSuccess;
    }

    public async Task<bool> RemoveFromFavoritesAsync(Guid productId)
    {
        var token = await _authService.GetTokenAsync();
        var response = await _apiClient.DeleteAsync($"api/v1/user/favorites/{productId}", token);
        return response.IsSuccess;
    }

    public async Task<bool> IsFavoriteAsync(Guid productId)
    {
        var token = await _authService.GetTokenAsync();
        var response = await _apiClient.GetAsync<bool>($"api/v1/user/favorites/{productId}/status", token);
        return response.IsSuccess && response.Data;
    }
}
