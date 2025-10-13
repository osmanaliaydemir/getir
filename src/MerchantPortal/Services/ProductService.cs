using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public class ProductService : IProductService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IApiClient apiClient, ILogger<ProductService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<PagedResult<ProductResponse>?> GetProductsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<PagedResult<ProductResponse>>>(
                $"api/v1/merchants/merchantproduct?page={page}&pageSize={pageSize}",
                ct);

            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products");
            return null;
        }
    }

    public async Task<ProductResponse?> GetProductByIdAsync(Guid productId, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<ProductResponse>>(
                $"api/v1/product/{productId}",
                ct);

            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", productId);
            return null;
        }
    }

    public async Task<ProductResponse?> CreateProductAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.PostAsync<ApiResponse<ProductResponse>>(
                "api/v1/merchants/merchantproduct",
                request,
                ct);

            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return null;
        }
    }

    public async Task<ProductResponse?> UpdateProductAsync(Guid productId, UpdateProductRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.PutAsync<ApiResponse<ProductResponse>>(
                $"api/v1/merchants/merchantproduct/{productId}",
                request,
                ct);

            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", productId);
            return null;
        }
    }

    public async Task<bool> DeleteProductAsync(Guid productId, CancellationToken ct = default)
    {
        try
        {
            return await _apiClient.DeleteAsync($"api/v1/merchants/merchantproduct/{productId}", ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", productId);
            return false;
        }
    }

    public async Task<List<ProductCategoryResponse>?> GetCategoriesAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<List<ProductCategoryResponse>>>(
                "api/v1/productcategory/my-categories",
                ct);

            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return null;
        }
    }
}

