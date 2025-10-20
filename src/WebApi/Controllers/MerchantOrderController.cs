using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Mağaza sipariş yönetimi için mağaza sipariş controller'ı
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
    /// Mağaza siparişlerini getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="status">Sipariş durumu filtresi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış siparişler</returns>
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
    /// Sipariş istatistiklerini getir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sipariş istatistikleri</returns>
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
    /// Sipariş detaylarını getir
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sipariş detayları</returns>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMerchantOrderDetails(
        [FromRoute] Guid orderId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _orderService.GetMerchantOrderDetailsAsync(orderId, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sipariş durumunu güncelle
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="request">Sipariş durumu güncelleme isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenen sipariş</returns>
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

        var result = await _orderService.UpdateOrderStatusAsync(orderId, request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sipariş analitiklerini getir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sipariş analitikleri</returns>
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

        var result = await _orderService.GetOrderAnalyticsAsync(userId, startDate, endDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Bekleyen siparişleri getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış bekleyen siparişler</returns>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(PagedResult<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPendingOrders(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _orderService.GetPendingOrdersAsync(userId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sipariş zaman çizelgesini getir
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sipariş zaman çizelgesi</returns>
    [HttpGet("{orderId:guid}/timeline")]
    [ProducesResponseType(typeof(IEnumerable<OrderTimelineResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderTimeline(
        [FromRoute] Guid orderId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _orderService.GetOrderTimelineAsync(orderId, userId, ct);
        return ToActionResult(result);
    }
}
