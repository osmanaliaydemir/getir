using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Products;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/products")
            .WithTags("Products");

        group.MapGet("/merchant/{merchantId:guid}", async (
            [FromRoute] Guid merchantId,
            [AsParameters] PaginationQuery query,
            [FromServices] IProductService productService,
            CancellationToken ct) =>
        {
            var result = await productService.GetProductsByMerchantAsync(merchantId, query, ct);
            return result.ToIResult();
        })
        .WithName("GetProductsByMerchant")
        .Produces<PagedResult<ProductResponse>>(200);

        group.MapGet("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IProductService productService,
            CancellationToken ct) =>
        {
            var result = await productService.GetProductByIdAsync(id, ct);
            return result.ToIResult();
        })
        .WithName("GetProductById")
        .Produces<ProductResponse>(200)
        .Produces(404);

        group.MapPost("/", async (
            [FromBody] CreateProductRequest request,
            [FromServices] IProductService productService,
            CancellationToken ct) =>
        {
            var result = await productService.CreateProductAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("CreateProduct")
        .RequireAuthorization()
        .Produces<ProductResponse>(200)
        .Produces(400);

        group.MapPut("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateProductRequest request,
            [FromServices] IProductService productService,
            CancellationToken ct) =>
        {
            var result = await productService.UpdateProductAsync(id, request, ct);
            return result.ToIResult();
        })
        .WithName("UpdateProduct")
        .RequireAuthorization()
        .Produces<ProductResponse>(200)
        .Produces(404);

        group.MapDelete("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IProductService productService,
            CancellationToken ct) =>
        {
            var result = await productService.DeleteProductAsync(id, ct);
            return result.ToIResult();
        })
        .WithName("DeleteProduct")
        .RequireAuthorization()
        .Produces(200)
        .Produces(404);
    }
}
