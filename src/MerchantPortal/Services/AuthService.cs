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
            // WebAPI now returns ApiResponse wrapper (BaseController updated)
            var apiResponse = await _apiClient.PostAsync<ApiResponse<LoginResponse>>("api/v1/auth/login", request, ct);

            if (apiResponse?.isSuccess == true && apiResponse.Data != null)
            {
                // Set auth token for future API calls
                _apiClient.SetAuthToken(apiResponse.Data.Token);
                return apiResponse.Data;
            }

            _logger.LogWarning("Login failed for user {Email}. Error: {Error}", 
                request.Email, apiResponse?.Error ?? "Unknown error");
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

    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.PostAsync<ApiResponse<object>>(
                "api/v1/auth/change-password",
                new { CurrentPassword = currentPassword, NewPassword = newPassword },
                ct);

            return response?.isSuccess == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return false;
        }
    }
}

