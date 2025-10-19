using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Getir.Application.Services.Orders;
using Getir.Application.Services.Notifications;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.WebApi.Hubs;

/// <summary>
/// SignalR Hub for real-time order tracking and updates
/// Handles order status changes, merchant actions, and customer interactions
/// </summary>
[Authorize]
public class OrderHub : Hub
{
    private readonly ILogger<OrderHub> _logger;
    private readonly IOrderService _orderService;
    private readonly INotificationService _notificationService;

    public OrderHub(ILogger<OrderHub> logger, IOrderService orderService, INotificationService notificationService)
    {
        _logger = logger;
        _orderService = orderService;
        _notificationService = notificationService;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = GetUserId();
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation("User {UserId} connected to OrderHub. ConnectionId: {ConnectionId}",
                    userId, Context.ConnectionId);
            }
            else
            {
                _logger.LogWarning("User connected to OrderHub without valid userId. ConnectionId: {ConnectionId}",
                    Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OrderHub.OnConnectedAsync. ConnectionId: {ConnectionId}",
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
            _logger.LogInformation("User {UserId} disconnected from OrderHub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to specific order updates
    /// </summary>
    public async Task SubscribeToOrder(Guid orderId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Verify user has permission to subscribe to this order
            var orderResult = await _orderService.GetOrderByIdAsync(orderId, CancellationToken.None);

            if (orderResult.Success && orderResult.Value != null)
            {
                var order = orderResult.Value;

                // Allow customer, merchant, courier, or admin
                if (order.UserId == userId.Value ||
                    order.MerchantId == userId.Value ||
                    order.CourierId == userId.Value ||
                    IsUserInRole("Admin"))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
                    _logger.LogInformation("User {UserId} subscribed to order {OrderId}", userId, orderId);

                    // Send current order status
                    await Clients.Caller.SendAsync("OrderSubscribed", order);
                }
                else
                {
                    _logger.LogWarning("User {UserId} attempted unauthorized subscription to order {OrderId}",
                        userId, orderId);
                    await Clients.Caller.SendAsync("Error", "You don't have permission to subscribe to this order");
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "Order not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to order {OrderId}", orderId);
            await Clients.Caller.SendAsync("Error", "An error occurred while subscribing to order");
        }
    }

    /// <summary>
    /// Unsubscribe from order updates
    /// </summary>
    public async Task UnsubscribeFromOrder(Guid orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from order {OrderId}",
            Context.ConnectionId, orderId);
    }

    /// <summary>
    /// Subscribe to merchant order updates (Merchant/Admin only)
    /// </summary>
    public async Task SubscribeToMerchantOrders(Guid merchantId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Verify user is the merchant or admin
            if (userId.Value == merchantId || IsUserInRole("Admin"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"merchant_{merchantId}");
                _logger.LogInformation("User {UserId} subscribed to merchant {MerchantId} orders",
                    userId, merchantId);

                // Send current pending orders for merchant
                var pendingOrdersResult = await _orderService.GetMerchantPendingOrdersAsync(merchantId, CancellationToken.None);

                if (pendingOrdersResult.Success)
                {
                    await Clients.Caller.SendAsync("MerchantPendingOrders", new
                    {
                        orders = pendingOrdersResult.Data,
                        count = pendingOrdersResult.Data?.Count ?? 0,
                        merchantId,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to get pending orders for merchant {MerchantId}: {Error}",
                        merchantId, pendingOrdersResult.Error);
                    await Clients.Caller.SendAsync("Error", pendingOrdersResult.Error ?? "Failed to get pending orders");
                }
            }
            else
            {
                _logger.LogWarning("User {UserId} attempted unauthorized subscription to merchant {MerchantId}",
                    userId, merchantId);
                await Clients.Caller.SendAsync("Error", "You don't have permission to subscribe to this merchant");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to merchant {MerchantId} orders", merchantId);
            await Clients.Caller.SendAsync("Error", "An error occurred while subscribing to merchant orders");
        }
    }

    /// <summary>
    /// Subscribe to all order updates (admin only)
    /// </summary>
    public async Task SubscribeToAllOrders()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admin_orders");
        _logger.LogInformation("Connection {ConnectionId} subscribed to all orders", Context.ConnectionId);
    }

    /// <summary>
    /// Get order tracking information
    /// </summary>
    public async Task GetOrderTracking(Guid orderId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Fetch order details from service
            var orderResult = await _orderService.GetOrderByIdAsync(orderId, CancellationToken.None);

            if (orderResult.Success && orderResult.Value != null)
            {
                // Verify user has permission to view this order
                if (orderResult.Value.UserId == userId.Value || IsUserInRole("Admin") || IsUserInRole("Merchant"))
                {
                    _logger.LogInformation("User {UserId} requested tracking for order {OrderId}", userId, orderId);

                    await Clients.Caller.SendAsync("OrderTrackingInfo", orderResult.Value);
                }
                else
                {
                    _logger.LogWarning("User {UserId} attempted unauthorized access to order {OrderId}", userId, orderId);
                    await Clients.Caller.SendAsync("Error", "You don't have permission to view this order");
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Error", orderResult.Error ?? "Order not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order tracking for order {OrderId}", orderId);
            await Clients.Caller.SendAsync("Error", "An error occurred while getting order tracking");
        }
    }

    /// <summary>
    /// Update order status (Merchant/Courier only)
    /// </summary>
    public async Task UpdateOrderStatus(Guid orderId, OrderStatus newStatus, string? notes = null)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var updateRequest = new UpdateOrderStatusRequest(
                orderId,
                newStatus.ToString(),
                notes);

            var result = await _orderService.UpdateOrderStatusAsync(updateRequest, CancellationToken.None);

            if (result.Success)
            {
                _logger.LogInformation("User {UserId} updated order {OrderId} status to {Status}",
                    userId, orderId, newStatus);

                // Broadcast to all subscribers of this order
                await Clients.Group($"order_{orderId}")
                    .SendAsync("OrderStatusUpdated", new
                    {
                        orderId,
                        status = newStatus.ToString(),
                        notes,
                        timestamp = DateTime.UtcNow
                    });

                // Send notification to customer
                var order = result.Value;
                if (order != null)
                {
                    var notificationResult = await _notificationService.SendOrderStatusNotificationAsync(
                        order.UserId,
                        orderId,
                        newStatus,
                        CancellationToken.None);

                    if (!notificationResult.Success)
                    {
                        _logger.LogWarning("Failed to send order status notification for order {OrderId}: {Error}",
                            orderId, notificationResult.Error);
                    }

                    // Notify user group
                    await Clients.Group($"user_{order.UserId}")
                        .SendAsync("OrderStatusUpdated", new
                        {
                            orderId,
                            status = newStatus.ToString(),
                            notes,
                            timestamp = DateTime.UtcNow
                        });
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Error ?? "Failed to update order status");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId} status", orderId);
            await Clients.Caller.SendAsync("Error", "An error occurred while updating order status");
        }
    }

    /// <summary>
    /// Get user's active orders
    /// </summary>
    public async Task GetMyActiveOrders()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var activeOrdersResult = await _orderService.GetUserActiveOrdersAsync(userId.Value, CancellationToken.None);

            if (activeOrdersResult.Success)
            {
                _logger.LogInformation("User {UserId} requested active orders, found {Count}",
                    userId, activeOrdersResult.Data?.Count ?? 0);

                await Clients.Caller.SendAsync("ActiveOrders", new
                {
                    orders = activeOrdersResult.Data,
                    count = activeOrdersResult.Data?.Count ?? 0,
                    userId,
                    timestamp = DateTime.UtcNow
                });
            }
            else
            {
                _logger.LogWarning("Failed to get active orders for user {UserId}: {Error}",
                    userId, activeOrdersResult.Error);
                await Clients.Caller.SendAsync("Error", activeOrdersResult.Error ?? "Failed to get active orders");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active orders for user");
            await Clients.Caller.SendAsync("Error", "An error occurred while getting active orders");
        }
    }

    /// <summary>
    /// Cancel order (Customer only, within allowed time)
    /// </summary>
    public async Task CancelOrder(Guid orderId, string reason)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var cancelRequest = new CancelOrderRequest(
                orderId,
                reason);

            var result = await _orderService.CancelOrderAsync(cancelRequest, CancellationToken.None);

            if (result.Success)
            {
                _logger.LogInformation("User {UserId} cancelled order {OrderId}", userId, orderId);

                // Broadcast to order subscribers
                await Clients.Group($"order_{orderId}")
                    .SendAsync("OrderCancelled", new
                    {
                        orderId,
                        reason,
                        timestamp = DateTime.UtcNow
                    });

                // Notify merchant
                var order = result.Value;
                if (order != null)
                {
                    await Clients.Group($"merchant_{order.MerchantId}")
                        .SendAsync("OrderCancelled", new
                        {
                            orderId,
                            reason,
                            timestamp = DateTime.UtcNow
                        });
                }

                await Clients.Caller.SendAsync("OrderCancelledSuccess", orderId);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Error ?? "Failed to cancel order");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
            await Clients.Caller.SendAsync("Error", "An error occurred while cancelling order");
        }
    }

    /// <summary>
    /// Rate order (Customer only, after delivery)
    /// </summary>
    public async Task RateOrder(Guid orderId, int rating, string? comment = null)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            if (rating < 1 || rating > 5)
            {
                await Clients.Caller.SendAsync("Error", "Rating must be between 1 and 5");
                return;
            }

            var ratingRequest = new RateOrderRequest(
                orderId,
                userId.Value,
                rating,
                comment,
                DateTime.UtcNow);

            var result = await _orderService.RateOrderAsync(ratingRequest, CancellationToken.None);

            if (result.Success)
            {
                _logger.LogInformation("User {UserId} rated order {OrderId} with {Rating} stars",
                    userId, orderId, rating);

                await Clients.Caller.SendAsync("OrderRatedSuccess", new
                {
                    orderId,
                    rating,
                    comment,
                    timestamp = DateTime.UtcNow
                });

                // Notify merchant of new rating
                var order = result.Value;
                if (order != null)
                {
                    await Clients.Group($"merchant_{order.MerchantId}")
                        .SendAsync("NewOrderRating", new
                        {
                            orderId,
                            rating,
                            timestamp = DateTime.UtcNow
                        });
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Error ?? "Failed to rate order");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rating order {OrderId}", orderId);
            await Clients.Caller.SendAsync("Error", "An error occurred while rating order");
        }
    }

    private Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
            ? userId
            : null;
    }

    private bool IsUserInRole(string role)
    {
        return Context.User?.IsInRole(role) ?? false;
    }
}
