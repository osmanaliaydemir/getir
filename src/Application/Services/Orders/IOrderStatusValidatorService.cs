using Getir.Application.Common;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Orders;

public interface IOrderStatusValidatorService
{
    /// <summary>
    /// Validates if an order status transition is allowed
    /// </summary>
    Task<Result> ValidateStatusTransitionAsync(
        Guid orderId,
        OrderStatus fromStatus,
        OrderStatus toStatus,
        Guid userId,
        string userRole,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all valid next statuses for an order
    /// </summary>
    Task<Result<List<OrderStatus>>> GetValidNextStatusesAsync(
        Guid orderId,
        Guid userId,
        string userRole,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has permission to change order status
    /// </summary>
    Task<Result> ValidateUserPermissionAsync(
        Guid orderId,
        OrderStatus toStatus,
        Guid userId,
        string userRole,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transition reason requirements for a status change
    /// </summary>
    Task<Result<List<string>>> GetRequiredTransitionDataAsync(
        OrderStatus fromStatus,
        OrderStatus toStatus,
        CancellationToken cancellationToken = default);
}
