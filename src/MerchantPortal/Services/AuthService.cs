using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public class AuthService : IAuthService
{
    private readonly IApiClient _apiClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IApiClient apiClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthService> logger)
    {
        _apiClient = apiClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        try
        {
            // API direkt LoginResponse dönüyor (ApiResponse wrapper yok)
            var response = await _apiClient.PostAsync<LoginResponse>(
                "api/v1/auth/login",
                request,
                ct);

            if (response != null)
            {
                // Set auth token for future API calls
                _apiClient.SetAuthToken(response.Token);
                return response;
            }

            _logger.LogWarning("Login failed for user {Email}", request.Email);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", request.Email);
            return null;
        }
    }

    public Task LogoutAsync()
    {
        _apiClient.ClearAuthToken();
        return Task.CompletedTask;
    }
}

