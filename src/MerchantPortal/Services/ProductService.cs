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

    /// <summary>
    /// Ürünleri getirir.
    /// </summary>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Ürünler</returns>
    public async Task<PagedResult<ProductResponse>?> GetProductsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<PagedResult<ProductResponse>>>(
                $"api/v1/merchants/merchantproduct?page={page}&pageSize={pageSize}",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products");
            return null;
        }
    }

    /// <summary>
    /// Ürün detaylarını getirir.
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Ürün detayları</returns>
    public async Task<ProductResponse?> GetProductByIdAsync(Guid productId, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<ProductResponse>>(
                $"api/v1/product/{productId}",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", productId);
            return null;
        }
    }

    /// <summary>
    /// Ürün oluşturur.
    /// </summary>
    /// <param name="request">Ürün oluşturma isteği</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Ürün</returns>
    public async Task<ProductResponse?> CreateProductAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.PostAsync<ApiResponse<ProductResponse>>(
                "api/v1/merchants/merchantproduct",
                request,
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return null;
        }
    }

    /// <summary>
    /// Ürünü günceller.
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="request">Ürün güncelleme isteği</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Ürün</returns>
    public async Task<ProductResponse?> UpdateProductAsync(Guid productId, UpdateProductRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.PutAsync<ApiResponse<ProductResponse>>(
                $"api/v1/merchants/merchantproduct/{productId}",
                request,
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", productId);
            return null;
        }
    }

    /// <summary>
    /// Ürünü siler.
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Başarılı olup olmadığı</returns>
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

    /// <summary>
    /// Ürün kategorilerini getirir.
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Ürün kategorileri</returns>
    public async Task<List<ProductCategoryResponse>?> GetCategoriesAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<List<ProductCategoryResponse>>>(
                "api/v1/productcategory/my-categories",
                ct);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return null;
        }
    }
}

