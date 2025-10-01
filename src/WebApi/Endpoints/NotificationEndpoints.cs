using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/", async (
            [AsParameters] PaginationQuery query,
            [FromServices] INotificationService notificationService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var result = await notificationService.GetUserNotificationsAsync(userId.Value, query, ct);
            return result.ToIResult();
        })
        .WithName("GetNotifications")
        .Produces<PagedResult<NotificationResponse>>(200);

        group.MapPost("/mark-as-read", async (
            [FromBody] MarkAsReadRequest request,
            [FromServices] INotificationService notificationService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userId = GetUserId(httpContext);
            if (userId == null) return Results.Unauthorized();

            var result = await notificationService.MarkAsReadAsync(userId.Value, request, ct);
            return result.ToIResult();
        })
        .WithName("MarkAsRead")
        .Produces(200);
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }
}
