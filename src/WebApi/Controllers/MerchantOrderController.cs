using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Merchant order controller for merchant order management
/// </summary>
[ApiController]
[Route("api/v1/merchants/[controller]")]
[Tags("Merchant Orders")]
[Authorize]
public class MerchantOrderController : BaseController
{
    private readonly IOrderService _orderService;

    public MerchantOrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Get merchant orders
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="status">Order status filter</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged orders</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMerchantOrders(
        [FromQuery] PaginationQuery query,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _orderService.GetMerchantOrdersAsync(userId, query, status, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get order statistics
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Order statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(OrderStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _orderService.GetOrderStatisticsAsync(userId, startDate, endDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get order details
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Order details</returns>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMerchantOrderDetails(
        [FromRoute] Guid orderId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        return BadRequest("Get merchant order details functionality not implemented yet");
    }

    /// <summary>
    /// Update order status
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="request">Update order status request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated order</returns>
    [HttpPut("{orderId:guid}/status")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(
        [FromRoute] Guid orderId,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        return BadRequest("Update order status functionality not implemented yet");
    }

    /// <summary>
    /// Get order analytics
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Order analytics</returns>
    [HttpGet("analytics")]
    [ProducesResponseType(typeof(OrderAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        return BadRequest("Get order analytics functionality not implemented yet");
    }

    /// <summary>
    /// Get pending orders
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged pending orders</returns>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(PagedResult<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPendingOrders(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        return BadRequest("Get pending orders functionality not implemented yet");
    }

    /// <summary>
    /// Get order timeline
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Order timeline</returns>
    [HttpGet("{orderId:guid}/timeline")]
    [ProducesResponseType(typeof(IEnumerable<OrderTimelineResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderTimeline(
        [FromRoute] Guid orderId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        return BadRequest("Get order timeline functionality not implemented yet");
    }
}
