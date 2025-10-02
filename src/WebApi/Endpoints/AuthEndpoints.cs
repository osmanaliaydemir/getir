using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication");

        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            [FromServices] IAuthService authService,
            CancellationToken ct) =>
        {
            var result = await authService.RegisterAsync(request, ct);
            return result.ToIResult();
        })
        .Produces<AuthResponse>(200)
        .Produces(400);

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            [FromServices] IAuthService authService,
            CancellationToken ct) =>
        {
            var result = await authService.LoginAsync(request, ct);
            return result.ToIResult();
        })
        .Produces<AuthResponse>(200)
        .Produces(401);

        group.MapPost("/refresh", async (
            [FromBody] RefreshTokenRequest request,
            [FromServices] IAuthService authService,
            CancellationToken ct) =>
        {
            var result = await authService.RefreshAsync(request, ct);
            return result.ToIResult();
        })
        .Produces<AuthResponse>(200)
        .Produces(401);

        group.MapPost("/logout", async (
            [FromServices] IAuthService authService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await authService.LogoutAsync(userId, ct);
            return result.ToIResult();
        })
        .RequireAuthorization()
        .Produces(200)
        .Produces(401);
    }
}
