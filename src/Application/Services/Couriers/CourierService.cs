// System namespaces
using Microsoft.Extensions.Logging;

// Application namespaces
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;

// Domain namespaces
using Getir.Domain.Entities;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Couriers;

public class CourierService : BaseService, ICourierService
{
    private readonly ISignalRService? _signalRService;
    private readonly IBackgroundTaskService _backgroundTaskService;

    public CourierService(IUnitOfWork unitOfWork, ILogger<CourierService> logger, ILoggingService loggingService, ICacheService cacheService, IBackgroundTaskService backgroundTaskService, ISignalRService? signalRService = null)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _signalRService = signalRService;
        _backgroundTaskService = backgroundTaskService;
    }
    public async Task<Result<PagedResult<CourierOrderResponse>>> GetAssignedOrdersAsync(Guid courierId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Repository<Order>().GetPagedAsync(
            filter: o => o.CourierId == courierId && (o.Status == OrderStatus.Ready || o.Status == OrderStatus.OnTheWay),
            orderBy: o => o.CreatedAt,
            ascending: false,
            page: query.Page,
            pageSize: query.PageSize,
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(o => o.CourierId == courierId, cancellationToken);

        var response = orders.Select(o => new CourierOrderResponse(
            o.Id,
            o.OrderNumber,
            o.MerchantId,
            o.Merchant.Name,
            o.Status.ToStringValue(),
            o.Total,
            o.DeliveryAddress,
            o.EstimatedDeliveryTime,
            o.CreatedAt
        )).ToList();

        var pagedResult = PagedResult<CourierOrderResponse>.Create(response, total, query.Page, query.PageSize);

        return Result.Ok(pagedResult);
    }
    public async Task<Result> UpdateLocationAsync(Guid courierId, CourierLocationUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var courier = await _unitOfWork.Repository<Courier>()
            .GetByIdAsync(courierId, cancellationToken);

        if (courier == null)
        {
            return Result.Fail("Courier not found", "NOT_FOUND_COURIER");
        }

        courier.CurrentLatitude = request.Latitude;
        courier.CurrentLongitude = request.Longitude;
        courier.LastLocationUpdate = DateTime.UtcNow;

        _unitOfWork.Repository<Courier>().Update(courier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send real-time location update via SignalR to all active orders
        if (_signalRService != null)
        {
            var activeOrders = await _unitOfWork.ReadRepository<Order>()
                .ListAsync(
                    filter: o => o.CourierId == courierId &&
                                (o.Status == OrderStatus.Ready || o.Status == OrderStatus.OnTheWay),
                    cancellationToken: cancellationToken);

            foreach (var order in activeOrders)
            {
                await _signalRService.SendCourierLocationUpdateAsync(
                    order.Id,
                    request.Latitude,
                    request.Longitude);
            }
        }

        return Result.Ok();
    }
    public async Task<Result> SetAvailabilityAsync(Guid courierId, SetAvailabilityRequest request, CancellationToken cancellationToken = default)
    {
        var courier = await _unitOfWork.Repository<Courier>()
            .GetByIdAsync(courierId, cancellationToken);

        if (courier == null)
        {
            return Result.Fail("Courier not found", "NOT_FOUND_COURIER");
        }

        courier.IsAvailable = request.IsAvailable;

        _unitOfWork.Repository<Courier>().Update(courier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
    // Courier Panel methods
    public async Task<Result<CourierDashboardResponse>> GetCourierDashboardAsync(Guid courierId, CancellationToken cancellationToken = default)
    {
        var courier = await _unitOfWork.ReadRepository<Courier>()
            .FirstOrDefaultAsync(c => c.Id == courierId, cancellationToken: cancellationToken);

        if (courier == null)
        {
            return Result.Fail<CourierDashboardResponse>("Courier not found", "NOT_FOUND");
        }

        // Get stats
        var stats = await GetCourierStatsAsync(courierId, cancellationToken);
        if (!stats.Success)
        {
            return Result.Fail<CourierDashboardResponse>(stats.Error ?? "Failed to get courier stats", stats.ErrorCode);
        }

        // Get active orders
        var activeOrdersQuery = new PaginationQuery { Page = 1, PageSize = 10 };
        var activeOrders = await GetAssignedOrdersAsync(courierId, activeOrdersQuery, cancellationToken);
        if (!activeOrders.Success)
        {
            return Result.Fail<CourierDashboardResponse>(activeOrders.Error ?? "Failed to get active orders", activeOrders.ErrorCode);
        }

        // Get recent deliveries (completed orders)
        var recentDeliveries = await _unitOfWork.Repository<Order>()
            .ListAsync(
                filter: o => o.CourierId == courierId && o.Status == OrderStatus.Delivered,
                orderBy: o => o.UpdatedAt,
                ascending: false,
                take: 5,
                include: "Merchant,User",
                cancellationToken: cancellationToken);

        var recentDeliveriesResponse = recentDeliveries.Select(o => new CourierOrderResponse(
            o.Id,
            o.OrderNumber,
            o.MerchantId,
            o.Merchant.Name,
            o.Status.ToStringValue(),
            o.Total,
            o.DeliveryAddress,
            o.EstimatedDeliveryTime,
            o.CreatedAt)).ToList();

        // Get earnings
        var earnings = await GetCourierEarningsAsync(courierId, cancellationToken: cancellationToken);
        if (!earnings.Success)
        {
            return Result.Fail<CourierDashboardResponse>(earnings.Error ?? "Failed to get earnings", earnings.ErrorCode);
        }

        var dashboard = new CourierDashboardResponse(
            stats.Value!,
            activeOrders.Value!.Items.ToList(),
            recentDeliveriesResponse,
            earnings.Value!);

        return Result.Ok(dashboard);
    }
    public async Task<Result<CourierStatsResponse>> GetCourierStatsAsync(Guid courierId, CancellationToken cancellationToken = default)
    {
        var courier = await _unitOfWork.ReadRepository<Courier>()
            .FirstOrDefaultAsync(c => c.Id == courierId, cancellationToken: cancellationToken);

        if (courier == null)
        {
            return Result.Fail<CourierStatsResponse>("Courier not found", "NOT_FOUND");
        }

        // Count active orders
        var activeOrdersCount = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(o => o.CourierId == courierId && (o.Status == OrderStatus.Ready || o.Status == OrderStatus.OnTheWay), cancellationToken);

        // Count completed today
        var today = DateTime.UtcNow.Date;
        var completedToday = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(o => o.CourierId == courierId && o.Status == OrderStatus.Delivered && o.UpdatedAt.HasValue && o.UpdatedAt.Value.Date == today, cancellationToken);

        // Get last delivery time
        var lastDelivery = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(
                o => o.CourierId == courierId && o.Status == OrderStatus.Delivered,
                cancellationToken: cancellationToken);

        var stats = new CourierStatsResponse(
            courier.TotalDeliveries,
            activeOrdersCount,
            completedToday,
            courier.Rating,
            courier.IsAvailable,
            lastDelivery?.UpdatedAt);

        return Result.Ok(stats);
    }
    public async Task<Result<CourierEarningsResponse>> GetCourierEarningsAsync(Guid courierId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var courier = await _unitOfWork.ReadRepository<Courier>()
            .FirstOrDefaultAsync(c => c.Id == courierId, cancellationToken: cancellationToken);

        if (courier == null)
        {
            return Result.Fail<CourierEarningsResponse>("Courier not found", "NOT_FOUND");
        }

        var today = DateTime.UtcNow.Date;
        var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
        var thisMonthStart = new DateTime(today.Year, today.Month, 1);

        // Today's earnings
        var todayOrders = await _unitOfWork.ReadRepository<Order>()
            .ListAsync(o => o.CourierId == courierId && o.Status == OrderStatus.Delivered && o.UpdatedAt.HasValue && o.UpdatedAt.Value.Date == today, cancellationToken: cancellationToken);
        var todayEarnings = todayOrders.Sum(o => o.DeliveryFee);

        // This week's earnings
        var thisWeekOrders = await _unitOfWork.ReadRepository<Order>()
            .ListAsync(o => o.CourierId == courierId && o.Status == OrderStatus.Delivered && o.UpdatedAt.HasValue && o.UpdatedAt.Value.Date >= thisWeekStart, cancellationToken: cancellationToken);
        var thisWeekEarnings = thisWeekOrders.Sum(o => o.DeliveryFee);

        // This month's earnings
        var thisMonthOrders = await _unitOfWork.ReadRepository<Order>()
            .ListAsync(o => o.CourierId == courierId && o.Status == OrderStatus.Delivered && o.UpdatedAt.HasValue && o.UpdatedAt.Value.Date >= thisMonthStart, cancellationToken: cancellationToken);
        var thisMonthEarnings = thisMonthOrders.Sum(o => o.DeliveryFee);

        // Total earnings
        var allOrders = await _unitOfWork.ReadRepository<Order>()
            .ListAsync(o => o.CourierId == courierId && o.Status == OrderStatus.Delivered, cancellationToken: cancellationToken);
        var totalEarnings = allOrders.Sum(o => o.DeliveryFee);

        var earnings = new CourierEarningsResponse(
            todayEarnings,
            thisWeekEarnings,
            thisMonthEarnings,
            totalEarnings,
            todayOrders.Count,
            thisWeekOrders.Count,
            thisMonthOrders.Count);

        return Result.Ok(earnings);
    }
    public async Task<Result> AcceptOrderAsync(Guid courierId, AcceptOrderRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken: cancellationToken);

        if (order == null)
        {
            return Result.Fail("Order not found", "NOT_FOUND");
        }

        if (order.Status != OrderStatus.Ready)
        {
            return Result.Fail("Order is not ready for pickup", "INVALID_STATUS");
        }

        var courier = await _unitOfWork.ReadRepository<Courier>()
            .FirstOrDefaultAsync(c => c.Id == courierId, cancellationToken: cancellationToken);

        if (courier == null || !courier.IsAvailable)
        {
            return Result.Fail("Courier not available", "COURIER_UNAVAILABLE");
        }

        order.CourierId = courierId;
        order.Status = OrderStatus.OnTheWay;
        order.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send SignalR notification
        if (_signalRService != null)
        {
            await _signalRService.SendOrderStatusUpdateAsync(order.UserId, order.Id, OrderStatus.OnTheWay.ToStringValue(), "Order accepted by courier");
        }

        return Result.Ok();
    }
    public async Task<Result> StartDeliveryAsync(Guid courierId, StartDeliveryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.CourierId == courierId, cancellationToken: cancellationToken);

        if (order == null)
        {
            return Result.Fail("Order not found", "NOT_FOUND");
        }

        if (order.Status != OrderStatus.OnTheWay)
        {
            return Result.Fail("Order is not on the way", "INVALID_STATUS");
        }

        order.Status = OrderStatus.OnTheWay; // Delivering is same as OnTheWay
        order.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send SignalR notification
        if (_signalRService != null)
        {
            await _signalRService.SendOrderStatusUpdateAsync(order.UserId, order.Id, OrderStatus.OnTheWay.ToStringValue(), "Order delivery started");
        }

        return Result.Ok();
    }
    public async Task<Result> CompleteDeliveryAsync(Guid courierId, CompleteDeliveryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.CourierId == courierId, cancellationToken: cancellationToken);

        if (order == null)
        {
            return Result.Fail("Order not found", "NOT_FOUND");
        }

        if (order.Status != OrderStatus.OnTheWay) // Delivering is same as OnTheWay
        {
            return Result.Fail("Order is not being delivered", "INVALID_STATUS");
        }

        order.Status = OrderStatus.Delivered;
        order.UpdatedAt = DateTime.UtcNow;

        // Update courier stats
        var courier = await _unitOfWork.ReadRepository<Courier>()
            .FirstOrDefaultAsync(c => c.Id == courierId, cancellationToken: cancellationToken);

        if (courier != null)
        {
            courier.TotalDeliveries++;
            _unitOfWork.Repository<Courier>().Update(courier);
        }

        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send SignalR notification
        if (_signalRService != null)
        {
            await _signalRService.SendOrderStatusUpdateAsync(order.UserId, order.Id, "Delivered", "Order delivered successfully");
        }

        return Result.Ok();
    }
    // Order Assignment methods
    public async Task<Result<CourierAssignmentResponse>> AssignOrderAsync(AssignOrderRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken: cancellationToken);

        if (order == null)
        {
            return Result.Fail<CourierAssignmentResponse>("Order not found", "NOT_FOUND");
        }

        if (order.Status != OrderStatus.Ready)
        {
            return Result.Fail<CourierAssignmentResponse>("Order is not ready for assignment", "INVALID_STATUS");
        }

        Courier? assignedCourier = null;

        if (request.PreferredCourierId.HasValue)
        {
            assignedCourier = await _unitOfWork.ReadRepository<Courier>()
                .FirstOrDefaultAsync(c => c.Id == request.PreferredCourierId.Value && c.IsAvailable, cancellationToken: cancellationToken);
        }

        if (assignedCourier == null)
        {
            // Find nearest available courier
            var nearestCouriers = await FindNearestCouriersAsync(new FindNearestCouriersRequest(
                order.DeliveryLatitude,
                order.DeliveryLongitude), cancellationToken);

            if (!nearestCouriers.Success || !nearestCouriers.Value.HasAvailableCouriers)
            {
                return Result.Fail<CourierAssignmentResponse>("No available couriers found", "NO_COURIERS_AVAILABLE");
            }

            var nearestCourier = nearestCouriers.Value.AvailableCouriers.First();
            assignedCourier = await _unitOfWork.ReadRepository<Courier>()
                .FirstOrDefaultAsync(c => c.Id == nearestCourier.CourierId, cancellationToken: cancellationToken);
        }

        if (assignedCourier == null)
        {
            return Result.Fail<CourierAssignmentResponse>("Courier not found", "COURIER_NOT_FOUND");
        }

        // Assign order to courier
        order.CourierId = assignedCourier.Id;
        order.Status = OrderStatus.OnTheWay;
        order.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CourierAssignmentResponse(
            assignedCourier.Id,
            assignedCourier.User.FirstName + " " + assignedCourier.User.LastName,
            0, // Distance calculation would be implemented here
            0, // Estimated time calculation would be implemented here
            true);

        // Send SignalR notification to courier
        if (_signalRService != null)
        {
            await _signalRService.SendOrderStatusUpdateAsync(assignedCourier.UserId, order.Id, "Assigned", "New order assigned to you");
        }

        return Result.Ok(response);
    }
    public async Task<Result<FindNearestCouriersResponse>> FindNearestCouriersAsync(FindNearestCouriersRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var availableCouriers = await _unitOfWork.ReadRepository<Courier>()
            .ListAsync(c => c.IsAvailable && c.IsActive,
                include: "User",
                cancellationToken: cancellationToken);

        var courierResponses = availableCouriers
            .Where(c => c.CurrentLatitude.HasValue && c.CurrentLongitude.HasValue)
            .Select(c => new CourierAssignmentResponse(
                c.Id,
                c.User.FirstName + " " + c.User.LastName,
                CalculateDistance(request.Latitude, request.Longitude, c.CurrentLatitude.Value, c.CurrentLongitude.Value),
                CalculateEstimatedTime(request.Latitude, request.Longitude, c.CurrentLatitude.Value, c.CurrentLongitude.Value),
                true))
            .OrderBy(c => c.Distance)
            .Take(request.MaxCouriers)
            .ToList();

        var response = new FindNearestCouriersResponse(
            courierResponses,
            courierResponses.Any());

        return Result.Ok(response);
    }
    public async Task<Result<List<CourierPerformanceResponse>>> GetTopPerformersAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var couriers = await _unitOfWork.ReadRepository<Courier>()
            .ListAsync(c => c.IsActive,
                include: "User",
                cancellationToken: cancellationToken);

        var performers = new List<CourierPerformanceResponse>();

        foreach (var courier in couriers)
        {
            var orders = await _unitOfWork.ReadRepository<Order>()
                .ListAsync(o => o.CourierId == courier.Id && o.Status == OrderStatus.Delivered, cancellationToken: cancellationToken);

            var totalEarnings = orders.Sum(o => o.DeliveryFee);
            var thisWeek = DateTime.UtcNow.AddDays(-7);
            var thisMonth = DateTime.UtcNow.AddDays(-30);

            var completedThisWeek = orders.Count(o => o.UpdatedAt >= thisWeek);
            var completedThisMonth = orders.Count(o => o.UpdatedAt >= thisMonth);

            performers.Add(new CourierPerformanceResponse(
                courier.Id,
                courier.User.FirstName + " " + courier.User.LastName,
                courier.TotalDeliveries,
                courier.Rating,
                totalEarnings,
                completedThisWeek,
                completedThisMonth,
                orders.OrderByDescending(o => o.UpdatedAt).FirstOrDefault()?.UpdatedAt));
        }

        var topPerformers = performers
            .OrderByDescending(p => p.TotalDeliveries)
            .ThenByDescending(p => p.Rating ?? 0)
            .Take(count)
            .ToList();

        return Result.Ok(topPerformers);
    }
    public async Task<Result<CourierEarningsDetailResponse>> GetEarningsDetailAsync(CourierEarningsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var startDate = query.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = query.EndDate ?? DateTime.UtcNow;

        var orders = await _unitOfWork.ReadRepository<Order>()
            .ListAsync(o => o.CourierId == query.CourierId &&
                           o.Status == OrderStatus.Delivered &&
                           o.UpdatedAt >= startDate &&
                           o.UpdatedAt <= endDate,
                orderBy: o => o.UpdatedAt,
                ascending: true,
                cancellationToken: cancellationToken);

        var baseEarnings = orders.Sum(o => o.DeliveryFee);
        var bonusEarnings = 0m; // Bonus logic would be implemented here
        var totalEarnings = baseEarnings + bonusEarnings;

        var earningsBreakdown = orders
            .GroupBy(o => o.UpdatedAt.HasValue ? o.UpdatedAt.Value.Date : DateTime.MinValue.Date)
            .Select(g => new CourierEarningsItemResponse(
                g.Key,
                g.Count(),
                g.Sum(o => o.DeliveryFee),
                0m, // Bonus earnings per day
                g.Sum(o => o.DeliveryFee)))
            .ToList();

        var response = new CourierEarningsDetailResponse(
            query.CourierId,
            baseEarnings,
            bonusEarnings,
            totalEarnings,
            orders.Count,
            earningsBreakdown);

        return Result.Ok(response);
    }
    // SignalR Hub-specific methods
    public async Task<Result> UpdateLocationAsync(CourierLocationUpdateWithOrderRequest request, CancellationToken cancellationToken = default)
    {
        // Store location in database (assuming CourierLocation entity exists)
        var location = new CourierLocation
        {
            Id = Guid.NewGuid(),
            CourierId = request.CourierId,
            OrderId = request.OrderId,
            Latitude = (double)request.Latitude,
            Longitude = (double)request.Longitude,
            Timestamp = request.Timestamp,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<CourierLocation>().AddAsync(location, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _loggingService.LogBusinessEvent("CourierLocationUpdated", new
        {
            courierId = request.CourierId,
            orderId = request.OrderId,
            latitude = request.Latitude,
            longitude = request.Longitude
        });

        return Result.Ok();
    }
    public async Task<Result<CourierLocationResponse>> GetCurrentLocationAsync(Guid courierId, CancellationToken cancellationToken = default)
    {
        // Get most recent location
        var locations = await _unitOfWork.Repository<CourierLocation>()
            .GetPagedAsync(
                filter: l => l.CourierId == courierId,
                orderBy: l => l.Timestamp,
                ascending: false,
                page: 1,
                pageSize: 1,
                cancellationToken: cancellationToken);

        var latestLocation = locations.FirstOrDefault();

        if (latestLocation == null)
        {
            return Result.Fail<CourierLocationResponse>("No location found for courier", "LOCATION_NOT_FOUND");
        }

        // Check courier availability
        var courier = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Id == courierId && u.Role == UserRole.Courier, cancellationToken: cancellationToken);

        var response = new CourierLocationResponse(
            courierId,
            (decimal)latestLocation.Latitude,
            (decimal)latestLocation.Longitude,
            latestLocation.Timestamp,
            courier?.IsActive ?? false);

        return Result.Ok(response);
    }
    public async Task<Result<List<CourierLocationHistoryItem>>> GetLocationHistoryAsync(Guid courierId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var locations = await _unitOfWork.Repository<CourierLocation>()
            .GetPagedAsync(
                filter: l => l.CourierId == courierId && l.OrderId == orderId,
                orderBy: l => l.Timestamp,
                ascending: true,
                page: 1,
                pageSize: 100,
                cancellationToken: cancellationToken);

        var response = locations.Select(l => new CourierLocationHistoryItem(
            l.Id,
            l.CourierId,
            l.OrderId,
            (decimal)l.Latitude,
            (decimal)l.Longitude,
            l.Timestamp,
            null, // Speed
            null  // Accuracy
        )).ToList();

        return Result.Ok(response);
    }
    public async Task<Result<List<CourierOrderResponse>>> GetAssignedOrdersAsync(Guid courierId, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Repository<Order>()
            .GetPagedAsync(
                filter: o => o.CourierId == courierId &&
                            (o.Status == OrderStatus.Ready ||
                             o.Status == OrderStatus.PickedUp ||
                             o.Status == OrderStatus.OnTheWay),
                orderBy: o => o.CreatedAt,
                ascending: true,
                page: 1,
                pageSize: 20,
                cancellationToken: cancellationToken);

        var response = orders.Select(o => new CourierOrderResponse(
            o.Id,
            o.OrderNumber,
            o.MerchantId,
            o.Merchant?.Name ?? "Unknown",
            o.Status.ToString(),
            o.Total,
            o.DeliveryAddress,
            o.EstimatedDeliveryTime,
            o.CreatedAt
        )).ToList();

        return Result.Ok(response);
    }
    public async Task<Result> UpdateAvailabilityAsync(Guid courierId, CourierAvailabilityStatus status, CancellationToken cancellationToken = default)
    {
        var courier = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(
                u => u.Id == courierId && u.Role == UserRole.Courier,
                null,
                cancellationToken);

        if (courier == null)
        {
            return Result.Fail("Courier not found", "COURIER_NOT_FOUND");
        }

        // Update availability (assuming User has availability-related fields)
        courier.IsActive = (status == CourierAvailabilityStatus.Available);
        courier.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<User>().Update(courier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _loggingService.LogBusinessEvent("CourierAvailabilityUpdated", new
        {
            courierId,
            status = status.ToString()
        });

        return Result.Ok();
    }
    // Helper methods
    private static decimal CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        // Haversine formula implementation would go here
        // For now, return a simple calculation
        return Math.Abs((decimal)Math.Sqrt(Math.Pow((double)(lat2 - lat1), 2) + Math.Pow((double)(lon2 - lon1), 2))) * 111; // Rough km conversion
    }
    private static int CalculateEstimatedTime(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        // Simple calculation based on distance
        var distance = CalculateDistance(lat1, lon1, lat2, lon2);
        return (int)(distance * 2); // Assume 30 km/h average speed
    }
    // Additional methods implementation
    public async Task<Result<CourierResponse>> GetCourierByIdAsync(Guid courierId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetCourierByIdInternalAsync(courierId, cancellationToken),
            "GetCourierById",
            new { courierId },
            cancellationToken);
    }
    public async Task<Result<PagedResult<CourierResponse>>> GetCouriersByAvailabilityAsync(bool isAvailable, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetCouriersByAvailabilityInternalAsync(isAvailable, query, cancellationToken),
            "GetCouriersByAvailability",
            new { isAvailable, query.Page, query.PageSize },
            cancellationToken);
    }
    public async Task<Result> AssignCourierToOrderAsync(Guid orderId, Guid courierId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await AssignCourierToOrderInternalAsync(orderId, courierId, cancellationToken),
            "AssignCourierToOrder",
            new { orderId, courierId },
            cancellationToken);
    }
    private async Task<Result<CourierResponse>> GetCourierByIdInternalAsync(Guid courierId, CancellationToken cancellationToken)
    {
        var courier = await _unitOfWork.ReadRepository<Courier>()
            .FirstOrDefaultAsync(c => c.Id == courierId, cancellationToken: cancellationToken);

        if (courier == null)
            return ServiceResult.Failure<CourierResponse>("Courier not found", "NOT_FOUND_COURIER");

        return Result.Ok(MapToCourierResponse(courier));
    }
    private async Task<Result<PagedResult<CourierResponse>>> GetCouriersByAvailabilityInternalAsync(bool isAvailable, PaginationQuery query, CancellationToken cancellationToken)
    {
        var couriers = await _unitOfWork.ReadRepository<Courier>()
            .GetPagedAsync(
                filter: c => c.IsAvailable == isAvailable && c.IsActive,
                page: query.Page,
                pageSize: query.PageSize,
                orderBy: c => c.CreatedAt,
                ascending: false,
                cancellationToken: cancellationToken);

        var totalCount = await _unitOfWork.ReadRepository<Courier>()
            .CountAsync(c => c.IsAvailable == isAvailable && c.IsActive, cancellationToken);

        var responses = (IReadOnlyList<CourierResponse>)couriers.Select(MapToCourierResponse).ToList();
        return Result.Ok(PagedResult<CourierResponse>.Create(responses, totalCount, query.Page, query.PageSize));
    }
    private async Task<Result> AssignCourierToOrderInternalAsync(Guid orderId, Guid courierId, CancellationToken cancellationToken)
    {
        var courier = await _unitOfWork.ReadRepository<Courier>()
            .FirstOrDefaultAsync(c => c.Id == courierId, cancellationToken: cancellationToken);

        if (courier == null)
            return ServiceResult.Failure("Courier not found", "NOT_FOUND_COURIER");

        if (!courier.IsAvailable)
            return ServiceResult.Failure("Courier is not available", "COURIER_NOT_AVAILABLE");

        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken: cancellationToken);

        if (order == null)
            return ServiceResult.Failure("Order not found", ErrorCodes.ORDER_NOT_FOUND);

        order.CourierId = courierId;
        order.Status = OrderStatus.Ready;

        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
    private static CourierResponse MapToCourierResponse(Courier courier)
    {
        return new CourierResponse(
            Id: courier.Id,
            UserId: courier.UserId,
            VehicleType: courier.VehicleType,
            LicensePlate: courier.LicensePlate,
            IsAvailable: courier.IsAvailable,
            IsActive: courier.IsActive,
            CurrentLatitude: courier.CurrentLatitude,
            CurrentLongitude: courier.CurrentLongitude,
            LastLocationUpdate: courier.LastLocationUpdate,
            TotalDeliveries: courier.TotalDeliveries,
            Rating: courier.Rating,
            CreatedAt: courier.CreatedAt);
    }
}
