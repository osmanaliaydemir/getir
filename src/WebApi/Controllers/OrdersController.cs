using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Siparişleri yönetmek için sipariş controller'ı
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Orders")]
[Authorize]
public class OrdersController : BaseController
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Merchant'ın siparişlerini getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="status">Sipariş durumu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış sipariş listesi</returns>
    [HttpGet("merchantorder")]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(PagedResult<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMerchantOrders(
        [FromQuery] PaginationQuery query,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // Merchant ID'yi session'dan al (MerchantPortal'da session kullanılıyor)
        // Bu endpoint sadece MerchantPortal tarafından kullanılıyor
        var result = await _orderService.GetMerchantOrdersAsync(userId, query, status, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Merchant'ın bekleyen siparişlerini getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış bekleyen sipariş listesi</returns>
    [HttpGet("merchantorder/pending")]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(PagedResult<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    /// Sipariş detaylarını getir
    /// </summary>
    /// <param name="orderId">Sipariş ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sipariş detayları</returns>
    [HttpGet("merchantorder/{orderId:guid}")]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(OrderDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrderDetails(
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
    /// <param name="orderId">Sipariş ID'si</param>
    /// <param name="request">Sipariş durumu güncelleme talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenmiş sipariş</returns>
    [HttpPut("merchantorder/{orderId:guid}/status")]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
}
