using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Getir.Application.Services.Orders;
using Getir.Application.DTO;
using Getir.Application.Common;
using System.Security.Claims;
using Getir.WebApi.Extensions;

namespace Getir.WebApi.Endpoints;

public static class MerchantOrderEndpoints
{
    public static void MapMerchantOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/merchants/orders")
            .WithTags("Merchant Orders")
            .RequireAuthorization();

        // Get merchant orders
        group.MapGet("/", async (
            ClaimsPrincipal user,
            [FromServices] IOrderService service,
            [AsParameters] PaginationQuery query,
            CancellationToken ct,
            [FromQuery] string? status = null) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetMerchantOrdersAsync(userId, query, status, ct);
            return result.ToIResult();
        })
        .WithName("GetMerchantOrders")
        .Produces<PagedResult<OrderResponse>>(200)
        .Produces(404)
        .RequireAuthorization();

        // Get order statistics
        group.MapGet("/statistics", async (
            ClaimsPrincipal user,
            [FromServices] IOrderService service,
            CancellationToken ct,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetOrderStatisticsAsync(userId, startDate, endDate, ct);
            return result.ToIResult();
        })
        .WithName("GetOrderStatistics")
        .Produces<OrderStatisticsResponse>(200)
        .Produces(404)
        .RequireAuthorization();

        // Accept order
        group.MapPut("/{id:guid}/accept", async (
            [FromRoute] Guid id,
            ClaimsPrincipal user,
            [FromServices] IOrderService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.AcceptOrderAsync(id, userId, ct);
            return result.ToIResult();
        })
        .WithName("AcceptOrderByMerchant")
        .Produces<OrderResponse>(200)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Reject order
        group.MapPut("/{id:guid}/reject", async (
            [FromRoute] Guid id,
            [FromBody] RejectOrderRequest request,
            ClaimsPrincipal user,
            [FromServices] IOrderService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.RejectOrderAsync(id, userId, request.Reason, ct);
            return result.ToIResult();
        })
        .WithName("RejectOrder")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Start preparing order
        group.MapPut("/{id:guid}/start-preparing", async (
            [FromRoute] Guid id,
            ClaimsPrincipal user,
            [FromServices] IOrderService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.StartPreparingOrderAsync(id, userId, ct);
            return result.ToIResult();
        })
        .WithName("StartPreparingOrder")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Mark order as ready
        group.MapPut("/{id:guid}/mark-ready", async (
            [FromRoute] Guid id,
            ClaimsPrincipal user,
            [FromServices] IOrderService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.MarkOrderAsReadyAsync(id, userId, ct);
            return result.ToIResult();
        })
        .WithName("MarkOrderAsReady")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Cancel order
        group.MapPut("/{id:guid}/cancel", async (
            [FromRoute] Guid id,
            [FromBody] CancelOrderRequest request,
            ClaimsPrincipal user,
            [FromServices] IOrderService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.CancelOrderAsync(id, userId, request.Reason, ct);
            return result.ToIResult();
        })
        .WithName("CancelOrder")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Get pending orders (convenience endpoint)
        group.MapGet("/pending", async (
            ClaimsPrincipal user,
            [FromServices] IOrderService service,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var userId = user.GetUserId();
            var query = new PaginationQuery { Page = page, PageSize = pageSize };
            var result = await service.GetMerchantOrdersAsync(userId, query, "Pending", ct);
            return result.ToIResult();
        })
        .WithName("GetPendingOrders")
        .Produces<PagedResult<OrderResponse>>(200)
        .Produces(404)
        .RequireAuthorization();

        // Get preparing orders (convenience endpoint)
        group.MapGet("/preparing", async (
            ClaimsPrincipal user,
            [FromServices] IOrderService service,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var userId = user.GetUserId();
            var query = new PaginationQuery { Page = page, PageSize = pageSize };
            var result = await service.GetMerchantOrdersAsync(userId, query, "Preparing", ct);
            return result.ToIResult();
        })
        .WithName("GetPreparingOrders")
        .Produces<PagedResult<OrderResponse>>(200)
        .Produces(404)
        .RequireAuthorization();
    }
}
