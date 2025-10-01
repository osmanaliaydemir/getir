using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Addresses;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/users")
            .WithTags("Users")
            .RequireAuthorization();

        // Addresses
        group.MapGet("/addresses", async (
            [FromServices] IUserAddressService addressService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var result = await addressService.GetUserAddressesAsync(userId.Value, ct);
            return result.ToIResult();
        })
        .WithName("GetUserAddresses")
        .Produces<List<AddressResponse>>(200);

        group.MapPost("/addresses", async (
            [FromBody] CreateAddressRequest request,
            [FromServices] IUserAddressService addressService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var result = await addressService.AddAddressAsync(userId.Value, request, ct);
            return result.ToIResult();
        })
        .WithName("AddAddress")
        .Produces<AddressResponse>(200);

        group.MapPut("/addresses/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateAddressRequest request,
            [FromServices] IUserAddressService addressService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var result = await addressService.UpdateAddressAsync(userId.Value, id, request, ct);
            return result.ToIResult();
        })
        .WithName("UpdateAddress")
        .Produces<AddressResponse>(200);

        group.MapDelete("/addresses/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IUserAddressService addressService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var result = await addressService.DeleteAddressAsync(userId.Value, id, ct);
            return result.ToIResult();
        })
        .WithName("DeleteAddress")
        .Produces(200);

        group.MapPut("/addresses/{id:guid}/set-default", async (
            [FromRoute] Guid id,
            [FromServices] IUserAddressService addressService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var result = await addressService.SetDefaultAddressAsync(userId.Value, id, ct);
            return result.ToIResult();
        })
        .WithName("SetDefaultAddress")
        .Produces(200);
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }
}
