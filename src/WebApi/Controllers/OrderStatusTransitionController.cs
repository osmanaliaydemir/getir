using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Orders;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Controller for order status transition management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Order Status Transitions")]
[Authorize]
public class OrderStatusTransitionController : BaseController
{
    private readonly IOrderStatusTransitionService _transitionService;
    private readonly IOrderStatusValidatorService _validatorService;
    private readonly IUnitOfWork _unitOfWork;

    public OrderStatusTransitionController(
        IOrderStatusTransitionService transitionService,
        IOrderStatusValidatorService validatorService,
        IUnitOfWork unitOfWork)
    {
        _transitionService = transitionService;
        _validatorService = validatorService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Change order status with validation and audit logging
    /// </summary>
    /// <param name="request">Status change request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result of status change</returns>
    [HttpPost("change-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangeOrderStatus(
        [FromBody] ChangeOrderStatusRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var userRole = GetCurrentUserRole() ?? "Customer";
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var result = await _transitionService.ChangeOrderStatusAsync(
            request, userId, userRole, ipAddress, userAgent, ct);

        return ToActionResult(result);
    }

    /// <summary>
    /// Rollback the last status change
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="reason">Rollback reason</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result of rollback</returns>
    [HttpPost("rollback/{orderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RollbackStatusChange(
        [FromRoute] Guid orderId,
        [FromQuery] string? reason = null,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var userRole = GetCurrentUserRole() ?? "Customer";

        var result = await _transitionService.RollbackLastStatusChangeAsync(
            orderId, userId, userRole, reason, ct);

        return ToActionResult(result);
    }

    /// <summary>
    /// Get order status transition history
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Status transition history</returns>
    [HttpGet("history/{orderId}")]
    [ProducesResponseType(typeof(List<OrderStatusTransitionLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrderStatusHistory(
        [FromRoute] Guid orderId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _transitionService.GetOrderStatusHistoryAsync(orderId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get available status transitions for an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Available transitions</returns>
    [HttpGet("available-transitions/{orderId}")]
    [ProducesResponseType(typeof(List<OrderStatusTransitionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAvailableTransitions(
        [FromRoute] Guid orderId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var userRole = GetCurrentUserRole() ?? "Customer";

        var result = await _transitionService.GetAvailableTransitionsAsync(
            orderId, userId, userRole, ct);

        return ToActionResult(result);
    }

    /// <summary>
    /// Validate order status transition
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="newStatus">New status to validate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate/{orderId}")]
    [ProducesResponseType(typeof(OrderStatusValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateStatusTransition(
        [FromRoute] Guid orderId,
        [FromBody] OrderStatus newStatus,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var userRole = GetCurrentUserRole() ?? "Customer";

        // Get current order status
        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken: ct);

        if (order == null)
        {
            return BadRequest(new OrderStatusValidationResponse(
                false, "Order not found", "ORDER_NOT_FOUND", new List<string>(), new List<OrderStatusTransitionResponse>()));
        }

        // Validate transition
        var validationResult = await _validatorService.ValidateStatusTransitionAsync(
            orderId, order.Status, newStatus, userId, userRole, ct);

        // Get required data
        var requiredDataResult = await _validatorService.GetRequiredTransitionDataAsync(
            order.Status, newStatus, ct);

        // Get available transitions
        var availableTransitionsResult = await _transitionService.GetAvailableTransitionsAsync(
            orderId, userId, userRole, ct);

        var response = new OrderStatusValidationResponse(
            validationResult.Success,
            validationResult.Error,
            validationResult.ErrorCode,
            requiredDataResult.Success ? requiredDataResult.Value : new List<string>(),
            availableTransitionsResult.Success ? availableTransitionsResult.Value : new List<OrderStatusTransitionResponse>());

        return Ok(response);
    }

    /// <summary>
    /// Bulk change order statuses
    /// </summary>
    /// <param name="request">Bulk status change request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Bulk operation result</returns>
    [HttpPost("bulk-change")]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(BulkOrderStatusChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkChangeOrderStatus(
        [FromBody] BulkOrderStatusChangeRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var userRole = GetCurrentUserRole() ?? "Customer";
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var results = new List<OrderStatusChangeResult>();
        int successCount = 0;
        int failureCount = 0;

        foreach (var orderId in request.OrderIds)
        {
            var changeRequest = new ChangeOrderStatusRequest(
                orderId, request.NewStatus, request.Reason, request.Notes);

            var result = await _transitionService.ChangeOrderStatusAsync(
                changeRequest, userId, userRole, ipAddress, userAgent, ct);

            if (result.Success)
            {
                successCount++;
                results.Add(new OrderStatusChangeResult(orderId, true, null, null));
            }
            else
            {
                failureCount++;
                results.Add(new OrderStatusChangeResult(orderId, false, result.Error, result.ErrorCode));
            }
        }

        var response = new BulkOrderStatusChangeResponse(successCount, failureCount, results);
        return Ok(response);
    }
}
