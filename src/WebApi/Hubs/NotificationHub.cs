using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Getir.WebApi.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            // Add user to their personal group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} connected to NotificationHub. ConnectionId: {ConnectionId}", 
                userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
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
    public async Task MarkAsRead(string notificationId)
    {
        var userId = GetUserId();
        if (userId != null)
        {
            _logger.LogInformation("User {UserId} marked notification {NotificationId} as read", 
                userId, notificationId);
            
            // Broadcast to user's other devices
            await Clients.Group($"user_{userId}")
                .SendAsync("NotificationRead", notificationId);
        }
    }

    /// <summary>
    /// Subscribe to specific notification types
    /// </summary>
    public async Task SubscribeToNotificationTypes(List<string> notificationTypes)
    {
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
        var userId = GetUserId();
        if (userId != null)
        {
            // This would typically fetch from a service
            _logger.LogInformation("User {UserId} requested unread notification count", userId);
            
            await Clients.Caller.SendAsync("UnreadCount", new
            {
                count = 0,
                timestamp = DateTime.UtcNow
            });
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
