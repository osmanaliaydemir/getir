using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Getir.Application.Services.Notifications;
using Getir.Application.DTO;

namespace Getir.WebApi.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications
/// Handles push notifications, alerts, and real-time user communication
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    private readonly INotificationService _notificationService;
    private readonly INotificationPreferencesService _preferencesService;

    public NotificationHub(ILogger<NotificationHub> logger, INotificationService notificationService, INotificationPreferencesService preferencesService)
    {
        _logger = logger;
        _notificationService = notificationService;
        _preferencesService = preferencesService;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            _logger.LogInformation("NotificationHub connection attempt. ConnectionId: {ConnectionId}, User: {User}",
                Context.ConnectionId, Context.User?.Identity?.Name ?? "Anonymous");

            var userId = GetUserId();
            if (userId != null)
            {
                // Add user to their personal group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation("User {UserId} connected to NotificationHub. ConnectionId: {ConnectionId}",
                    userId, Context.ConnectionId);
            }
            else
            {
                _logger.LogWarning("User connected to NotificationHub without valid userId. ConnectionId: {ConnectionId}",
                    Context.ConnectionId);
            }

            await base.OnConnectedAsync();
            _logger.LogInformation("NotificationHub.OnConnectedAsync completed successfully. ConnectionId: {ConnectionId}",
                Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in NotificationHub.OnConnectedAsync. ConnectionId: {ConnectionId}",
                Context.ConnectionId);
            throw; // Rethrow to signal connection failure
        }
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} disconnected from NotificationHub. ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client marks notification as read
    /// </summary>
    public async Task MarkAsRead(Guid notificationId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Update notification status in database
            var result = await _notificationService.MarkAsReadAsync(notificationId, userId.Value);

            if (result.Success)
            {
                _logger.LogInformation("User {UserId} marked notification {NotificationId} as read",
                    userId, notificationId);

                // Broadcast to user's other devices
                await Clients.Group($"user_{userId}")
                    .SendAsync("NotificationRead", notificationId);

                // Send updated unread count
                var unreadCount = await _notificationService.GetUnreadCountAsync(userId.Value);
                await Clients.Group($"user_{userId}")
                    .SendAsync("UnreadCountUpdated", unreadCount);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Error ?? "Failed to mark notification as read");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
            await Clients.Caller.SendAsync("Error", "An error occurred while marking notification as read");
        }
    }

    /// <summary>
    /// Subscribe to specific notification types
    /// </summary>
    public async Task SubscribeToNotificationTypes(List<string> notificationTypes)
    {
        ArgumentNullException.ThrowIfNull(notificationTypes);

        var userId = GetUserId();
        if (userId != null)
        {
            foreach (var type in notificationTypes)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"notification_{type}_{userId}");
            }

            _logger.LogInformation("User {UserId} subscribed to notification types: {Types}",
                userId, string.Join(", ", notificationTypes));
        }
    }

    /// <summary>
    /// Subscribe to role-based notifications
    /// </summary>
    public async Task SubscribeToRoleNotifications()
    {
        var userId = GetUserId();
        var userRole = GetUserRole();

        if (userId != null && !string.IsNullOrEmpty(userRole))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{userRole}");
            _logger.LogInformation("User {UserId} with role {Role} subscribed to role notifications",
                userId, userRole);
        }
    }

    /// <summary>
    /// Get unread notification count
    /// </summary>
    public async Task GetUnreadCount()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var result = await _notificationService.GetUnreadCountAsync(userId.Value);

            if (!result.Success)
            {
                await Clients.Caller.SendAsync("Error", result.Error ?? "Failed to get unread count");
                return;
            }

            _logger.LogInformation("User {UserId} has {Count} unread notifications", userId, result.Data);

            await Clients.Caller.SendAsync("UnreadCount", new
            {
                count = result.Data,
                userId,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user");
            await Clients.Caller.SendAsync("Error", "An error occurred while getting unread count");
        }
    }

    /// <summary>
    /// Get user's recent notifications
    /// </summary>
    public async Task GetRecentNotifications(int count = 20)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var result = await _notificationService.GetUserNotificationsAsync(userId.Value, count);

            if (!result.Success)
            {
                await Clients.Caller.SendAsync("Error", result.Error ?? "Failed to get notifications");
                return;
            }

            _logger.LogInformation("User {UserId} requested {Count} recent notifications, found {Found}",
                userId, count, result.Data?.Count ?? 0);

            await Clients.Caller.SendAsync("RecentNotifications", new
            {
                notifications = result.Data,
                count = result.Data?.Count ?? 0,
                userId,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent notifications for user");
            await Clients.Caller.SendAsync("Error", "An error occurred while getting recent notifications");
        }
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    public async Task MarkAllAsRead()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var result = await _notificationService.MarkAllAsReadAsync(userId.Value);

            if (result.Success)
            {
                _logger.LogInformation("User {UserId} marked all notifications as read", userId);

                // Broadcast to user's other devices
                await Clients.Group($"user_{userId}")
                    .SendAsync("AllNotificationsRead");

                // Send updated count (should be 0)
                await Clients.Group($"user_{userId}")
                    .SendAsync("UnreadCountUpdated", 0);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Error ?? "Failed to mark all notifications as read");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user");
            await Clients.Caller.SendAsync("Error", "An error occurred while marking all notifications as read");
        }
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    public async Task DeleteNotification(Guid notificationId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var result = await _notificationService.DeleteNotificationAsync(notificationId, userId.Value);

            if (result.Success)
            {
                _logger.LogInformation("User {UserId} deleted notification {NotificationId}", userId, notificationId);

                // Broadcast to user's other devices
                await Clients.Group($"user_{userId}")
                    .SendAsync("NotificationDeleted", notificationId);

                // Send updated unread count
                var unreadCount = await _notificationService.GetUnreadCountAsync(userId.Value);
                await Clients.Group($"user_{userId}")
                    .SendAsync("UnreadCountUpdated", unreadCount);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Error ?? "Failed to delete notification");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
            await Clients.Caller.SendAsync("Error", "An error occurred while deleting notification");
        }
    }

    /// <summary>
    /// Update notification preferences
    /// </summary>
    public async Task UpdatePreferences(UpdateNotificationPreferencesRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var result = await _preferencesService.UpdateUserPreferencesAsync(userId.Value, request, CancellationToken.None);

            if (result.Success)
            {
                _logger.LogInformation("User {UserId} updated notification preferences", userId);
                await Clients.Caller.SendAsync("PreferencesUpdated", new { success = true, timestamp = DateTime.UtcNow });
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Error ?? "Failed to update preferences");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification preferences for user");
            await Clients.Caller.SendAsync("Error", "An error occurred while updating preferences");
        }
    }

    /// <summary>
    /// Get notification preferences
    /// </summary>
    public async Task GetPreferences()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var result = await _preferencesService.GetUserPreferencesAsync(userId.Value);

            if (!result.Success)
            {
                await Clients.Caller.SendAsync("Error", result.Error ?? "Failed to get preferences");
                return;
            }

            _logger.LogInformation("User {UserId} requested notification preferences", userId);
            await Clients.Caller.SendAsync("NotificationPreferences", result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification preferences for user");
            await Clients.Caller.SendAsync("Error", "An error occurred while getting preferences");
        }
    }

    /// <summary>
    /// Subscribe to dashboard updates
    /// </summary>
    public async Task SubscribeToDashboard()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard_updates");
            _logger.LogInformation("User {UserId} subscribed to dashboard updates", userId);
        }
    }

    private string? GetUserRole()
    {
        return Context.User?.FindFirst("role")?.Value;
    }

    private Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
            ? userId
            : null;
    }
}
