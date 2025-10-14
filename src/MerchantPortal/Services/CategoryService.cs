using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public class CategoryService : ICategoryService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<CategoryService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CategoryService(
        IApiClient apiClient, 
        ILogger<CategoryService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _apiClient = apiClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Get merchant ID from session
    /// </summary>
    private Guid GetMerchantIdFromContext()
    {
        var merchantIdStr = _httpContextAccessor.HttpContext?.Session.GetString("MerchantId");
        _logger.LogInformation("GetMerchantIdFromContext - Session MerchantId: '{MerchantId}'", merchantIdStr ?? "(null)");
        
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
            _logger.LogError("MerchantId not found or invalid in session. Value: '{Value}'", merchantIdStr ?? "(null)");
            throw new InvalidOperationException("MerchantId not found in session");
        }
        
        _logger.LogInformation("MerchantId parsed: {MerchantId}", merchantId);
        return merchantId;
    }
    /// <summary>
    /// Kategorileri getirir.
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Kategoriler</returns>

    public async Task<List<ProductCategoryResponse>?> GetCategoriesAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<List<ProductCategoryResponse>>>("api/v1/productcategory", ct);

            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return null;
        }
    }

    /// <summary>
    /// Mağaza kategorilerini getirir.
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Mağaza kategorileri</returns>
    public async Task<List<ProductCategoryResponse>?> GetMyCategoriesAsync(CancellationToken ct = default)
    {
        try
        {
            // Get merchantId and use the flat list endpoint
            var merchantId = GetMerchantIdFromContext();
            var response = await _apiClient.GetAsync<ApiResponse<List<ProductCategoryResponse>>>(
                $"api/v1/productcategory/merchant/{merchantId}", 
                ct);

            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my categories");
            return null;
        }
    }

    /// <summary>
    /// Kategori detaylarını getirir.
    /// </summary>
    /// <param name="categoryId">Kategori ID</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Kategori detayları</returns>
    public async Task<ProductCategoryResponse?> GetCategoryByIdAsync(Guid categoryId, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<ProductCategoryResponse>>($"api/v1/productcategory/{categoryId}", ct);

            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {CategoryId}", categoryId);
            return null;
        }
    }

    /// <summary>
    /// Kategori oluşturur.
    /// </summary>
    /// <param name="request">Kategori oluşturma isteği</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Kategori</returns>
    public async Task<ProductCategoryResponse?> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        try
        {
            // Get merchantId from session
            var merchantId = GetMerchantIdFromContext();
            var response = await _apiClient.PostAsync<ApiResponse<ProductCategoryResponse>>(
                $"api/v1/productcategory/merchant/{merchantId}", 
                request, 
                ct);
            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return null;
        }
    }

    /// <summary>
    /// Kategori günceller.
    /// </summary>
    /// <param name="categoryId">Kategori ID</param>
    /// <param name="request">Kategori güncelleme isteği</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Kategori</returns>
    public async Task<ProductCategoryResponse?> UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.PutAsync<ApiResponse<ProductCategoryResponse>>($"api/v1/productcategory/{categoryId}", request, ct);

            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", categoryId);
            return null;
        }
    }

    /// <summary>
    /// Kategori siler.
    /// </summary>
    /// <param name="categoryId">Kategori ID</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Başarılı olup olmadığı</returns>
    public async Task<bool> DeleteCategoryAsync(Guid categoryId, CancellationToken ct = default)
    {
        try
        {
            return await _apiClient.DeleteAsync($"api/v1/productcategory/{categoryId}", ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", categoryId);
            return false;
        }
    }

    /// <summary>
    /// Kategori ağacını getirir.
    /// </summary>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Kategori ağacı</returns>
    public async Task<List<CategoryTreeNode>?> GetCategoryTreeAsync(CancellationToken ct = default)
    {
        try
        {
            var categories = await GetMyCategoriesAsync(ct);
            if (categories == null) return null;

            return BuildCategoryTree(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building category tree");
            return null;
        }
    }

    /// <summary>
    /// Kategori ağacını oluşturur.
    /// </summary>
    /// <param name="categories">Kategoriler</param>
    /// <returns>Kategori ağacı</returns>
    private List<CategoryTreeNode> BuildCategoryTree(List<ProductCategoryResponse> categories)
    {
        // Build hierarchical tree structure
        var categoryDict = categories.ToDictionary(c => c.Id, c => new CategoryTreeNode
        {
            Category = c,
            Children = new List<CategoryTreeNode>()
        });

        var rootNodes = new List<CategoryTreeNode>();

        foreach (var node in categoryDict.Values)
        {
            if (node.Category.ParentCategoryId.HasValue &&
                categoryDict.TryGetValue(node.Category.ParentCategoryId.Value, out var parentNode))
            {
                parentNode.Children.Add(node);
            }
            else
            {
                rootNodes.Add(node);
            }
        }

        return rootNodes;
    }
}

