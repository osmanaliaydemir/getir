using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace WebApp.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AuthService _authService;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;

    public CustomAuthenticationStateProvider(AuthService authService, ILogger<CustomAuthenticationStateProvider> logger)
    {
        _authService = authService;
        _logger = logger;
        
        // AuthService'deki değişiklikleri dinle
        _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Prerendering sırasında JavaScript interop çağrısı yapma
            // Sadece anonymous user döndür
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authentication state");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    private async void OnAuthenticationStateChanged()
    {
        try
        {
            var authState = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying authentication state change");
        }
    }

    public void Dispose()
    {
        _authService.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
