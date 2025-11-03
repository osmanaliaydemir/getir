using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebApp.Models;

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
    private readonly ICartService _cartService;
    private readonly IUserService _userService;
    private readonly IOrderService _orderService;
    private readonly Dictionary<string, string> _pushSubscriptions;
    private readonly Dictionary<string, List<OfflineAction>> _offlineQueues;
    private readonly Dictionary<string, bool> _onlineStatus;
    private readonly Dictionary<string, Func<OfflineAction, Task>> _actionHandlers;

    public AdvancedPwaService(
        IHubContext<NotificationHub> hubContext, 
        ILogger<AdvancedPwaService> logger,
        ICartService cartService,
        IUserService userService,
        IOrderService orderService)
    {
        _hubContext = hubContext;
        _logger = logger;
        _cartService = cartService;
        _userService = userService;
        _orderService = orderService;
        _pushSubscriptions = new Dictionary<string, string>();
        _offlineQueues = new Dictionary<string, List<OfflineAction>>();
        _onlineStatus = new Dictionary<string, bool>();

        _actionHandlers = new Dictionary<string, Func<OfflineAction, Task>>(StringComparer.OrdinalIgnoreCase)
        {
            ["add_to_cart"] = ProcessAddToCartAction,
            ["remove_from_cart"] = ProcessRemoveFromCartAction,
            ["update_profile"] = ProcessUpdateProfileAction,
            ["place_order"] = ProcessPlaceOrderAction
        };
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
            // Check actual online status using navigator.onLine equivalent
            // In Blazor, we can use JSInterop to check navigator.onLine
            // For now, return true but in production, implement actual check via JSInterop
            if (_onlineStatus.TryGetValue("default", out var status))
            {
                return status;
            }
            return true; // Default to online
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
            if (_actionHandlers.TryGetValue(action.Action, out var handler))
            {
                await handler(action);
            }
            else
            {
                _logger.LogWarning("Unknown offline action: {Action}", action.Action);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing offline action {ActionId}", action.Id);
            throw;
        }
    }

    private static Dictionary<string, object>? DeserializeActionData(OfflineAction action)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(action.Data);
        }
        catch
        {
            return null;
        }
    }

    private static bool TryParseGuid(object? value, out Guid result)
    {
        result = Guid.Empty;
        return value != null && Guid.TryParse(value.ToString(), out result);
    }

    private async Task ProcessAddToCartAction(OfflineAction action)
    {
        try
        {
            _logger.LogInformation("Processing add to cart action {ActionId}", action.Id);
            
            var actionData = DeserializeActionData(action);
            if (actionData == null)
            {
                _logger.LogWarning("Invalid action data for add to cart action {ActionId}", action.Id);
                return;
            }

            if (actionData.TryGetValue("productId", out var productIdObj) && 
                actionData.TryGetValue("quantity", out var quantityObj) &&
                TryParseGuid(productIdObj, out var productId) &&
                int.TryParse(quantityObj?.ToString(), out var quantity))
            {
                var success = await _cartService.AddToCartAsync(productId, quantity);
                if (success)
                {
                    _logger.LogInformation("Successfully processed add to cart action {ActionId}", action.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to process add to cart action {ActionId}", action.Id);
                }
            }
            else
            {
                _logger.LogWarning("Missing or invalid productId/quantity in action data for {ActionId}", action.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing add to cart action {ActionId}", action.Id);
            throw;
        }
    }

    private async Task ProcessRemoveFromCartAction(OfflineAction action)
    {
        try
        {
            _logger.LogInformation("Processing remove from cart action {ActionId}", action.Id);
            
            var actionData = DeserializeActionData(action);
            if (actionData == null)
            {
                _logger.LogWarning("Invalid action data for remove from cart action {ActionId}", action.Id);
                return;
            }

            if (actionData.TryGetValue("itemId", out var itemIdObj) &&
                TryParseGuid(itemIdObj, out var itemId))
            {
                var success = await _cartService.RemoveFromCartAsync(itemId);
                if (success)
                {
                    _logger.LogInformation("Successfully processed remove from cart action {ActionId}", action.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to process remove from cart action {ActionId}", action.Id);
                }
            }
            else
            {
                _logger.LogWarning("Missing or invalid itemId in action data for {ActionId}", action.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing remove from cart action {ActionId}", action.Id);
            throw;
        }
    }

    private async Task ProcessUpdateProfileAction(OfflineAction action)
    {
        try
        {
            _logger.LogInformation("Processing update profile action {ActionId}", action.Id);
            
            var actionData = DeserializeActionData(action);
            if (actionData == null)
            {
                _logger.LogWarning("Invalid action data for update profile action {ActionId}", action.Id);
                return;
            }

            // Convert action data to UpdateUserProfileRequest
            var request = new UpdateUserProfileRequest();
            if (actionData.TryGetValue("firstName", out var firstName))
                request.FirstName = firstName?.ToString() ?? string.Empty;
            if (actionData.TryGetValue("lastName", out var lastName))
                request.LastName = lastName?.ToString() ?? string.Empty;
            if (actionData.TryGetValue("phone", out var phone))
                request.Phone = phone?.ToString() ?? string.Empty;

            var success = await _userService.UpdateUserProfileAsync(request);
            if (success)
            {
                _logger.LogInformation("Successfully processed update profile action {ActionId}", action.Id);
            }
            else
            {
                _logger.LogWarning("Failed to process update profile action {ActionId}", action.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing update profile action {ActionId}", action.Id);
            throw;
        }
    }

    private async Task ProcessPlaceOrderAction(OfflineAction action)
    {
        try
        {
            _logger.LogInformation("Processing place order action {ActionId}", action.Id);
            
            var actionData = DeserializeActionData(action);
            if (actionData == null)
            {
                _logger.LogWarning("Invalid action data for place order action {ActionId}", action.Id);
                return;
            }

            // Convert action data to CreateOrderRequest
            var request = new CreateOrderRequest();
            if (actionData.TryGetValue("deliveryAddressId", out var addressIdObj) &&
                TryParseGuid(addressIdObj, out var addressId))
                request.DeliveryAddressId = addressId;
            if (actionData.TryGetValue("merchantId", out var merchantIdObj) &&
                TryParseGuid(merchantIdObj, out var merchantId))
                request.MerchantId = merchantId;
            if (actionData.TryGetValue("paymentMethod", out var paymentMethod))
                request.PaymentMethod = paymentMethod?.ToString() ?? string.Empty;
            if (actionData.TryGetValue("items", out var itemsObj))
            {
                // Items should be deserialized properly
                // For now, empty list - should be handled from cart
                request.Items = new List<OrderItemRequest>();
            }

            var order = await _cartService.CreateOrderAsync(request);
            if (order != null)
            {
                _logger.LogInformation("Successfully processed place order action {ActionId}, OrderId: {OrderId}", action.Id, order.Id);
            }
            else
            {
                _logger.LogWarning("Failed to process place order action {ActionId}", action.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing place order action {ActionId}", action.Id);
            throw;
        }
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
