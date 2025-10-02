using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Getir.Application.Services.Couriers;
using Getir.Application.DTO;
using Getir.Application.Common;
using Getir.WebApi.Extensions;
using System.Security.Claims;

namespace Getir.WebApi.Endpoints;

public static class CourierEndpoints
{
    public static void MapCourierEndpoints(this WebApplication app)
    {
        // Courier Panel
        var courierGroup = app.MapGroup("/api/v1/couriers")
            .WithTags("Courier Panel")
            .RequireAuthorization();

        // Get courier dashboard
        courierGroup.MapGet("/dashboard", async (
            ClaimsPrincipal user,
            [FromServices] ICourierService service,
            CancellationToken ct) =>
        {
            var courierId = user.GetUserId();
            var result = await service.GetCourierDashboardAsync(courierId, ct);
            return result.ToIResult();
        })
        .WithName("GetCourierDashboard")
        .Produces<CourierDashboardResponse>(200)
        .Produces(404)
        .RequireAuthorization();

        // Get courier stats
        courierGroup.MapGet("/stats", async (
            ClaimsPrincipal user,
            [FromServices] ICourierService service,
            CancellationToken ct) =>
        {
            var courierId = user.GetUserId();
            var result = await service.GetCourierStatsAsync(courierId, ct);
            return result.ToIResult();
        })
        .WithName("GetCourierStats")
        .Produces<CourierStatsResponse>(200)
        .Produces(404)
        .RequireAuthorization();

        // Get courier earnings
        courierGroup.MapGet("/earnings", async (
            ClaimsPrincipal user,
            [FromServices] ICourierService service,
            CancellationToken ct,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null) =>
        {
            var courierId = user.GetUserId();
            var result = await service.GetCourierEarningsAsync(courierId, startDate, endDate, ct);
            return result.ToIResult();
        })
        .WithName("GetCourierEarnings")
        .Produces<CourierEarningsResponse>(200)
        .Produces(404)
        .RequireAuthorization();

        // Get assigned orders
        courierGroup.MapGet("/orders", async (
            ClaimsPrincipal user,
            [FromServices] ICourierService service,
            CancellationToken ct,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var courierId = user.GetUserId();
            var query = new PaginationQuery { Page = page, PageSize = pageSize };
            var result = await service.GetAssignedOrdersAsync(courierId, query, ct);
            return result.ToIResult();
        })
        .WithName("GetCourierOrders")
        .Produces<PagedResult<CourierOrderResponse>>(200)
        .Produces(404)
        .RequireAuthorization();

        // Accept order
        courierGroup.MapPost("/orders/accept", async (
            [FromBody] AcceptOrderRequest request,
            ClaimsPrincipal user,
            [FromServices] ICourierService service,
            CancellationToken ct) =>
        {
            var courierId = user.GetUserId();
            var result = await service.AcceptOrderAsync(courierId, request, ct);
            return result.ToIResult();
        })
        .WithName("AcceptOrderByCourier")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization();

        // Start delivery
        courierGroup.MapPost("/orders/start-delivery", async (
            [FromBody] StartDeliveryRequest request,
            ClaimsPrincipal user,
            [FromServices] ICourierService service,
            CancellationToken ct) =>
        {
            var courierId = user.GetUserId();
            var result = await service.StartDeliveryAsync(courierId, request, ct);
            return result.ToIResult();
        })
        .WithName("StartDelivery")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization();

        // Complete delivery
        courierGroup.MapPost("/orders/complete-delivery", async (
            [FromBody] CompleteDeliveryRequest request,
            ClaimsPrincipal user,
            [FromServices] ICourierService service,
            CancellationToken ct) =>
        {
            var courierId = user.GetUserId();
            var result = await service.CompleteDeliveryAsync(courierId, request, ct);
            return result.ToIResult();
        })
        .WithName("CompleteDelivery")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization();

        // Update location
        courierGroup.MapPut("/location", async (
            [FromBody] UpdateCourierLocationRequest request,
            ClaimsPrincipal user,
            [FromServices] ICourierService service,
            CancellationToken ct) =>
        {
            var courierId = user.GetUserId();
            var locationRequest = new CourierLocationUpdateRequest(
                request.Latitude,
                request.Longitude);
            var result = await service.UpdateLocationAsync(courierId, locationRequest, ct);
            return result.ToIResult();
        })
        .WithName("UpdateCourierLocation")
        .Produces(200)
        .Produces(400)
        .RequireAuthorization();

        // Set availability
        courierGroup.MapPut("/availability", async (
            [FromBody] SetAvailabilityRequest request,
            ClaimsPrincipal user,
            [FromServices] ICourierService service,
            CancellationToken ct) =>
        {
            var courierId = user.GetUserId();
            var result = await service.SetAvailabilityAsync(courierId, request, ct);
            return result.ToIResult();
        })
        .WithName("SetCourierAvailability")
        .Produces(200)
        .Produces(400)
        .RequireAuthorization();

        // Admin/System endpoints
        var adminGroup = app.MapGroup("/api/v1/admin/couriers")
            .WithTags("Admin - Courier Management")
            .RequireAuthorization("Admin");

        // Assign order to courier
        adminGroup.MapPost("/assign-order", async (
            [FromBody] AssignOrderRequest request,
            [FromServices] ICourierService service,
            CancellationToken ct) =>
        {
            var result = await service.AssignOrderAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("AssignOrder")
        .Produces<CourierAssignmentResponse>(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization("Admin");

        // Find nearest couriers
        adminGroup.MapPost("/find-nearest", async (
            [FromBody] FindNearestCouriersRequest request,
            [FromServices] ICourierService service,
            CancellationToken ct) =>
        {
            var result = await service.FindNearestCouriersAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("FindNearestCouriers")
        .Produces<FindNearestCouriersResponse>(200)
        .Produces(400)
        .RequireAuthorization("Admin");

        // Get top performers
        adminGroup.MapGet("/top-performers", async (
            [FromServices] ICourierService service,
            CancellationToken ct,
            [FromQuery] int count = 10) =>
        {
            var result = await service.GetTopPerformersAsync(count, ct);
            return result.ToIResult();
        })
        .WithName("GetTopPerformers")
        .Produces<List<CourierPerformanceResponse>>(200)
        .RequireAuthorization("Admin");

        // Get earnings detail for specific courier
        adminGroup.MapGet("/earnings-detail", async (
            [FromServices] ICourierService service,
            CancellationToken ct,
            [FromQuery] Guid courierId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null) =>
        {
            var query = new CourierEarningsQuery(courierId, startDate, endDate);
            var result = await service.GetEarningsDetailAsync(query, ct);
            return result.ToIResult();
        })
        .WithName("GetCourierEarningsDetail")
        .Produces<CourierEarningsDetailResponse>(200)
        .Produces(404)
        .RequireAuthorization("Admin");
    }
}