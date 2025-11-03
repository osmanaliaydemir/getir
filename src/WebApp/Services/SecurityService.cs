using WebApp.Models;

namespace WebApp.Services;

public interface ISecurityService
{
    Task<SecurityInfoResponse?> GetSecurityInfoAsync();
    Task<bool> ChangeEmailAsync(ChangeEmailRequest request);
    Task<bool> ChangePhoneAsync(ChangePhoneRequest request);
    Task<bool> ToggleTwoFactorAsync(bool enabled);
    Task<bool> LogoutDeviceAsync(Guid sessionId);
    Task<bool> LogoutAllDevicesAsync();
}

public class SecurityService : ISecurityService
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    public SecurityService(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    public async Task<SecurityInfoResponse?> GetSecurityInfoAsync()
    {
        var token = await _authService.GetTokenAsync();
        var response = await _apiClient.GetAsync<SecurityInfoResponse>("api/v1/user/security/info", token);
        return response.IsSuccess ? response.Data : null;
    }

    public async Task<bool> ChangeEmailAsync(ChangeEmailRequest request)
    {
        var token = await _authService.GetTokenAsync();
        var response = await _apiClient.PostAsync("api/v1/user/security/change-email", request, token);
        return response.IsSuccess;
    }

    public async Task<bool> ChangePhoneAsync(ChangePhoneRequest request)
    {
        var token = await _authService.GetTokenAsync();
        var response = await _apiClient.PostAsync("api/v1/user/security/change-phone", request, token);
        return response.IsSuccess;
    }

    public async Task<bool> ToggleTwoFactorAsync(bool enabled)
    {
        var token = await _authService.GetTokenAsync();
        var request = new ToggleTwoFactorRequest { Enabled = enabled };
        var response = await _apiClient.PostAsync("api/v1/user/security/toggle-2fa", request, token);
        return response.IsSuccess;
    }

    public async Task<bool> LogoutDeviceAsync(Guid sessionId)
    {
        var token = await _authService.GetTokenAsync();
        var response = await _apiClient.PostAsync($"api/v1/user/security/logout-device/{sessionId}", null, token);
        return response.IsSuccess;
    }

    public async Task<bool> LogoutAllDevicesAsync()
    {
        var token = await _authService.GetTokenAsync();
        var response = await _apiClient.PostAsync("api/v1/user/security/logout-all-devices", null, token);
        return response.IsSuccess;
    }
}

