using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Orders;

public interface IOrderStatusTransitionService
{
    /// <summary>
    /// Changes order status with full validation and audit logging
    /// </summary>
    Task<Result> ChangeOrderStatusAsync(
        ChangeOrderStatusRequest request,
        Guid userId,
        string userRole,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the last status change
    /// </summary>
    Task<Result> RollbackLastStatusChangeAsync(
        Guid orderId,
        Guid userId,
        string userRole,
        string? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets order status transition history
    /// </summary>
    Task<Result<List<OrderStatusTransitionLogResponse>>> GetOrderStatusHistoryAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available status transitions for an order
    /// </summary>
    Task<Result<List<OrderStatusTransitionResponse>>> GetAvailableTransitionsAsync(
        Guid orderId,
        Guid userId,
        string userRole,
        CancellationToken cancellationToken = default);
}
