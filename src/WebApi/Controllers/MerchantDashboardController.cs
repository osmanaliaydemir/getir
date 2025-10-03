using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Merchants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Merchant dashboard controller for merchant dashboard operations
/// </summary>
[ApiController]
[Route("api/v1/merchants/{merchantId:guid}/[controller]")]
[Tags("Merchant Dashboard")]
[Authorize]
[Authorize(Roles = "Admin,MerchantOwner")]
public class MerchantDashboardController : BaseController
{
    private readonly IMerchantDashboardService _merchantDashboardService;

    public MerchantDashboardController(IMerchantDashboardService merchantDashboardService)
    {
        _merchantDashboardService = merchantDashboardService;
    }

    /// <summary>
    /// Get dashboard overview
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Dashboard data</returns>
    [HttpGet]
    [ProducesResponseType(typeof(MerchantDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMerchantDashboard(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _merchantDashboardService.GetDashboardAsync(merchantId, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get recent orders
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="limit">Number of orders to return</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Recent orders</returns>
    [HttpGet("recent-orders")]
    [ProducesResponseType(typeof(List<RecentOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecentOrders(
        [FromRoute] Guid merchantId,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _merchantDashboardService.GetRecentOrdersAsync(merchantId, userId, limit, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get top products
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="limit">Number of products to return</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Top products</returns>
    [HttpGet("top-products")]
    [ProducesResponseType(typeof(List<TopProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTopProducts(
        [FromRoute] Guid merchantId,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _merchantDashboardService.GetTopProductsAsync(merchantId, userId, limit, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get performance metrics
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Performance metrics</returns>
    [HttpGet("performance")]
    [ProducesResponseType(typeof(MerchantPerformanceMetrics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMerchantPerformanceMetrics(
        [FromRoute] Guid merchantId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _merchantDashboardService.GetPerformanceMetricsAsync(merchantId, userId, startDate, endDate, ct);
        return ToActionResult(result);
    }
}
