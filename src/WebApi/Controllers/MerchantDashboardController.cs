using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Merchants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Mağaza dashboard işlemleri için mağaza dashboard controller'ı
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
    /// Dashboard genel görünümünü getirir
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Dashboard verileri</returns>
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
    /// Son siparişleri getirir
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="limit">Döndürülecek sipariş sayısı</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Son siparişler</returns>
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
    /// En çok satan ürünleri getirir
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="limit">Döndürülecek ürün sayısı</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>En çok satan ürünler</returns>
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
    /// Performans metriklerini getirir
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Performans metrikleri</returns>
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
