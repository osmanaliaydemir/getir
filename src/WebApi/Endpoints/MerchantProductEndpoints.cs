using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Getir.Application.Services.Products;
using Getir.Application.DTO;
using Getir.Application.Common;
using System.Security.Claims;
using Getir.WebApi.Extensions;

namespace Getir.WebApi.Endpoints;

public static class MerchantProductEndpoints
{
    public static void MapMerchantProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/merchants/products")
            .WithTags("Merchant Products")
            .RequireAuthorization();

        // Get my products
        group.MapGet("/", async (
            ClaimsPrincipal user,
            [FromServices] IProductService service,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var userId = user.GetUserId();
            var query = new PaginationQuery { Page = page, PageSize = pageSize };
            var result = await service.GetMyProductsAsync(userId, query, ct);
            return result.ToIResult();
        })
        .WithName("GetMyProducts")
        .Produces<PagedResult<ProductResponse>>(200)
        .Produces(404)
        .RequireAuthorization();

        // Create new product
        group.MapPost("/", async (
            [FromBody] CreateProductRequest request,
            ClaimsPrincipal user,
            [FromServices] IProductService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.CreateMyProductAsync(request, userId, ct);
            return result.ToIResult();
        })
        .WithName("CreateMyProduct")
        .Produces<ProductResponse>(201)
        .Produces(400)
        .Produces(403)
        .RequireAuthorization();

        // Update my product
        group.MapPut("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateProductRequest request,
            ClaimsPrincipal user,
            [FromServices] IProductService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.UpdateMyProductAsync(id, request, userId, ct);
            return result.ToIResult();
        })
        .WithName("UpdateMyProduct")
        .Produces<ProductResponse>(200)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Delete my product
        group.MapDelete("/{id:guid}", async (
            [FromRoute] Guid id,
            ClaimsPrincipal user,
            [FromServices] IProductService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.DeleteMyProductAsync(id, userId, ct);
            return result.ToIResult();
        })
        .WithName("DeleteMyProduct")
        .Produces(200)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Update product stock
        group.MapPut("/{id:guid}/stock", async (
            [FromRoute] Guid id,
            [FromBody] UpdateProductStockRequest request,
            ClaimsPrincipal user,
            [FromServices] IProductService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.UpdateProductStockAsync(id, request.NewStockQuantity, userId, ct);
            return result.ToIResult();
        })
        .WithName("UpdateProductStock")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Toggle product availability
        group.MapPut("/{id:guid}/availability", async (
            [FromRoute] Guid id,
            ClaimsPrincipal user,
            [FromServices] IProductService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.ToggleProductAvailabilityAsync(id, userId, ct);
            return result.ToIResult();
        })
        .WithName("ToggleProductAvailability")
        .Produces(200)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Bulk update product order
        group.MapPut("/order", async (
            [FromBody] BulkUpdateProductOrderRequest request,
            ClaimsPrincipal user,
            [FromServices] IProductService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.BulkUpdateProductOrderAsync(request.Products, userId, ct);
            return result.ToIResult();
        })
        .WithName("BulkUpdateProductOrder")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .RequireAuthorization();
    }
}
