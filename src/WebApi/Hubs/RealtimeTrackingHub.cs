using Getir.Application.DTO;
using Getir.Application.Services.RealtimeTracking;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Getir.WebApi.Hubs;

public class RealtimeTrackingHub : Hub
{
    private readonly IRealtimeTrackingService _realtimeTrackingService;
    private readonly IOrderTrackingService _orderTrackingService;
    private readonly ITrackingNotificationService _notificationService;
    private readonly ILogger<RealtimeTrackingHub> _logger;

    public RealtimeTrackingHub(IRealtimeTrackingService realtimeTrackingService, IOrderTrackingService orderTrackingService, ITrackingNotificationService notificationService, ILogger<RealtimeTrackingHub> logger)
    {
        _realtimeTrackingService = realtimeTrackingService;
        _orderTrackingService = orderTrackingService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinOrderTrackingGroup(Guid orderId)
    {
        var groupName = $"OrderTracking_{orderId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);

        // Send current tracking data to the client
        var tracking = await _orderTrackingService.GetTrackingByOrderIdAsync(orderId);
        if (tracking != null)
        {
            await Clients.Caller.SendAsync("TrackingData", tracking);
        }
    }

    public async Task LeaveOrderTrackingGroup(Guid orderId)
    {
        var groupName = $"OrderTracking_{orderId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task JoinUserTrackingGroup(Guid userId)
    {
        var groupName = $"UserTracking_{userId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined user group {GroupName}", Context.ConnectionId, groupName);

        // Send user's active trackings
        var trackings = await _orderTrackingService.GetTrackingsByUserAsync(userId);
        await Clients.Caller.SendAsync("UserTrackings", trackings);
    }

    public async Task LeaveUserTrackingGroup(Guid userId)
    {
        var groupName = $"UserTracking_{userId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left user group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task JoinCourierTrackingGroup(Guid courierId)
    {
        var groupName = $"CourierTracking_{courierId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined courier group {GroupName}", Context.ConnectionId, groupName);

        // Send courier's active trackings
        var trackings = await _orderTrackingService.GetTrackingsByCourierAsync(courierId);
        await Clients.Caller.SendAsync("CourierTrackings", trackings);
    }

    public async Task LeaveCourierTrackingGroup(Guid courierId)
    {
        var groupName = $"CourierTracking_{courierId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left courier group {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task UpdateLocation(Guid orderTrackingId, double latitude, double longitude, string? address = null)
    {
        try
        {
            var request = new LocationUpdateRequest
            {
                OrderTrackingId = orderTrackingId,
                Latitude = latitude,
                Longitude = longitude,
                Address = address,
                UpdateType = Domain.Enums.LocationUpdateType.GPS,
                Accuracy = 10.0
            };

            var response = await _orderTrackingService.UpdateLocationAsync(request);

            if (response.Success)
            {
                // Get updated tracking data
                var tracking = await _orderTrackingService.GetTrackingByIdAsync(orderTrackingId);
                if (tracking != null)
                {
                    // Broadcast to order tracking group
                    var orderGroupName = $"OrderTracking_{tracking.OrderId}";
                    await Clients.Group(orderGroupName).SendAsync("LocationUpdated", tracking);

                    // Send notification if near destination
                    if (tracking.DistanceFromDestination <= 0.5) // 500 meters
                    {
                        await _notificationService.SendDeliveryAlertAsync(orderTrackingId);
                    }
                }
            }
            else
            {
                await Clients.Caller.SendAsync("LocationUpdateFailed", response.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating location for tracking {OrderTrackingId}", orderTrackingId);
            await Clients.Caller.SendAsync("LocationUpdateFailed", "An error occurred while updating location");
        }
    }

    public async Task UpdateStatus(Guid orderTrackingId, Domain.Enums.TrackingStatus status, string? message = null)
    {
        try
        {
            var request = new StatusUpdateRequest
            {
                OrderTrackingId = orderTrackingId,
                Status = status,
                StatusMessage = message
            };

            var response = await _orderTrackingService.UpdateStatusAsync(request);

            if (response.Success)
            {
                // Get updated tracking data
                var tracking = await _orderTrackingService.GetTrackingByIdAsync(orderTrackingId);
                if (tracking != null)
                {
                    // Broadcast to order tracking group
                    var orderGroupName = $"OrderTracking_{tracking.OrderId}";
                    await Clients.Group(orderGroupName).SendAsync("StatusUpdated", tracking);

                    // Send status update notification
                    await _notificationService.SendStatusUpdateNotificationAsync(orderTrackingId, status, message);

                    // Send completion alert if delivered
                    if (status == Domain.Enums.TrackingStatus.Delivered)
                    {
                        await _notificationService.SendCompletionAlertAsync(orderTrackingId);
                    }
                }
            }
            else
            {
                await Clients.Caller.SendAsync("StatusUpdateFailed", response.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for tracking {OrderTrackingId}", orderTrackingId);
            await Clients.Caller.SendAsync("StatusUpdateFailed", "An error occurred while updating status");
        }
    }

    public async Task RequestTrackingData(Guid orderId)
    {
        try
        {
            var tracking = await _orderTrackingService.GetTrackingByOrderIdAsync(orderId);
            if (tracking != null)
            {
                await Clients.Caller.SendAsync("TrackingData", tracking);
            }
            else
            {
                await Clients.Caller.SendAsync("TrackingNotFound", orderId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracking data for order {OrderId}", orderId);
            await Clients.Caller.SendAsync("TrackingDataError", "An error occurred while getting tracking data");
        }
    }

    public async Task RequestUserNotifications(Guid userId)
    {
        try
        {
            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
            await Clients.Caller.SendAsync("UserNotifications", notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
            await Clients.Caller.SendAsync("NotificationsError", "An error occurred while getting notifications");
        }
    }

    public async Task MarkNotificationAsRead(Guid notificationId)
    {
        try
        {
            var success = await _notificationService.MarkNotificationAsReadAsync(notificationId);
            if (success)
            {
                await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
            }
            else
            {
                await Clients.Caller.SendAsync("NotificationMarkFailed", "Failed to mark notification as read");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
            await Clients.Caller.SendAsync("NotificationMarkFailed", "An error occurred while marking notification as read");
        }
    }

    public async Task GetTrackingMetrics(Guid orderTrackingId)
    {
        try
        {
            var metrics = await _realtimeTrackingService.GetTrackingMetricsAsync(orderTrackingId);
            await Clients.Caller.SendAsync("TrackingMetrics", metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracking metrics for {OrderTrackingId}", orderTrackingId);
            await Clients.Caller.SendAsync("MetricsError", "An error occurred while getting tracking metrics");
        }
    }

    public async Task ValidateLocation(double latitude, double longitude)
    {
        try
        {
            var isValid = await _realtimeTrackingService.ValidateLocationAsync(latitude, longitude);
            await Clients.Caller.SendAsync("LocationValidation", new { IsValid = isValid, Latitude = latitude, Longitude = longitude });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating location {Latitude}, {Longitude}", latitude, longitude);
            await Clients.Caller.SendAsync("LocationValidationError", "An error occurred while validating location");
        }
    }

    // Admin methods
    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "AdminTracking");
        _logger.LogInformation("Admin client {ConnectionId} joined admin group", Context.ConnectionId);

        // Send all active trackings to admin
        var activeTrackings = await _orderTrackingService.GetActiveTrackingsAsync();
        await Clients.Caller.SendAsync("AllActiveTrackings", activeTrackings);
    }

    public async Task LeaveAdminGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AdminTracking");
        _logger.LogInformation("Admin client {ConnectionId} left admin group", Context.ConnectionId);
    }

    public async Task BroadcastToAllClients(string method, object data)
    {
        await Clients.All.SendAsync(method, data);
    }

    public async Task BroadcastToOrderGroup(Guid orderId, string method, object data)
    {
        var groupName = $"OrderTracking_{orderId}";
        await Clients.Group(groupName).SendAsync(method, data);
    }

    public async Task BroadcastToUserGroup(Guid userId, string method, object data)
    {
        var groupName = $"UserTracking_{userId}";
        await Clients.Group(groupName).SendAsync(method, data);
    }

    public async Task BroadcastToCourierGroup(Guid courierId, string method, object data)
    {
        var groupName = $"CourierTracking_{courierId}";
        await Clients.Group(groupName).SendAsync(method, data);
    }
}
