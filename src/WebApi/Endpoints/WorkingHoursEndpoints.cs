using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.WorkingHours;
using Getir.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Getir.WebApi.Endpoints;

public static class WorkingHoursEndpoints
{
    public static void MapWorkingHoursEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/working-hours")
            .WithTags("Working Hours");

        // Get working hours for a merchant
        group.MapGet("/merchant/{merchantId:guid}", async (
            [FromRoute] Guid merchantId,
            [FromServices] IWorkingHoursService service,
            CancellationToken ct) =>
        {
            var result = await service.GetWorkingHoursByMerchantAsync(merchantId, ct);
            return result.ToIResult();
        })
        .WithName("GetWorkingHoursByMerchant")
        .Produces<List<WorkingHoursResponse>>(200)
        .Produces(404);

        // Get specific working hours
        group.MapGet("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IWorkingHoursService service,
            CancellationToken ct) =>
        {
            var result = await service.GetWorkingHoursByIdAsync(id, ct);
            return result.ToIResult();
        })
        .WithName("GetWorkingHoursById")
        .Produces<WorkingHoursResponse>(200)
        .Produces(404);

        // Create working hours (Merchant Owner only)
        group.MapPost("/", async (
            [FromBody] CreateWorkingHoursRequest request,
            ClaimsPrincipal user,
            [FromServices] IWorkingHoursService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.CreateWorkingHoursAsync(request, userId, ct);
            return result.ToIResult();
        })
        .WithName("CreateWorkingHours")
        .RequireAuthorization()
        .RequireRole("Admin", "MerchantOwner")
        .Produces<WorkingHoursResponse>(200)
        .Produces(400)
        .Produces(403);

        // Update working hours (Merchant Owner only)
        group.MapPut("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateWorkingHoursRequest request,
            ClaimsPrincipal user,
            [FromServices] IWorkingHoursService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.UpdateWorkingHoursAsync(id, request, userId, ct);
            return result.ToIResult();
        })
        .WithName("UpdateWorkingHours")
        .RequireAuthorization()
        .RequireRole("Admin", "MerchantOwner")
        .Produces<WorkingHoursResponse>(200)
        .Produces(403)
        .Produces(404);

        // Delete working hours (Merchant Owner only)
        group.MapDelete("/{id:guid}", async (
            [FromRoute] Guid id,
            ClaimsPrincipal user,
            [FromServices] IWorkingHoursService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.DeleteWorkingHoursAsync(id, userId, ct);
            return result.ToIResult();
        })
        .WithName("DeleteWorkingHours")
        .RequireAuthorization()
        .RequireRole("Admin", "MerchantOwner")
        .Produces(200)
        .Produces(403)
        .Produces(404);

        // Bulk update working hours for a merchant (Merchant Owner only)
        group.MapPut("/merchant/{merchantId:guid}/bulk", async (
            [FromRoute] Guid merchantId,
            [FromBody] BulkUpdateWorkingHoursRequest request,
            ClaimsPrincipal user,
            [FromServices] IWorkingHoursService service,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.BulkUpdateWorkingHoursAsync(merchantId, request, userId, ct);
            return result.ToIResult();
        })
        .WithName("BulkUpdateWorkingHours")
        .RequireAuthorization()
        .RequireRole("Admin", "MerchantOwner")
        .Produces(200)
        .Produces(400)
        .Produces(403);

        // Check if merchant is open
        group.MapGet("/merchant/{merchantId:guid}/is-open", async (
            [FromRoute] Guid merchantId,
            [FromQuery] DateTime? checkTime,
            [FromServices] IWorkingHoursService service,
            CancellationToken ct) =>
        {
            var result = await service.IsMerchantOpenAsync(merchantId, checkTime, ct);
            return result.ToIResult();
        })
        .WithName("IsMerchantOpen")
        .Produces<bool>(200)
        .Produces(404);
    }
}
