using WebApp.Models;

namespace WebApp.Services;

public interface IHelpService
{
    Task<PagedResult<HelpArticleResponse>> SearchHelpArticlesAsync(string query, int page = 1, int pageSize = 10);
    Task<bool> RateArticleAsync(int articleId, bool isHelpful);
    Task<List<HelpCategoryResponse>> GetHelpCategoriesAsync();
    Task<List<HelpArticleResponse>> GetPopularArticlesAsync(int limit = 5);
}

public class HelpService : IHelpService
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    public HelpService(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    public async Task<PagedResult<HelpArticleResponse>> SearchHelpArticlesAsync(string query, int page = 1, int pageSize = 10)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiClient.GetAsync<PagedResult<HelpArticleResponse>>(
                $"api/v1/help/articles/search?query={Uri.EscapeDataString(query)}&page={page}&pageSize={pageSize}", 
                token);
            
            return response.IsSuccess ? response.Data ?? new PagedResult<HelpArticleResponse>() : new PagedResult<HelpArticleResponse>();
        }
        catch
        {
            // Backend hazır değilse boş sonuç döndür
            return new PagedResult<HelpArticleResponse>();
        }
    }

    public async Task<bool> RateArticleAsync(int articleId, bool isHelpful)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var request = new RateArticleRequest { ArticleId = articleId, IsHelpful = isHelpful };
            var response = await _apiClient.PostAsync($"api/v1/help/articles/{articleId}/rate", request, token);
            return response.IsSuccess;
        }
        catch
        {
            // Backend hazır değilse false döndür
            return false;
        }
    }

    public async Task<List<HelpCategoryResponse>> GetHelpCategoriesAsync()
    {
        try
        {
            var response = await _apiClient.GetAsync<List<HelpCategoryResponse>>("api/v1/help/categories");
            return response.IsSuccess ? response.Data ?? new List<HelpCategoryResponse>() : new List<HelpCategoryResponse>();
        }
        catch
        {
            return new List<HelpCategoryResponse>();
        }
    }

    public async Task<List<HelpArticleResponse>> GetPopularArticlesAsync(int limit = 5)
    {
        try
        {
            var response = await _apiClient.GetAsync<List<HelpArticleResponse>>($"api/v1/help/articles/popular?limit={limit}");
            return response.IsSuccess ? response.Data ?? new List<HelpArticleResponse>() : new List<HelpArticleResponse>();
        }
        catch
        {
            return new List<HelpArticleResponse>();
        }
    }
}

