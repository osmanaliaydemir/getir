using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Coupons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Kuponları yönetmek için kupon controller'ı
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Coupons")]
public class CouponController : BaseController
{
    private readonly ICouponService _couponService;

    public CouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    /// <summary>
    /// Kupon kodunu doğrula
    /// </summary>
    /// <param name="request">Kupon doğrulama talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kupon doğrulama yanıtı</returns>
    [HttpPost("validate")]
    [Authorize]
    [ProducesResponseType(typeof(CouponValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateCoupon(
        [FromBody] ValidateCouponRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _couponService.ValidateCouponAsync(userId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Yeni kupon oluştur
    /// </summary>
    /// <param name="request">Kupon oluşturma talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan kupon</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CouponResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCoupon(
        [FromBody] CreateCouponRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _couponService.CreateCouponAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sayfalama ile kuponları getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış kupon listesi</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CouponResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCoupons(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _couponService.GetCouponsAsync(query, ct);
        return ToActionResult(result);
    }
}
