using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Cart;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Endpoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/cart")
            .WithTags("Shopping Cart")
            .RequireAuthorization();

        group.MapGet("/", async (
            [FromServices] ICartService cartService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var result = await cartService.GetCartAsync(userId.Value, ct);
            return result.ToIResult();
        })
        .WithName("GetCart")
        .Produces<CartResponse>(200);

        group.MapPost("/items", async (
            [FromBody] AddToCartRequest request,
            [FromServices] ICartService cartService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var result = await cartService.AddItemAsync(userId.Value, request, ct);
            return result.ToIResult();
        })
        .WithName("AddToCart")
        .Produces<CartItemResponse>(200);

        group.MapPut("/items/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateCartItemRequest request,
            [FromServices] ICartService cartService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var result = await cartService.UpdateItemAsync(userId.Value, id, request, ct);
            return result.ToIResult();
        })
        .WithName("UpdateCartItem")
        .Produces<CartItemResponse>(200);

        group.MapDelete("/items/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] ICartService cartService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var result = await cartService.RemoveItemAsync(userId.Value, id, ct);
            return result.ToIResult();
        })
        .WithName("RemoveCartItem")
        .Produces(200);

        group.MapDelete("/clear", async (
            [FromServices] ICartService cartService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var result = await cartService.ClearCartAsync(userId.Value, ct);
            return result.ToIResult();
        })
        .WithName("ClearCart")
        .Produces(200);
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }
}
