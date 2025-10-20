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
public class OrderController : BaseController
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Yeni sipariş oluştur
    /// </summary>
    /// <param name="request">Sipariş oluşturma talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan sipariş</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _orderService.CreateOrderAsync(userId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// ID'ye göre sipariş getir
    /// </summary>
    /// <param name="id">Sipariş ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sipariş detayları</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrderById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _orderService.GetOrderByIdAsync(id, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sayfalama ile kullanıcı siparişlerini getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış kullanıcı siparişleri listesi</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserOrders(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _orderService.GetUserOrdersAsync(userId, query, ct);
        return ToActionResult(result);
    }
}
