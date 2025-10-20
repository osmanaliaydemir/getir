using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Orders;

/// <summary>
/// Sipariş durum validasyon servisi: geçiş kuralları, role bazlı yetki, business rule kontrolleri.
/// </summary>
public class OrderStatusValidatorService : BaseService, IOrderStatusValidatorService
{
    private new readonly ILogger<OrderStatusValidatorService> _logger;
    public OrderStatusValidatorService(IUnitOfWork unitOfWork, ILogger<OrderStatusValidatorService> logger, ILoggingService loggingService, ICacheService cacheService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _logger = logger;
    }
    /// <summary>
    /// Sipariş durum geçişinin geçerli olup olmadığını kontrol eder (geçiş kuralı, yetki, business rule).
    /// </summary>
    public async Task<Result> ValidateStatusTransitionAsync(Guid orderId, OrderStatus fromStatus, OrderStatus toStatus, Guid userId, string userRole, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Basic transition validation
            if (!fromStatus.CanTransitionTo(toStatus))
            {
                return Result.Fail($"Invalid status transition from {fromStatus} to {toStatus}", "INVALID_STATUS_TRANSITION");
            }

            // 2. Get order details
            var order = await _unitOfWork.ReadRepository<Order>()
                .FirstOrDefaultAsync(o => o.Id == orderId,
                    include: "Merchant,User,Courier",
                    cancellationToken: cancellationToken);

            if (order == null)
            {
                return Result.Fail("Order not found", "ORDER_NOT_FOUND");
            }

            // 3. Validate user permissions
            var permissionResult = await ValidateUserPermissionAsync(orderId, toStatus, userId, userRole, cancellationToken);
            if (!permissionResult.Success)
            {
                return permissionResult;
            }

            // 4. Business rule validations
            var businessRuleResult = await ValidateBusinessRulesAsync(order, fromStatus, toStatus, cancellationToken);
            if (!businessRuleResult.Success)
            {
                return businessRuleResult;
            }

            // 5. Check required data for transition
            var requiredDataResult = await GetRequiredTransitionDataAsync(fromStatus, toStatus, cancellationToken);
            if (requiredDataResult.Success && requiredDataResult.Value.Any())
            {
                // Additional validation for required data can be added here
                _logger.LogInformation("Required transition data: {RequiredData}", string.Join(", ", requiredDataResult.Value));
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating order status transition");
            return Result.Fail("Failed to validate status transition", "VALIDATION_ERROR");
        }
    }
    /// <summary>
    /// Sipariş için geçerli sonraki durumları getirir (mevcut durum ve role bazlı).
    /// </summary>
    public async Task<Result<List<OrderStatus>>> GetValidNextStatusesAsync(Guid orderId, Guid userId, string userRole, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _unitOfWork.ReadRepository<Order>()
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken: cancellationToken);

            if (order == null)
            {
                return Result.Fail<List<OrderStatus>>("Order not found", "ORDER_NOT_FOUND");
            }

            var validStatuses = new List<OrderStatus>();
            var allStatuses = Enum.GetValues<OrderStatus>();

            foreach (var status in allStatuses)
            {
                if (status == order.Status) continue; // Skip current status

                var validationResult = await ValidateStatusTransitionAsync(
                    orderId, order.Status, status, userId, userRole, cancellationToken);

                if (validationResult.Success)
                {
                    validStatuses.Add(status);
                }
            }

            return Result.Ok(validStatuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting valid next statuses");
            return Result.Fail<List<OrderStatus>>("Failed to get valid next statuses", "ERROR");
        }
    }
    /// <summary>
    /// Kullanıcının sipariş durumunu değiştirme yetkisi olup olmadığını kontrol eder (admin/merchant/courier/customer).
    /// </summary>
    public async Task<Result> ValidateUserPermissionAsync(Guid orderId, OrderStatus toStatus, Guid userId, string userRole, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _unitOfWork.ReadRepository<Order>()
                .FirstOrDefaultAsync(o => o.Id == orderId,
                    include: "Merchant,User,Courier",
                    cancellationToken: cancellationToken);

            if (order == null)
            {
                return Result.Fail("Order not found", "ORDER_NOT_FOUND");
            }

            // Role-based permission validation
            return userRole.ToLowerInvariant() switch
            {
                "admin" => Result.Ok(), // Admin can do anything
                "merchantowner" => ValidateMerchantOwnerPermission(order, toStatus, userId),
                "courier" => ValidateCourierPermission(order, toStatus, userId),
                "customer" => ValidateCustomerPermission(order, toStatus, userId),
                _ => Result.Fail("Invalid user role", "INVALID_ROLE")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user permission");
            return Result.Fail("Failed to validate user permission", "PERMISSION_ERROR");
        }
    }
    /// <summary>
    /// Durum geçişi için gerekli verileri getirir (CourierId, CancellationReason vb).
    /// </summary>
    public Task<Result<List<string>>> GetRequiredTransitionDataAsync(OrderStatus fromStatus, OrderStatus toStatus, CancellationToken cancellationToken = default)
    {
        var requiredData = new List<string>();

        switch ((fromStatus, toStatus))
        {
            case (OrderStatus.Pending, OrderStatus.Confirmed):
                requiredData.Add("EstimatedPreparationTime");
                break;
            case (OrderStatus.Confirmed, OrderStatus.Preparing):
                // No additional data required
                break;
            case (OrderStatus.Preparing, OrderStatus.Ready):
                // No additional data required
                break;
            case (OrderStatus.Ready, OrderStatus.OnTheWay):
                requiredData.Add("CourierId");
                requiredData.Add("EstimatedDeliveryTime");
                break;
            case (OrderStatus.OnTheWay, OrderStatus.Delivered):
                requiredData.Add("ActualDeliveryTime");
                requiredData.Add("DeliveryConfirmation");
                break;
            case (_, OrderStatus.Cancelled):
                requiredData.Add("CancellationReason");
                break;
        }

        return Task.FromResult(Result.Ok(requiredData));
    }
    private Task<Result> ValidateBusinessRulesAsync(Order order, OrderStatus fromStatus, OrderStatus toStatus, CancellationToken cancellationToken)
    {
        // Business rule validations
        switch ((fromStatus, toStatus))
        {
            case (OrderStatus.Pending, OrderStatus.Confirmed):
                // Check if merchant is active
                if (!order.Merchant.IsActive)
                {
                    return Task.FromResult(Result.Fail("Merchant is not active", "MERCHANT_INACTIVE"));
                }
                break;

            case (OrderStatus.Ready, OrderStatus.OnTheWay):
                // Check if courier is assigned
                if (order.CourierId == null)
                {
                    return Task.FromResult(Result.Fail("Courier must be assigned before marking as on the way", "COURIER_REQUIRED"));
                }
                break;

            case (OrderStatus.OnTheWay, OrderStatus.Delivered):
                // Check if delivery time is reasonable
                if (order.EstimatedDeliveryTime.HasValue &&
                    DateTime.UtcNow < order.EstimatedDeliveryTime.Value.AddMinutes(-30))
                {
                    return Task.FromResult(Result.Fail("Cannot mark as delivered before estimated delivery time", "TOO_EARLY_DELIVERY"));
                }
                break;

            case (_, OrderStatus.Cancelled):
                // Check if order can be cancelled
                if (order.Status == OrderStatus.Delivered)
                {
                    return Task.FromResult(Result.Fail("Cannot cancel a delivered order", "CANNOT_CANCEL_DELIVERED"));
                }
                break;
        }

        return Task.FromResult(Result.Ok());
    }
    private Result ValidateMerchantOwnerPermission(Order order, OrderStatus toStatus, Guid userId)
    {
        // Merchant owner can only change status for their own orders
        if (order.Merchant.OwnerId != userId)
        {
            return Result.Fail("Access denied - not your merchant's order", "ACCESS_DENIED");
        }

        // Merchant can only change to these statuses
        var allowedStatuses = new[] { OrderStatus.Confirmed, OrderStatus.Preparing, OrderStatus.Ready };
        if (!allowedStatuses.Contains(toStatus))
        {
            return Result.Fail($"Merchant cannot change status to {toStatus}", "INVALID_MERCHANT_ACTION");
        }

        return Result.Ok();
    }
    private Result ValidateCourierPermission(Order order, OrderStatus toStatus, Guid userId)
    {
        // Courier can only change status for assigned orders
        if (order.CourierId != userId)
        {
            return Result.Fail("Access denied - order not assigned to you", "ACCESS_DENIED");
        }

        // Courier can only change to these statuses
        var allowedStatuses = new[] { OrderStatus.OnTheWay, OrderStatus.Delivered };
        if (!allowedStatuses.Contains(toStatus))
        {
            return Result.Fail($"Courier cannot change status to {toStatus}", "INVALID_COURIER_ACTION");
        }

        return Result.Ok();
    }
    private Result ValidateCustomerPermission(Order order, OrderStatus toStatus, Guid userId)
    {
        // Customer can only change status for their own orders
        if (order.UserId != userId)
        {
            return Result.Fail("Access denied - not your order", "ACCESS_DENIED");
        }

        // Customer can only cancel orders
        if (toStatus != OrderStatus.Cancelled)
        {
            return Result.Fail("Customer can only cancel orders", "INVALID_CUSTOMER_ACTION");
        }

        // Customer can only cancel pending or confirmed orders
        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
        {
            return Result.Fail("Cannot cancel order in current status", "CANNOT_CANCEL");
        }

        return Result.Ok();
    }
}
