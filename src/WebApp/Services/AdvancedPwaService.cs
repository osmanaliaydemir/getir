using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace WebApp.Services;

public interface IAdvancedPwaService
{
    Task SendPushNotificationAsync(string userId, string title, string message, string? iconUrl = null, string? actionUrl = null);
    Task SendBulkPushNotificationAsync(List<string> userIds, string title, string message, string? iconUrl = null, string? actionUrl = null);
    Task RegisterPushSubscriptionAsync(string userId, string subscription);
    Task UnregisterPushSubscriptionAsync(string userId);
    Task SyncOfflineDataAsync(string userId);
    Task QueueOfflineActionAsync(string userId, string action, object data);
    Task ProcessOfflineQueueAsync(string userId);
    Task<bool> IsOnlineAsync();
    Task SetOnlineStatusAsync(bool isOnline);
    Task<Dictionary<string, object>> GetPwaStatusAsync();
}

public class AdvancedPwaService : IAdvancedPwaService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<AdvancedPwaService> _logger;
    private readonly Dictionary<string, string> _pushSubscriptions;
    private readonly Dictionary<string, List<OfflineAction>> _offlineQueues;
    private readonly Dictionary<string, bool> _onlineStatus;

    public AdvancedPwaService(IHubContext<NotificationHub> hubContext, ILogger<AdvancedPwaService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
        _pushSubscriptions = new Dictionary<string, string>();
        _offlineQueues = new Dictionary<string, List<OfflineAction>>();
        _onlineStatus = new Dictionary<string, bool>();
    }

    public async Task SendPushNotificationAsync(string userId, string title, string message, string? iconUrl = null, string? actionUrl = null)
    {
        try
        {
            if (_pushSubscriptions.TryGetValue(userId, out var subscription))
            {
                var notification = new PushNotification
                {
                    Title = title,
                    Message = message,
                    IconUrl = iconUrl ?? "/icons/icon-192x192.png",
                    ActionUrl = actionUrl,
                    Timestamp = DateTime.UtcNow,
                    UserId = userId
                };

                await _hubContext.Clients.User(userId).SendAsync("ReceivePushNotification", notification);
                _logger.LogInformation("Push notification sent to user {UserId}: {Title}", userId, title);
            }
            else
            {
                _logger.LogWarning("No push subscription found for user {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
        }
    }

    public async Task SendBulkPushNotificationAsync(List<string> userIds, string title, string message, string? iconUrl = null, string? actionUrl = null)
    {
        try
        {
            var tasks = userIds.Select(userId => SendPushNotificationAsync(userId, title, message, iconUrl, actionUrl));
            await Task.WhenAll(tasks);
            _logger.LogInformation("Bulk push notification sent to {Count} users: {Title}", userIds.Count, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk push notification");
        }
    }

    public async Task RegisterPushSubscriptionAsync(string userId, string subscription)
    {
        try
        {
            _pushSubscriptions[userId] = subscription;
            _logger.LogInformation("Push subscription registered for user {UserId}", userId);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering push subscription for user {UserId}", userId);
        }
    }

    public async Task UnregisterPushSubscriptionAsync(string userId)
    {
        try
        {
            _pushSubscriptions.Remove(userId);
            _logger.LogInformation("Push subscription unregistered for user {UserId}", userId);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering push subscription for user {UserId}", userId);
        }
    }

    public async Task SyncOfflineDataAsync(string userId)
    {
        try
        {
            if (_offlineQueues.TryGetValue(userId, out var queue) && queue.Any())
            {
                _logger.LogInformation("Syncing {Count} offline actions for user {UserId}", queue.Count, userId);
                
                foreach (var action in queue.ToList())
                {
                    try
                    {
                        await ProcessOfflineActionAsync(action);
                        queue.Remove(action);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing offline action {ActionId} for user {UserId}", action.Id, userId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing offline data for user {UserId}", userId);
        }
    }

    public async Task QueueOfflineActionAsync(string userId, string action, object data)
    {
        try
        {
            if (!_offlineQueues.ContainsKey(userId))
            {
                _offlineQueues[userId] = new List<OfflineAction>();
            }

            var offlineAction = new OfflineAction
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Action = action,
                Data = JsonSerializer.Serialize(data),
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0
            };

            _offlineQueues[userId].Add(offlineAction);
            _logger.LogDebug("Offline action queued for user {UserId}: {Action}", userId, action);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queuing offline action for user {UserId}", userId);
        }
    }

    public async Task ProcessOfflineQueueAsync(string userId)
    {
        try
        {
            if (_offlineQueues.TryGetValue(userId, out var queue))
            {
                var actionsToProcess = queue.Where(a => a.RetryCount < 3).ToList();
                
                foreach (var action in actionsToProcess)
                {
                    try
                    {
                        await ProcessOfflineActionAsync(action);
                        queue.Remove(action);
                    }
                    catch (Exception ex)
                    {
                        action.RetryCount++;
                        _logger.LogWarning(ex, "Failed to process offline action {ActionId}, retry {RetryCount}", 
                            action.Id, action.RetryCount);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing offline queue for user {UserId}", userId);
        }
    }

    public async Task<bool> IsOnlineAsync()
    {
        try
        {
            // Simple online check - in a real app, you might want to ping a server
            return true; // Assume online for now
        }
        catch
        {
            return false;
        }
    }

    public async Task SetOnlineStatusAsync(bool isOnline)
    {
        try
        {
            // This would typically be called when the connection status changes
            _logger.LogInformation("Online status changed to: {IsOnline}", isOnline);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting online status");
        }
    }

    public async Task<Dictionary<string, object>> GetPwaStatusAsync()
    {
        try
        {
            var status = new Dictionary<string, object>
            {
                ["isOnline"] = await IsOnlineAsync(),
                ["pushSubscriptions"] = _pushSubscriptions.Count,
                ["offlineQueues"] = _offlineQueues.Sum(kvp => kvp.Value.Count),
                ["timestamp"] = DateTime.UtcNow
            };

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PWA status");
            return new Dictionary<string, object>();
        }
    }

    private async Task ProcessOfflineActionAsync(OfflineAction action)
    {
        try
        {
            switch (action.Action.ToLower())
            {
                case "add_to_cart":
                    await ProcessAddToCartAction(action);
                    break;
                case "remove_from_cart":
                    await ProcessRemoveFromCartAction(action);
                    break;
                case "update_profile":
                    await ProcessUpdateProfileAction(action);
                    break;
                case "place_order":
                    await ProcessPlaceOrderAction(action);
                    break;
                default:
                    _logger.LogWarning("Unknown offline action: {Action}", action.Action);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing offline action {ActionId}", action.Id);
            throw;
        }
    }

    private async Task ProcessAddToCartAction(OfflineAction action)
    {
        // Implement add to cart logic
        _logger.LogDebug("Processing add to cart action {ActionId}", action.Id);
        await Task.CompletedTask;
    }

    private async Task ProcessRemoveFromCartAction(OfflineAction action)
    {
        // Implement remove from cart logic
        _logger.LogDebug("Processing remove from cart action {ActionId}", action.Id);
        await Task.CompletedTask;
    }

    private async Task ProcessUpdateProfileAction(OfflineAction action)
    {
        // Implement update profile logic
        _logger.LogDebug("Processing update profile action {ActionId}", action.Id);
        await Task.CompletedTask;
    }

    private async Task ProcessPlaceOrderAction(OfflineAction action)
    {
        // Implement place order logic
        _logger.LogDebug("Processing place order action {ActionId}", action.Id);
        await Task.CompletedTask;
    }
}

public class PushNotification
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string? ActionUrl { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public class OfflineAction
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int RetryCount { get; set; }
}

public class NotificationHub : Hub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
