using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Coupons;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Endpoints;

public static class CouponEndpoints
{
    public static void MapCouponEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/coupons")
            .WithTags("Coupons");

        group.MapPost("/validate", async (
            [FromBody] ValidateCouponRequest request,
            [FromServices] ICouponService couponService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var result = await couponService.ValidateCouponAsync(userId.Value, request, ct);
            return result.ToIResult();
        })
        .WithName("ValidateCoupon")
        .RequireAuthorization()
        .Produces<CouponValidationResponse>(200);

        group.MapPost("/", async (
            [FromBody] CreateCouponRequest request,
            [FromServices] ICouponService couponService,
            CancellationToken ct) =>
        {
            var result = await couponService.CreateCouponAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("CreateCoupon")
        .RequireAuthorization()
        .Produces<CouponResponse>(200);

        group.MapGet("/", async (
            [AsParameters] PaginationQuery query,
            [FromServices] ICouponService couponService,
            CancellationToken ct) =>
        {
            var result = await couponService.GetCouponsAsync(query, ct);
            return result.ToIResult();
        })
        .WithName("GetCoupons")
        .Produces<PagedResult<CouponResponse>>(200);
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }
}
