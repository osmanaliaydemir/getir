using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.DeliveryZones;
using Getir.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Getir.WebApi.Endpoints;

public static class DeliveryZoneEndpoints
{
    public static void MapDeliveryZoneEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/delivery-zones")
            .WithTags("Delivery Zones");

        // Get delivery zones for a merchant
        group.MapGet("/merchant/{merchantId:guid}", async (
            [FromRoute] Guid merchantId,
            [FromServices] IDeliveryZoneService service,
            CancellationToken ct) =>
        {
            var result = await service.GetDeliveryZonesByMerchantAsync(merchantId, ct);
            return result.ToIResult();
        })
        .WithName("GetDeliveryZonesByMerchant")
        .Produces<List<DeliveryZoneResponse>>(200)
        .Produces(404);

        // Get specific delivery zone
        group.MapGet("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IDeliveryZoneService service,
            CancellationToken ct) =>
        {
            var result = await service.GetDeliveryZoneByIdAsync(id, ct);
            return result.ToIResult();
        })
        .WithName("GetDeliveryZoneById")
        .Produces<DeliveryZoneResponse>(200)
        .Produces(404);

        // Create delivery zone (Merchant Owner only)
        group.MapPost("/", async (
            [FromBody] CreateDeliveryZoneRequest request,
            ClaimsPrincipal user,
            [FromServices] IDeliveryZoneService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.CreateDeliveryZoneAsync(request, userId, ct);
            return result.ToIResult();
        })
        .WithName("CreateDeliveryZone")
        .RequireAuthorization()
        .RequireRole("Admin", "MerchantOwner")
        .Produces<DeliveryZoneResponse>(200)
        .Produces(400)
        .Produces(403);

        // Update delivery zone (Merchant Owner only)
        group.MapPut("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateDeliveryZoneRequest request,
            ClaimsPrincipal user,
            [FromServices] IDeliveryZoneService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.UpdateDeliveryZoneAsync(id, request, userId, ct);
            return result.ToIResult();
        })
        .WithName("UpdateDeliveryZone")
        .RequireAuthorization()
        .RequireRole("Admin", "MerchantOwner")
        .Produces<DeliveryZoneResponse>(200)
        .Produces(403)
        .Produces(404);

        // Delete delivery zone (Merchant Owner only)
        group.MapDelete("/{id:guid}", async (
            [FromRoute] Guid id,
            ClaimsPrincipal user,
            [FromServices] IDeliveryZoneService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.DeleteDeliveryZoneAsync(id, userId, ct);
            return result.ToIResult();
        })
        .WithName("DeleteDeliveryZone")
        .RequireAuthorization()
        .RequireRole("Admin", "MerchantOwner")
        .Produces(200)
        .Produces(403)
        .Produces(404);

        // Check if location is in delivery zone
        group.MapPost("/merchant/{merchantId:guid}/check", async (
            [FromRoute] Guid merchantId,
            [FromBody] CheckDeliveryZoneRequest request,
            [FromServices] IDeliveryZoneService service,
            CancellationToken ct) =>
        {
            var result = await service.CheckDeliveryZoneAsync(merchantId, request, ct);
            return result.ToIResult();
        })
        .WithName("CheckDeliveryZone")
        .Produces<CheckDeliveryZoneResponse>(200)
        .Produces(404);
    }
}
