using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Getir.Application.Services.Couriers;
using Getir.Application.Services.Orders;
using Getir.Application.Services.DeliveryOptimization;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.WebApi.Hubs;

/// <summary>
/// SignalR Hub for real-time courier location tracking and delivery management
/// Handles courier location updates, route optimization, and delivery coordination
/// </summary>
[Authorize] // Role-based authorization on method level
public class CourierHub : Hub
{
    private readonly ILogger<CourierHub> _logger;
    private readonly ICourierService _courierService;
    private readonly IOrderService _orderService;
    private readonly IRouteOptimizationService _routeService;

    public CourierHub(ILogger<CourierHub> logger, ICourierService courierService, IOrderService orderService, IRouteOptimizationService routeService)
    {
        _logger = logger;
        _courierService = courierService;
        _orderService = orderService;
        _routeService = routeService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"courier_{userId}");
            _logger.LogInformation("Courier {UserId} connected to CourierHub", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"courier_{userId}");
            _logger.LogInformation("Courier {UserId} disconnected from CourierHub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Courier sends location update (broadcasted to customers tracking their orders)
    /// </summary>
    public async Task UpdateLocation(double latitude, double longitude, Guid orderId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Validate coordinates
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            {
                await Clients.Caller.SendAsync("Error", "Invalid coordinates");
                return;
            }

            // Update location in database
            var updateRequest = new CourierLocationUpdateWithOrderRequest(
                userId.Value,
                orderId,
                (decimal)latitude,
                (decimal)longitude,
                DateTime.UtcNow);

            var result = await _courierService.UpdateLocationAsync(updateRequest, CancellationToken.None);

            if (result.Success)
            {
                _logger.LogInformation("Courier {UserId} updated location for order {OrderId}: {Lat}, {Lng}",
                    userId, orderId, latitude, longitude);

                // Calculate ETA
                var etaResult = await _routeService.CalculateETAAsync(orderId, latitude, longitude, CancellationToken.None);
                var estimatedMinutes = etaResult.Success ? etaResult.Value : 0;

                // Broadcast location to order subscribers (customers)
                await Clients.Group($"order_{orderId}")
                    .SendAsync("CourierLocationUpdated", new
                    {
                        orderId,
                        courierId = userId.Value,
                        latitude,
                        longitude,
                        estimatedMinutes,
                        timestamp = DateTime.UtcNow
                    });

                // Check if near destination
                if (estimatedMinutes <= 5)
                {
                    await Clients.Group($"order_{orderId}")
                        .SendAsync("CourierNearDestination", new
                        {
                            orderId,
                            estimatedMinutes,
                            timestamp = DateTime.UtcNow
                        });
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Error ?? "Failed to update location");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating courier location for order {OrderId}", orderId);
            await Clients.Caller.SendAsync("Error", "An error occurred while updating location");
        }
    }

    /// <summary>
    /// Track specific order's courier (Customer can track their order)
    /// </summary>
    [AllowAnonymous] // Allow customers to track
    public async Task TrackOrder(Guid orderId)
    {
        try
        {
            var userId = GetUserId();

            // Verify permission if authenticated
            if (userId != null)
            {
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
                        _logger.LogInformation("User {UserId} subscribed to courier tracking for order {OrderId}",
                            userId, orderId);

                        // Send current courier location if available
                        if (order.CourierId.HasValue)
                        {
                            var locationResult = await _courierService.GetCurrentLocationAsync(
                                order.CourierId.Value,
                                CancellationToken.None);

                            if (locationResult.Success && locationResult.Value != null)
                            {
                                await Clients.Caller.SendAsync("CurrentCourierLocation", locationResult.Value);
                            }
                        }
                    }
                    else
                    {
                        await Clients.Caller.SendAsync("Error", "You don't have permission to track this order");
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Order not found");
                }
            }
            else
            {
                // Guest tracking (limited functionality)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
                _logger.LogInformation("Guest subscribed to courier tracking for order {OrderId}", orderId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking order {OrderId}", orderId);
            await Clients.Caller.SendAsync("Error", "An error occurred while tracking order");
        }
    }

    /// <summary>
    /// Courier joins courier group for broadcasts
    /// </summary>
    public async Task JoinCourierGroup()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "all_couriers");
            _logger.LogInformation("Courier {UserId} joined courier group", userId);
        }
    }

    /// <summary>
    /// Subscribe to courier-specific notifications
    /// </summary>
    public async Task SubscribeToCourierNotifications()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"courier_notifications_{userId}");
            _logger.LogInformation("Courier {UserId} subscribed to notifications", userId);
        }
    }

    /// <summary>
    /// Get courier location history for an order
    /// </summary>
    public async Task GetLocationHistory(Guid orderId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var orderResult = await _orderService.GetOrderByIdAsync(orderId, CancellationToken.None);

            if (orderResult.Success && orderResult.Value != null && orderResult.Value.CourierId.HasValue)
            {
                // Verify permission
                if (orderResult.Value.UserId == userId.Value ||
                    orderResult.Value.CourierId == userId.Value ||
                    IsUserInRole("Admin"))
                {
                    var historyResult = await _courierService.GetLocationHistoryAsync(
                        orderResult.Value.CourierId.Value,
                        orderId,
                        CancellationToken.None);

                    if (historyResult.Success)
                    {
                        _logger.LogInformation("User {UserId} requested location history for order {OrderId}",
                            userId, orderId);

                        await Clients.Caller.SendAsync("LocationHistory", new
                        {
                            orderId,
                            locations = historyResult.Value,
                            timestamp = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        await Clients.Caller.SendAsync("Error", "No location history available");
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "You don't have permission to view location history");
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "Order not found or courier not assigned");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location history for order {OrderId}", orderId);
            await Clients.Caller.SendAsync("Error", "An error occurred while getting location history");
        }
    }

    /// <summary>
    /// Send estimated arrival time
    /// </summary>
    public async Task SendEstimatedArrival(Guid orderId, int estimatedMinutes)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Verify courier is assigned to this order
            var orderResult = await _orderService.GetOrderByIdAsync(orderId, CancellationToken.None);

            if (orderResult.Success && orderResult.Value != null)
            {
                if (orderResult.Value.CourierId == userId.Value || IsUserInRole("Admin"))
                {
                    _logger.LogInformation("Courier {UserId} sent estimated arrival for order {OrderId}: {Minutes} minutes",
                        userId, orderId, estimatedMinutes);

                    // Update ETA in database
                    var etaRequest = new UpdateETARequest(
                        orderId,
                        userId.Value,
                        estimatedMinutes,
                        DateTime.UtcNow);

                    var etaUpdateResult = await _routeService.UpdateETAAsync(etaRequest, CancellationToken.None);

                    if (!etaUpdateResult.Success)
                    {
                        _logger.LogWarning("Failed to update ETA for order {OrderId}: {Error}",
                            orderId, etaUpdateResult.Error);
                    }

                    // Broadcast to order subscribers
                    await Clients.Group($"order_{orderId}")
                        .SendAsync("EstimatedArrival", new
                        {
                            orderId,
                            estimatedMinutes,
                            timestamp = DateTime.UtcNow
                        });
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "You are not assigned to this order");
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "Order not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending estimated arrival for order {OrderId}", orderId);
            await Clients.Caller.SendAsync("Error", "An error occurred while sending estimated arrival");
        }
    }

    /// <summary>
    /// Courier marks order as picked up
    /// </summary>
    public async Task MarkAsPickedUp(Guid orderId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var request = new UpdateOrderStatusRequest(
                orderId,
                OrderStatus.PickedUp.ToString(),
                "Order picked up by courier");

            var result = await _orderService.UpdateOrderStatusAsync(request, CancellationToken.None);

            if (result.Success)
            {
                _logger.LogInformation("Courier {UserId} marked order {OrderId} as picked up", userId, orderId);

                await Clients.Group($"order_{orderId}")
                    .SendAsync("OrderPickedUp", new
                    {
                        orderId,
                        courierId = userId.Value,
                        timestamp = DateTime.UtcNow
                    });
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Error ?? "Failed to mark order as picked up");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking order {OrderId} as picked up", orderId);
            await Clients.Caller.SendAsync("Error", "An error occurred while marking order as picked up");
        }
    }

    /// <summary>
    /// Courier marks order as delivered
    /// </summary>
    public async Task MarkAsDelivered(Guid orderId, string? deliveryProof = null)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var request = new UpdateOrderStatusRequest(
                orderId,
                OrderStatus.Delivered.ToString(),
                deliveryProof != null ? $"Delivered. Proof: {deliveryProof}" : "Delivered");

            var result = await _orderService.UpdateOrderStatusAsync(request, CancellationToken.None);

            if (result.Success)
            {
                _logger.LogInformation("Courier {UserId} marked order {OrderId} as delivered", userId, orderId);

                await Clients.Group($"order_{orderId}")
                    .SendAsync("OrderDelivered", new
                    {
                        orderId,
                        courierId = userId.Value,
                        deliveryProof,
                        timestamp = DateTime.UtcNow
                    });

                // Update courier availability
                var availabilityResult = await _courierService.UpdateAvailabilityAsync(
                    userId.Value,
                    CourierAvailabilityStatus.Available,
                    CancellationToken.None);

                if (!availabilityResult.Success)
                {
                    _logger.LogWarning("Failed to update courier {UserId} availability after delivery: {Error}",
                        userId, availabilityResult.Error);
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Error ?? "Failed to mark order as delivered");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking order {OrderId} as delivered", orderId);
            await Clients.Caller.SendAsync("Error", "An error occurred while marking order as delivered");
        }
    }

    /// <summary>
    /// Courier gets assigned orders
    /// </summary>
    public async Task GetMyAssignedOrders()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var ordersResult = await _courierService.GetAssignedOrdersAsync(userId.Value, CancellationToken.None);

            if (ordersResult.Success)
            {
                _logger.LogInformation("Courier {UserId} requested assigned orders", userId);
                await Clients.Caller.SendAsync("AssignedOrders", ordersResult.Value);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", ordersResult.Error ?? "Failed to get assigned orders");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assigned orders for courier");
            await Clients.Caller.SendAsync("Error", "An error occurred while getting assigned orders");
        }
    }

    /// <summary>
    /// Courier updates availability status
    /// </summary>
    public async Task UpdateAvailability(CourierAvailabilityStatus status)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var result = await _courierService.UpdateAvailabilityAsync(userId.Value, status, CancellationToken.None);

            if (result.Success)
            {
                _logger.LogInformation("Courier {UserId} updated availability to {Status}", userId, status);

                await Clients.Caller.SendAsync("AvailabilityUpdated", new
                {
                    status = status.ToString(),
                    timestamp = DateTime.UtcNow
                });

                // Notify admin dashboard
                await Clients.Group("admin_courier_monitoring")
                    .SendAsync("CourierAvailabilityChanged", new
                    {
                        courierId = userId.Value,
                        status = status.ToString(),
                        timestamp = DateTime.UtcNow
                    });
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Error ?? "Failed to update availability");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating courier availability");
            await Clients.Caller.SendAsync("Error", "An error occurred while updating availability");
        }
    }

    /// <summary>
    /// Get optimized route for assigned orders
    /// </summary>
    public async Task GetOptimizedRoute()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var routeResult = await _routeService.GetOptimizedRouteForCourierAsync(userId.Value, CancellationToken.None);

            if (routeResult.Success)
            {
                _logger.LogInformation("Courier {UserId} requested optimized route", userId);
                await Clients.Caller.SendAsync("OptimizedRoute", routeResult.Value);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", routeResult.Error ?? "Failed to get optimized route");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting optimized route for courier");
            await Clients.Caller.SendAsync("Error", "An error occurred while getting optimized route");
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
