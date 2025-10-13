using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public class CategoryService : ICategoryService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IApiClient apiClient, ILogger<CategoryService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<List<ProductCategoryResponse>?> GetCategoriesAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<List<ProductCategoryResponse>>>(
                "api/v1/productcategory",
                ct);

            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return null;
        }
    }

    public async Task<List<ProductCategoryResponse>?> GetMyCategoriesAsync(CancellationToken ct = default)
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
            _logger.LogError(ex, "Error getting my categories");
            return null;
        }
    }

    public async Task<ProductCategoryResponse?> GetCategoryByIdAsync(Guid categoryId, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<ProductCategoryResponse>>(
                $"api/v1/productcategory/{categoryId}",
                ct);

            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {CategoryId}", categoryId);
            return null;
        }
    }

    public async Task<ProductCategoryResponse?> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.PostAsync<ApiResponse<ProductCategoryResponse>>(
                "api/v1/productcategory",
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

    public async Task<ProductCategoryResponse?> UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.PutAsync<ApiResponse<ProductCategoryResponse>>(
                $"api/v1/productcategory/{categoryId}",
                request,
                ct);

            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", categoryId);
            return null;
        }
    }

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

