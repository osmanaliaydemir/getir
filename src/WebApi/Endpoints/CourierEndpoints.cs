using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Couriers;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Endpoints;

public static class CourierEndpoints
{
    public static void MapCourierEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/courier")
            .WithTags("Courier")
            .RequireAuthorization();

        group.MapGet("/orders", async (
            [AsParameters] PaginationQuery query,
            [FromServices] ICourierService courierService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var courierId = GetCourierId(httpContext);
            if (courierId == null) return Results.Unauthorized();

            var result = await courierService.GetAssignedOrdersAsync(courierId.Value, query, ct);
            return result.ToIResult();
        })
        .WithName("GetCourierOrders")
        .Produces<PagedResult<CourierOrderResponse>>(200);

        group.MapPost("/location/update", async (
            [FromBody] CourierLocationUpdateRequest request,
            [FromServices] ICourierService courierService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var courierId = GetCourierId(httpContext);
            if (courierId == null) return Results.Unauthorized();

            var result = await courierService.UpdateLocationAsync(courierId.Value, request, ct);
            return result.ToIResult();
        })
        .WithName("UpdateCourierLocation")
        .Produces(200);

        group.MapPost("/availability/set", async (
            [FromBody] SetAvailabilityRequest request,
            [FromServices] ICourierService courierService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var courierId = GetCourierId(httpContext);
            if (courierId == null) return Results.Unauthorized();

            var result = await courierService.SetAvailabilityAsync(courierId.Value, request, ct);
            return result.ToIResult();
        })
        .WithName("SetCourierAvailability")
        .Produces(200);
    }

    private static Guid? GetCourierId(HttpContext context)
    {
        // Courier ID claim'den veya user ID'den alınabilir
        var claim = context.User.FindFirst("CourierId") ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var courierId) ? courierId : null;
    }
}
