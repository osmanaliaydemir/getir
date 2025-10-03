using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.ServiceCategories;
using Getir.WebApi.Extensions;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Endpoints;

public static class ServiceCategoryEndpoints
{
    public static void MapServiceCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/service-categories")
            .WithTags("Service Categories");

        group.MapGet("/", async (
            [AsParameters] PaginationQuery query,
            [FromServices] IServiceCategoryService service,
            CancellationToken ct) =>
        {
            var result = await service.GetServiceCategoriesAsync(query, ct);
            return result.ToIResult();
        })
        .WithName("GetServiceCategories")
        .Produces<PagedResult<ServiceCategoryResponse>>(200);

        group.MapGet("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IServiceCategoryService service,
            CancellationToken ct) =>
        {
            var result = await service.GetServiceCategoryByIdAsync(id, ct);
            return result.ToIResult();
        })
        .WithName("GetServiceCategoryById")
        .Produces<ServiceCategoryResponse>(200)
        .Produces(404);

        group.MapPost("/", async (
            [FromBody] CreateServiceCategoryRequest request,
            [FromServices] IServiceCategoryService service,
            CancellationToken ct) =>
        {
            var result = await service.CreateServiceCategoryAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("CreateServiceCategory")
        .RequireAuthorization()
        .RequireAdmin()
        .Produces<ServiceCategoryResponse>(200)
        .Produces(403);

        group.MapPut("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateServiceCategoryRequest request,
            [FromServices] IServiceCategoryService service,
            CancellationToken ct) =>
        {
            var result = await service.UpdateServiceCategoryAsync(id, request, ct);
            return result.ToIResult();
        })
        .WithName("UpdateServiceCategory")
        .RequireAuthorization()
        .RequireAdmin()
        .Produces<ServiceCategoryResponse>(200)
        .Produces(403)
        .Produces(404);

        group.MapDelete("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IServiceCategoryService service,
            CancellationToken ct) =>
        {
            var result = await service.DeleteServiceCategoryAsync(id, ct);
            return result.ToIResult();
        })
        .WithName("DeleteServiceCategory")
        .RequireAuthorization()
        .RequireAdmin()
        .Produces(200)
        .Produces(403)
        .Produces(404);

        // Kategori tipine göre filtreleme
        group.MapGet("/by-type/{type}", async (
            [FromRoute] ServiceCategoryType type,
            [AsParameters] PaginationQuery query,
            [FromServices] IServiceCategoryService service,
            CancellationToken ct) =>
        {
            var result = await service.GetServiceCategoriesByTypeAsync(type, query, ct);
            return result.ToIResult();
        })
        .WithName("GetServiceCategoriesByType")
        .Produces<PagedResult<ServiceCategoryResponse>>(200);

        // Aktif kategorileri tipine göre getir (pagination olmadan)
        group.MapGet("/active/by-type/{type}", async (
            [FromRoute] ServiceCategoryType type,
            [FromServices] IServiceCategoryService service,
            CancellationToken ct) =>
        {
            var result = await service.GetActiveServiceCategoriesByTypeAsync(type, ct);
            return result.ToIResult();
        })
        .WithName("GetActiveServiceCategoriesByType")
        .Produces<IEnumerable<ServiceCategoryResponse>>(200);
    }
}

