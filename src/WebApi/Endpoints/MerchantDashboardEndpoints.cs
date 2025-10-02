using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Merchants;
using Getir.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Getir.WebApi.Endpoints;

public static class MerchantDashboardEndpoints
{
    public static void MapMerchantDashboardEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/merchants/{merchantId:guid}/dashboard")
            .WithTags("Merchant Dashboard")
            .RequireAuthorization()
            .RequireRole("Admin", "MerchantOwner");

        // Get dashboard overview
        group.MapGet("/", async (
            [FromRoute] Guid merchantId,
            ClaimsPrincipal user,
            [FromServices] IMerchantDashboardService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetDashboardAsync(merchantId, userId, ct);
            return result.ToIResult();
        })
        .WithName("GetMerchantDashboard")
        .Produces<MerchantDashboardResponse>(200)
        .Produces(403)
        .Produces(404);

        // Get recent orders
        group.MapGet("/recent-orders", async (
            [FromRoute] Guid merchantId,
            ClaimsPrincipal user,
            [FromServices] IMerchantDashboardService service,
            CancellationToken ct,
            [FromQuery] int limit = 10) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetRecentOrdersAsync(merchantId, userId, limit, ct);
            return result.ToIResult();
        })
        .WithName("GetRecentOrders")
        .Produces<List<RecentOrderResponse>>(200)
        .Produces(403)
        .Produces(404);

        // Get top products
        group.MapGet("/top-products", async (
            [FromRoute] Guid merchantId,
            ClaimsPrincipal user,
            [FromServices] IMerchantDashboardService service,
            CancellationToken ct,
            [FromQuery] int limit = 10) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetTopProductsAsync(merchantId, userId, limit, ct);
            return result.ToIResult();
        })
        .WithName("GetTopProducts")
        .Produces<List<TopProductResponse>>(200)
        .Produces(403)
        .Produces(404);

        // Get performance metrics
        group.MapGet("/performance", async (
            [FromRoute] Guid merchantId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            ClaimsPrincipal user,
            [FromServices] IMerchantDashboardService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetPerformanceMetricsAsync(merchantId, userId, startDate, endDate, ct);
            return result.ToIResult();
        })
        .WithName("GetMerchantPerformanceMetrics")
        .Produces<MerchantPerformanceMetrics>(200)
        .Produces(403)
        .Produces(404);
    }
}
