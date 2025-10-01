using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Categories;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/categories")
            .WithTags("Categories");

        group.MapGet("/", async (
            [AsParameters] PaginationQuery query,
            [FromServices] ICategoryService categoryService,
            CancellationToken ct) =>
        {
            var result = await categoryService.GetCategoriesAsync(query, ct);
            return result.ToIResult();
        })
        .WithName("GetCategories")
        .Produces<PagedResult<CategoryResponse>>(200);

        group.MapGet("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] ICategoryService categoryService,
            CancellationToken ct) =>
        {
            var result = await categoryService.GetCategoryByIdAsync(id, ct);
            return result.ToIResult();
        })
        .WithName("GetCategoryById")
        .Produces<CategoryResponse>(200)
        .Produces(404);

        group.MapPost("/", async (
            [FromBody] CreateCategoryRequest request,
            [FromServices] ICategoryService categoryService,
            CancellationToken ct) =>
        {
            var result = await categoryService.CreateCategoryAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("CreateCategory")
        .RequireAuthorization()
        .Produces<CategoryResponse>(200);

        group.MapPut("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateCategoryRequest request,
            [FromServices] ICategoryService categoryService,
            CancellationToken ct) =>
        {
            var result = await categoryService.UpdateCategoryAsync(id, request, ct);
            return result.ToIResult();
        })
        .WithName("UpdateCategory")
        .RequireAuthorization()
        .Produces<CategoryResponse>(200);

        group.MapDelete("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] ICategoryService categoryService,
            CancellationToken ct) =>
        {
            var result = await categoryService.DeleteCategoryAsync(id, ct);
            return result.ToIResult();
        })
        .WithName("DeleteCategory")
        .RequireAuthorization()
        .Produces(200);
    }
}
