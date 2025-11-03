using WebApp.Models;
using Microsoft.JSInterop;

namespace WebApp.Services;

public interface IAuthService
{
    event Action? AuthenticationStateChanged;
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task LogoutAsync();
    Task<AuthResponse?> GetCurrentUserAsync();
    Task<string?> GetTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task<string?> GetCsrfTokenAsync();
    Task<bool> RefreshTokenAsync();
    Task<bool> IsAuthenticatedAsync();
}

public class AuthService : IAuthService
{
    private readonly ApiClient _apiClient;
    private readonly IJSRuntime _jsRuntime;
    private System.Threading.Timer? _refreshTimer;
    private const string TOKEN_KEY = "auth_token";
    private const string USER_KEY = "user_data";
    private const string CSRF_KEY = "csrf_token";

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
            ScheduleProactiveRefresh(response.Data.ExpiresAt);
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
            ScheduleProactiveRefresh(response.Data.ExpiresAt);
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
        try
        {
            _refreshTimer?.Dispose();
            _refreshTimer = null;
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", USER_KEY);
        }
        catch
        {
            // JavaScript interop hatası durumunda sessizce devam et (prerendering sırasında)
        }
        
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
        catch(Exception ex)
        {
            return null;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            var userJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", USER_KEY);
            if (string.IsNullOrEmpty(userJson)) return null;
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            var user = System.Text.Json.JsonSerializer.Deserialize<AuthResponse>(userJson, options);
            return user?.RefreshToken;
        }
        catch { return null; }
    }

    public async Task<string?> GetCsrfTokenAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", CSRF_KEY);
        }
        catch { return null; }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var payload = new { refreshToken };
            var response = await _apiClient.PostAsync<AuthResponse>("api/v1/auth/refresh", payload);
            if (response.IsSuccess && response.Data != null && !string.IsNullOrEmpty(response.Data.AccessToken))
            {
                await SaveAuthDataAsync(response.Data);
                AuthenticationStateChanged?.Invoke();
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    private async Task SaveAuthDataAsync(AuthResponse authData)
    {
        try
        {
            // Sadece token'ı auth_token key'ine kaydet
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, authData.AccessToken);
            
            // Kullanıcı datasını user_data key'ine kaydet
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", USER_KEY, System.Text.Json.JsonSerializer.Serialize(authData));
        }
        catch
        {
            // JavaScript interop hatası durumunda sessizce devam et (prerendering sırasında)
        }
    }

    private void ScheduleProactiveRefresh(DateTime expiresAt)
    {
        try
        {
            var now = DateTime.UtcNow;
            var due = expiresAt.ToUniversalTime() - now - TimeSpan.FromSeconds(60);
            if (due < TimeSpan.FromSeconds(5))
            {
                due = TimeSpan.FromSeconds(5);
            }
            _refreshTimer?.Dispose();
            _refreshTimer = new System.Threading.Timer(async _ =>
            {
                try
                {
                    var ok = await RefreshTokenAsync();
                    if (!ok)
                    {
                        _refreshTimer?.Dispose();
                        _refreshTimer = null;
                    }
                    else
                    {
                        var user = await GetCurrentUserAsync();
                        if (user != null)
                        {
                            ScheduleProactiveRefresh(user.ExpiresAt);
                        }
                    }
                }
                catch
                {
                    _refreshTimer?.Dispose();
                    _refreshTimer = null;
                }
            }, null, due, Timeout.InfiniteTimeSpan);
        }
        catch { }
    }
}
