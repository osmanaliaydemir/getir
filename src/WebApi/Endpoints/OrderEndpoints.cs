using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Orders;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapPost("/", async (
            [FromBody] CreateOrderRequest request,
            [FromServices] IOrderService orderService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await orderService.CreateOrderAsync(userId, request, ct);
            return result.ToIResult();
        })
        .Produces<OrderResponse>(200)
        .Produces(400);

        group.MapGet("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IOrderService orderService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await orderService.GetOrderByIdAsync(id, userId, ct);
            return result.ToIResult();
        })
        .Produces<OrderResponse>(200)
        .Produces(404);

        group.MapGet("/", async (
            [AsParameters] PaginationQuery query,
            [FromServices] IOrderService orderService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await orderService.GetUserOrdersAsync(userId, query, ct);
            return result.ToIResult();
        })
        .Produces<PagedResult<OrderResponse>>(200);
    }
}
