using WebApp.Models;

namespace WebApp.Services;

public interface INotificationService
{
    Task<List<NotificationResponse>> GetUserNotificationsAsync();
    Task<bool> MarkAsReadAsync(Guid notificationId);
    Task<bool> MarkAllAsReadAsync();
    Task<bool> DeleteNotificationAsync(Guid notificationId);
    Task<bool> ClearAllNotificationsAsync();
}

public class NotificationService : INotificationService
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    public NotificationService(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    public async Task<List<NotificationResponse>> GetUserNotificationsAsync()
    {
        var token = await _authService.GetTokenAsync();
        var response = await _apiClient.GetAsync<List<NotificationResponse>>("api/v1/user/notifications", token);
        return response.IsSuccess ? response.Data ?? new List<NotificationResponse>() : new List<NotificationResponse>();
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId)
    {
        var token = await _authService.GetTokenAsync();
        var response = await _apiClient.PostAsync($"api/v1/user/notifications/{notificationId}/mark-read", null, token);
        return response.IsSuccess;
    }

    public async Task<bool> MarkAllAsReadAsync()
    {
        var token = await _authService.GetTokenAsync();
        var response = await _apiClient.PostAsync("api/v1/user/notifications/mark-all-read", null, token);
        return response.IsSuccess;
    }

    public async Task<bool> DeleteNotificationAsync(Guid notificationId)
    {
        var token = await _authService.GetTokenAsync();
        var response = await _apiClient.DeleteAsync($"api/v1/user/notifications/{notificationId}", token);
        return response.IsSuccess;
    }

    public async Task<bool> ClearAllNotificationsAsync()
    {
        var token = await _authService.GetTokenAsync();
        var response = await _apiClient.PostAsync("api/v1/user/notifications/clear-all", null, token);
        return response.IsSuccess;
    }
}

