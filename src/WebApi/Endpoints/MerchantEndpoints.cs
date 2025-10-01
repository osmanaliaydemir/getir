using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Merchants;
using Getir.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Getir.WebApi.Endpoints;

public static class MerchantEndpoints
{
    public static void MapMerchantEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/merchants")
            .WithTags("Merchants");

        group.MapGet("/", async (
            [AsParameters] PaginationQuery query,
            [FromServices] IMerchantService merchantService,
            CancellationToken ct) =>
        {
            var result = await merchantService.GetMerchantsAsync(query, ct);
            return result.ToIResult();
        })
        .WithName("GetMerchants")
        .Produces<PagedResult<MerchantResponse>>(200);

        group.MapGet("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IMerchantService merchantService,
            CancellationToken ct) =>
        {
            var result = await merchantService.GetMerchantByIdAsync(id, ct);
            return result.ToIResult();
        })
        .WithName("GetMerchantById")
        .Produces<MerchantResponse>(200)
        .Produces(404);

        group.MapPost("/", async (
            [FromBody] CreateMerchantRequest request,
            ClaimsPrincipal user,
            [FromServices] IMerchantService merchantService,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await merchantService.CreateMerchantAsync(request, userId, ct);
            return result.ToIResult();
        })
        .WithName("CreateMerchant")
        .RequireAuthorization()
        .RequireRole("Admin", "MerchantOwner") // Sadece Admin ve MerchantOwner oluşturabilir
        .Produces<MerchantResponse>(200)
        .Produces(400)
        .Produces(403);

        group.MapPut("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateMerchantRequest request,
            ClaimsPrincipal user,
            [FromServices] IMerchantService merchantService,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await merchantService.UpdateMerchantAsync(id, request, userId, ct);
            return result.ToIResult();
        })
        .WithName("UpdateMerchant")
        .RequireAuthorization()
        .RequireRole("Admin", "MerchantOwner") // Sadece Admin ve MerchantOwner güncelleyebilir
        .Produces<MerchantResponse>(200)
        .Produces(403)
        .Produces(404);

        group.MapDelete("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IMerchantService merchantService,
            CancellationToken ct) =>
        {
            var result = await merchantService.DeleteMerchantAsync(id, ct);
            return result.ToIResult();
        })
        .WithName("DeleteMerchant")
        .RequireAuthorization()
        .RequireAdmin() // Sadece Admin silebilir
        .Produces(200)
        .Produces(403)
        .Produces(404);
    }
}
