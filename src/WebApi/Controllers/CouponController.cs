using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Coupons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Coupon controller for managing coupons
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
    /// Validate coupon code
    /// </summary>
    /// <param name="request">Validate coupon request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Coupon validation response</returns>
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
    /// Create a new coupon
    /// </summary>
    /// <param name="request">Create coupon request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created coupon</returns>
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
    /// Get coupons with pagination
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged list of coupons</returns>
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
