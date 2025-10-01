using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Getir.Application.Services.ProductOptions;
using Getir.Application.DTO;
using Getir.Application.Common;
using Getir.WebApi.Extensions;
using System.Security.Claims;

namespace Getir.WebApi.Endpoints;

public static class ProductOptionEndpoints
{
    public static void MapProductOptionEndpoints(this WebApplication app)
    {
        // Product Option Groups
        var optionGroupGroup = app.MapGroup("/api/merchants/products/{productId:guid}/option-groups")
            .WithTags("Product Option Groups")
            .RequireAuthorization();

        // Get product option groups
        optionGroupGroup.MapGet("/", async (
            [FromRoute] Guid productId,
            [FromServices] IProductOptionGroupService service,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = new PaginationQuery { Page = page, PageSize = pageSize };
            var result = await service.GetProductOptionGroupsAsync(productId, query, ct);
            return result.ToIResult();
        })
        .WithName("GetProductOptionGroups")
        .Produces<PagedResult<ProductOptionGroupResponse>>(200)
        .Produces(404)
        .RequireAuthorization();

        // Get product option groups with options (for product display)
        optionGroupGroup.MapGet("/with-options", async (
            [FromRoute] Guid productId,
            [FromServices] IProductOptionGroupService service,
            CancellationToken ct) =>
        {
            var result = await service.GetProductOptionGroupsWithOptionsAsync(productId, ct);
            return result.ToIResult();
        })
        .WithName("GetProductOptionGroupsWithOptions")
        .Produces<List<ProductOptionGroupResponse>>(200)
        .Produces(404)
        .RequireAuthorization();

        // Create product option group
        optionGroupGroup.MapPost("/", async (
            [FromRoute] Guid productId,
            [FromBody] CreateProductOptionGroupRequest request,
            ClaimsPrincipal user,
            [FromServices] IProductOptionGroupService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.CreateProductOptionGroupAsync(request, userId, ct);
            return result.ToIResult();
        })
        .WithName("CreateProductOptionGroup")
        .Produces<ProductOptionGroupResponse>(201)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Update product option group
        optionGroupGroup.MapPut("/{id:guid}", async (
            [FromRoute] Guid productId,
            [FromRoute] Guid id,
            [FromBody] UpdateProductOptionGroupRequest request,
            ClaimsPrincipal user,
            [FromServices] IProductOptionGroupService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.UpdateProductOptionGroupAsync(id, request, userId, ct);
            return result.ToIResult();
        })
        .WithName("UpdateProductOptionGroup")
        .Produces<ProductOptionGroupResponse>(200)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Delete product option group
        optionGroupGroup.MapDelete("/{id:guid}", async (
            [FromRoute] Guid productId,
            [FromRoute] Guid id,
            ClaimsPrincipal user,
            [FromServices] IProductOptionGroupService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.DeleteProductOptionGroupAsync(id, userId, ct);
            return result.ToIResult();
        })
        .WithName("DeleteProductOptionGroup")
        .Produces(200)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Product Options
        var optionGroup = app.MapGroup("/api/merchants/products/option-groups/{optionGroupId:guid}/options")
            .WithTags("Product Options")
            .RequireAuthorization();

        // Get product options
        optionGroup.MapGet("/", async (
            [FromRoute] Guid optionGroupId,
            [FromServices] IProductOptionService service,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = new PaginationQuery { Page = page, PageSize = pageSize };
            var result = await service.GetProductOptionsAsync(optionGroupId, query, ct);
            return result.ToIResult();
        })
        .WithName("GetProductOptions")
        .Produces<PagedResult<ProductOptionResponse>>(200)
        .Produces(404)
        .RequireAuthorization();

        // Create product option
        optionGroup.MapPost("/", async (
            [FromRoute] Guid optionGroupId,
            [FromBody] CreateProductOptionRequest request,
            ClaimsPrincipal user,
            [FromServices] IProductOptionService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.CreateProductOptionAsync(request, userId, ct);
            return result.ToIResult();
        })
        .WithName("CreateProductOption")
        .Produces<ProductOptionResponse>(201)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Update product option
        optionGroup.MapPut("/{id:guid}", async (
            [FromRoute] Guid optionGroupId,
            [FromRoute] Guid id,
            [FromBody] UpdateProductOptionRequest request,
            ClaimsPrincipal user,
            [FromServices] IProductOptionService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.UpdateProductOptionAsync(id, request, userId, ct);
            return result.ToIResult();
        })
        .WithName("UpdateProductOption")
        .Produces<ProductOptionResponse>(200)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Delete product option
        optionGroup.MapDelete("/{id:guid}", async (
            [FromRoute] Guid optionGroupId,
            [FromRoute] Guid id,
            ClaimsPrincipal user,
            [FromServices] IProductOptionService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.DeleteProductOptionAsync(id, userId, ct);
            return result.ToIResult();
        })
        .WithName("DeleteProductOption")
        .Produces(200)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Bulk create product options
        optionGroup.MapPost("/bulk", async (
            [FromRoute] Guid optionGroupId,
            [FromBody] BulkCreateProductOptionsRequest request,
            ClaimsPrincipal user,
            [FromServices] IProductOptionService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.BulkCreateProductOptionsAsync(request, userId, ct);
            return result.ToIResult();
        })
        .WithName("BulkCreateProductOptions")
        .Produces(201)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        // Bulk update product options
        optionGroup.MapPut("/bulk", async (
            [FromRoute] Guid optionGroupId,
            [FromBody] BulkUpdateProductOptionsRequest request,
            ClaimsPrincipal user,
            [FromServices] IProductOptionService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.BulkUpdateProductOptionsAsync(request, userId, ct);
            return result.ToIResult();
        })
        .WithName("BulkUpdateProductOptions")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();
    }
}
