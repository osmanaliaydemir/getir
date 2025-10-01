using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.ProductCategories;
using Getir.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Getir.WebApi.Endpoints;

public static class ProductCategoryEndpoints
{
    public static void MapProductCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/product-categories")
            .WithTags("Product Categories");

        // Public - Get merchant's category tree
        group.MapGet("/merchant/{merchantId:guid}/tree", async (
            [FromRoute] Guid merchantId,
            [FromServices] IProductCategoryService service,
            CancellationToken ct) =>
        {
            var result = await service.GetMerchantCategoryTreeAsync(merchantId, ct);
            return result.ToIResult();
        })
        .WithName("GetMerchantCategoryTree")
        .Produces<List<ProductCategoryTreeResponse>>(200);

        group.MapGet("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IProductCategoryService service,
            CancellationToken ct) =>
        {
            var result = await service.GetProductCategoryByIdAsync(id, ct);
            return result.ToIResult();
        })
        .WithName("GetProductCategoryById")
        .Produces<ProductCategoryResponse>(200)
        .Produces(404);

        // Merchant Panel - Create category
        group.MapPost("/merchant/{merchantId:guid}", async (
            [FromRoute] Guid merchantId,
            [FromBody] CreateProductCategoryRequest request,
            ClaimsPrincipal user,
            [FromServices] IProductCategoryService service,
            CancellationToken ct) =>
        {
            // Merchant owner kontrolü endpoint'te yapılacak
            var result = await service.CreateProductCategoryAsync(request, merchantId, ct);
            return result.ToIResult();
        })
        .WithName("CreateProductCategory")
        .RequireAuthorization()
        .RequireRole("Admin", "MerchantOwner")
        .Produces<ProductCategoryResponse>(200)
        .Produces(403);

        group.MapPut("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateProductCategoryRequest request,
            ClaimsPrincipal user,
            [FromServices] IProductCategoryService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.UpdateProductCategoryAsync(id, request, userId, ct);
            return result.ToIResult();
        })
        .WithName("UpdateProductCategory")
        .RequireAuthorization()
        .RequireRole("Admin", "MerchantOwner")
        .Produces<ProductCategoryResponse>(200)
        .Produces(403)
        .Produces(404);

        group.MapDelete("/{id:guid}", async (
            [FromRoute] Guid id,
            ClaimsPrincipal user,
            [FromServices] IProductCategoryService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.DeleteProductCategoryAsync(id, userId, ct);
            return result.ToIResult();
        })
        .WithName("DeleteProductCategory")
        .RequireAuthorization()
        .RequireRole("Admin", "MerchantOwner")
        .Produces(200)
        .Produces(403)
        .Produces(404);
    }
}

