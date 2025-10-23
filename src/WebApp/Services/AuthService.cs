using WebApp.Models;
using Microsoft.JSInterop;

namespace WebApp.Services;

public class AuthService
{
    private readonly ApiClient _apiClient;
    private readonly IJSRuntime _jsRuntime;
    private const string TOKEN_KEY = "auth_token";
    private const string USER_KEY = "user_data";

    public event Action? AuthenticationStateChanged;

    public AuthService(ApiClient apiClient, IJSRuntime jsRuntime)
    {
        _apiClient = apiClient;
        _jsRuntime = jsRuntime;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _apiClient.PostAsync<AuthResponse>("api/v1/auth/login", request);
        
        if (response.IsSuccess && response.Data != null && !string.IsNullOrEmpty(response.Data.AccessToken))
        {
            await SaveAuthDataAsync(response.Data);
            AuthenticationStateChanged?.Invoke();
            return response.Data;
        }
        
        return null;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var response = await _apiClient.PostAsync<AuthResponse>("api/v1/auth/register", request);
        
        if (response.IsSuccess && response.Data != null && !string.IsNullOrEmpty(response.Data.AccessToken))
        {
            await SaveAuthDataAsync(response.Data);
            return response.Data;
        }
        
        return null;
    }

    public async Task<AuthResponse?> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var response = await _apiClient.PostAsync<AuthResponse>("api/v1/auth/forgot-password", request);
        
        if (response.IsSuccess && response.Data != null)
        {
            return response.Data;
        }
        
        return null;
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", USER_KEY);
        AuthenticationStateChanged?.Invoke();
    }

    public async Task<AuthResponse?> GetCurrentUserAsync()
    {
        try
        {
            var userJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", USER_KEY);
            if (string.IsNullOrEmpty(userJson))
                return null;

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            
            return System.Text.Json.JsonSerializer.Deserialize<AuthResponse>(userJson, options);
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TOKEN_KEY);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    private async Task SaveAuthDataAsync(AuthResponse authData)
    {
        // Sadece token'ı auth_token key'ine kaydet
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, authData.AccessToken);
        
        // Kullanıcı datasını user_data key'ine kaydet
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", USER_KEY, System.Text.Json.JsonSerializer.Serialize(authData));
    }
}
