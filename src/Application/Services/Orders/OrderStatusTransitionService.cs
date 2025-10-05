using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Orders;

public class OrderStatusTransitionService : BaseService, IOrderStatusTransitionService
{
    private readonly IOrderStatusValidatorService _validatorService;
    private readonly ISignalRService? _signalRService;
    private new readonly ILogger<OrderStatusTransitionService> _logger;

    public OrderStatusTransitionService(
        IUnitOfWork unitOfWork,
        ILogger<OrderStatusTransitionService> logger,
        ILoggingService loggingService,
        ICacheService cacheService,
        IOrderStatusValidatorService validatorService,
        ISignalRService? signalRService = null) 
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _validatorService = validatorService;
        _signalRService = signalRService;
        _logger = logger;
    }

    public async Task<Result> ChangeOrderStatusAsync(
        ChangeOrderStatusRequest request,
        Guid userId,
        string userRole,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Get current order
            var order = await _unitOfWork.ReadRepository<Order>()
                .FirstOrDefaultAsync(o => o.Id == request.OrderId,
                    include: "Merchant,User,Courier",
                    cancellationToken: cancellationToken);

            if (order == null)
            {
                return Result.Fail("Order not found", "ORDER_NOT_FOUND");
            }

            var fromStatus = order.Status;

            // 2. Validate transition
            var validationResult = await _validatorService.ValidateStatusTransitionAsync(
                request.OrderId, fromStatus, request.NewStatus, userId, userRole, cancellationToken);

            if (!validationResult.Success)
            {
                return validationResult;
            }

            // 3. Begin transaction
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // 4. Update order status
                order.Status = request.NewStatus;
                order.UpdatedAt = DateTime.UtcNow;

                // Update specific fields based on status
                await UpdateOrderFieldsForStatusAsync(order, request.NewStatus, request.AdditionalData, cancellationToken);

                _unitOfWork.Repository<Order>().Update(order);

                // 5. Create audit log
                var transitionLog = new OrderStatusTransitionLog
                {
                    Id = Guid.NewGuid(),
                    OrderId = request.OrderId,
                    FromStatus = fromStatus,
                    ToStatus = request.NewStatus,
                    ChangedBy = userId,
                    ChangedByRole = userRole,
                    Reason = request.Reason,
                    Notes = request.Notes,
                    ChangedAt = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                await _unitOfWork.Repository<OrderStatusTransitionLog>().AddAsync(transitionLog, cancellationToken);

                // 6. Commit transaction
                await _unitOfWork.CommitAsync(cancellationToken);

                // 7. Send notifications
                await SendStatusChangeNotificationsAsync(order, fromStatus, request.NewStatus, cancellationToken);

                _logger.LogInformation("Order {OrderId} status changed from {FromStatus} to {ToStatus} by {UserId} ({UserRole})",
                    request.OrderId, fromStatus, request.NewStatus, userId, userRole);

                return Result.Ok();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing order status");
            return Result.Fail("Failed to change order status", "STATUS_CHANGE_ERROR");
        }
    }

    public async Task<Result> RollbackLastStatusChangeAsync(
        Guid orderId,
        Guid userId,
        string userRole,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Get the last status transition
            var lastTransition = await _unitOfWork.ReadRepository<OrderStatusTransitionLog>()
                .FirstOrDefaultAsync(log => log.OrderId == orderId && !log.IsRollback,
                    cancellationToken: cancellationToken);

            if (lastTransition == null)
            {
                return Result.Fail("No status transition found to rollback", "NO_TRANSITION_TO_ROLLBACK");
            }

            // 2. Get current order
            var order = await _unitOfWork.ReadRepository<Order>()
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken: cancellationToken);

            if (order == null)
            {
                return Result.Fail("Order not found", "ORDER_NOT_FOUND");
            }

            // 3. Validate rollback permission
            var permissionResult = await _validatorService.ValidateUserPermissionAsync(
                orderId, lastTransition.FromStatus, userId, userRole, cancellationToken);

            if (!permissionResult.Success)
            {
                return permissionResult;
            }

            // 4. Begin transaction
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // 5. Rollback order status
                order.Status = lastTransition.FromStatus;
                order.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Order>().Update(order);

                // 6. Create rollback audit log
                var rollbackLog = new OrderStatusTransitionLog
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    FromStatus = lastTransition.ToStatus,
                    ToStatus = lastTransition.FromStatus,
                    ChangedBy = userId,
                    ChangedByRole = userRole,
                    Reason = reason ?? "Rollback",
                    Notes = $"Rollback of transition {lastTransition.Id}",
                    ChangedAt = DateTime.UtcNow,
                    IsRollback = true,
                    RollbackFromLogId = lastTransition.Id
                };

                await _unitOfWork.Repository<OrderStatusTransitionLog>().AddAsync(rollbackLog, cancellationToken);

                // 7. Commit transaction
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Order {OrderId} status rolled back from {FromStatus} to {ToStatus} by {UserId}",
                    orderId, lastTransition.ToStatus, lastTransition.FromStatus, userId);

                return Result.Ok();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back order status");
            return Result.Fail("Failed to rollback order status", "ROLLBACK_ERROR");
        }
    }

    public async Task<Result<List<OrderStatusTransitionLogResponse>>> GetOrderStatusHistoryAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var transitions = await _unitOfWork.ReadRepository<OrderStatusTransitionLog>()
                .ListAsync(log => log.OrderId == orderId,
                    orderBy: log => log.ChangedAt,
                    ascending: true,
                    include: "ChangedByUser",
                    cancellationToken: cancellationToken);

            var responses = transitions.Select(log => new OrderStatusTransitionLogResponse(
                log.Id,
                log.OrderId,
                log.FromStatus,
                log.ToStatus,
                log.FromStatus.ToStringValue(),
                log.ToStatus.ToStringValue(),
                log.ChangedBy,
                log.ChangedByRole,
                log.Reason,
                log.Notes,
                log.ChangedAt,
                log.IpAddress,
                log.IsRollback,
                log.RollbackFromLogId)).ToList();

            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order status history");
            return Result.Fail<List<OrderStatusTransitionLogResponse>>("Failed to get order status history", "ERROR");
        }
    }

    public async Task<Result<List<OrderStatusTransitionResponse>>> GetAvailableTransitionsAsync(
        Guid orderId,
        Guid userId,
        string userRole,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validStatuses = await _validatorService.GetValidNextStatusesAsync(
                orderId, userId, userRole, cancellationToken);

            if (!validStatuses.Success)
            {
                return Result.Fail<List<OrderStatusTransitionResponse>>(validStatuses.Error ?? "Failed to get valid statuses", validStatuses.ErrorCode);
            }

            var transitions = new List<OrderStatusTransitionResponse>();

            foreach (var status in validStatuses.Value!)
            {
                var requiredDataResult = await _validatorService.GetRequiredTransitionDataAsync(
                    OrderStatus.Pending, status, cancellationToken); // We'll get current status from order

                var transition = new OrderStatusTransitionResponse(
                    status,
                    status.ToStringValue(),
                    GetStatusDescription(status),
                    true,
                    requiredDataResult.Success ? requiredDataResult.Value! : new List<string>());

                transitions.Add(transition);
            }

            return Result.Ok(transitions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available transitions");
            return Result.Fail<List<OrderStatusTransitionResponse>>("Failed to get available transitions", "ERROR");
        }
    }

    private async Task UpdateOrderFieldsForStatusAsync(
        Order order,
        OrderStatus newStatus,
        Dictionary<string, object>? additionalData,
        CancellationToken cancellationToken)
    {
        switch (newStatus)
        {
            case OrderStatus.Confirmed:
                if (additionalData?.ContainsKey("EstimatedPreparationTime") == true)
                {
                    // Update estimated preparation time
                }
                break;

            case OrderStatus.OnTheWay:
                if (additionalData?.ContainsKey("CourierId") == true)
                {
                    order.CourierId = (Guid)additionalData["CourierId"];
                }
                if (additionalData?.ContainsKey("EstimatedDeliveryTime") == true)
                {
                    order.EstimatedDeliveryTime = (DateTime)additionalData["EstimatedDeliveryTime"];
                }
                break;

            case OrderStatus.Delivered:
                order.ActualDeliveryTime = DateTime.UtcNow;
                break;

            case OrderStatus.Cancelled:
                if (additionalData?.ContainsKey("CancellationReason") == true)
                {
                    order.CancellationReason = additionalData["CancellationReason"].ToString();
                }
                break;
        }
    }

    private async Task SendStatusChangeNotificationsAsync(
        Order order,
        OrderStatus fromStatus,
        OrderStatus toStatus,
        CancellationToken cancellationToken)
    {
        if (_signalRService == null) return;

        try
        {
            var message = GetStatusChangeMessage(order, fromStatus, toStatus);
            
            // Notify customer
            await _signalRService.SendOrderStatusUpdateAsync(
                order.Id,
                order.UserId,
                toStatus.ToStringValue(),
                message);

            // Notify merchant if needed
            if (ShouldNotifyMerchant(toStatus))
            {
                var merchantNotification = new RealtimeNotificationEvent(
                    Guid.NewGuid(),
                    "Order Status Update",
                    $"Order {order.OrderNumber} status changed to {toStatus.ToStringValue()}",
                    "OrderStatusUpdate",
                    DateTime.UtcNow,
                    false,
                    new Dictionary<string, object> { { "OrderId", order.Id } });

                await _signalRService.SendRealtimeNotificationAsync(merchantNotification);
            }

            // Notify courier if needed
            if (order.CourierId.HasValue && ShouldNotifyCourier(toStatus))
            {
                var courierNotification = new RealtimeNotificationEvent(
                    Guid.NewGuid(),
                    "Order Status Update",
                    $"Order {order.OrderNumber} status changed to {toStatus.ToStringValue()}",
                    "OrderStatusUpdate",
                    DateTime.UtcNow,
                    false,
                    new Dictionary<string, object> { { "OrderId", order.Id } });

                await _signalRService.SendRealtimeNotificationAsync(courierNotification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending status change notifications");
        }
    }

    private string GetStatusChangeMessage(Order order, OrderStatus fromStatus, OrderStatus toStatus)
    {
        return toStatus switch
        {
            OrderStatus.Confirmed => $"Your order {order.OrderNumber} has been confirmed by {order.Merchant.Name}",
            OrderStatus.Preparing => $"Your order {order.OrderNumber} is being prepared by {order.Merchant.Name}",
            OrderStatus.Ready => $"Your order {order.OrderNumber} is ready for pickup",
            OrderStatus.OnTheWay => $"Your order {order.OrderNumber} is on the way to you",
            OrderStatus.Delivered => $"Your order {order.OrderNumber} has been delivered",
            OrderStatus.Cancelled => $"Your order {order.OrderNumber} has been cancelled",
            _ => $"Your order {order.OrderNumber} status has been updated"
        };
    }

    private bool ShouldNotifyMerchant(OrderStatus status)
    {
        return status is OrderStatus.Cancelled or OrderStatus.Delivered;
    }

    private bool ShouldNotifyCourier(OrderStatus status)
    {
        return status is OrderStatus.Ready or OrderStatus.Cancelled;
    }

    private string GetStatusDescription(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Order is waiting for confirmation",
            OrderStatus.Confirmed => "Order has been confirmed by merchant",
            OrderStatus.Preparing => "Order is being prepared",
            OrderStatus.Ready => "Order is ready for pickup",
            OrderStatus.OnTheWay => "Order is on the way to customer",
            OrderStatus.Delivered => "Order has been delivered",
            OrderStatus.Cancelled => "Order has been cancelled",
            _ => "Unknown status"
        };
    }
}
