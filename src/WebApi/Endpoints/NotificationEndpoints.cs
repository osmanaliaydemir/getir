using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Notifications;
using Microsoft.AspNetCore.Mvc;
using Getir.Application.Abstractions;
using Getir.WebApi.Extensions;
using System.Security.Claims;

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

        // === NOTIFICATION PREFERENCES ===
        
        group.MapGet("/preferences", GetNotificationPreferences)
            .WithName("GetNotificationPreferences")
            .WithSummary("Get user notification preferences")
            .Produces<NotificationPreferencesResponse>(200)
            .Produces<ProblemDetails>(401);

        group.MapPut("/preferences", UpdateNotificationPreferences)
            .WithName("UpdateNotificationPreferences")
            .WithSummary("Update user notification preferences")
            .Produces(204)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401);

        group.MapPost("/preferences/reset", ResetNotificationPreferences)
            .WithName("ResetNotificationPreferences")
            .WithSummary("Reset notification preferences to defaults")
            .Produces(204)
            .Produces<ProblemDetails>(401);

        group.MapGet("/preferences/summary", GetPreferencesSummary)
            .WithName("GetPreferencesSummary")
            .WithSummary("Get notification preferences summary")
            .Produces<NotificationPreferencesSummary>(200)
            .Produces<ProblemDetails>(401);

        // === SEND NOTIFICATIONS ===
        
        group.MapPost("/send/email", SendEmailNotification)
            .WithName("SendEmailNotification")
            .WithSummary("Send email notification")
            .Produces(204)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401);

        group.MapPost("/send/sms", SendSmsNotification)
            .WithName("SendSmsNotification")
            .WithSummary("Send SMS notification")
            .Produces(204)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401);

        group.MapPost("/send/push", SendPushNotification)
            .WithName("SendPushNotification")
            .WithSummary("Send push notification")
            .Produces(204)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401);
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }

    #region Notification Preferences Endpoints

    private static async Task<IResult> GetNotificationPreferences(
        ClaimsPrincipal user,
        INotificationPreferencesService preferencesService,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        var result = await preferencesService.GetUserPreferencesAsync(userId, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> UpdateNotificationPreferences(
        UpdateNotificationPreferencesRequest request,
        ClaimsPrincipal user,
        INotificationPreferencesService preferencesService,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        var result = await preferencesService.UpdateUserPreferencesAsync(userId, request, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> ResetNotificationPreferences(
        ClaimsPrincipal user,
        INotificationPreferencesService preferencesService,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        var result = await preferencesService.ResetToDefaultsAsync(userId, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> GetPreferencesSummary(
        ClaimsPrincipal user,
        INotificationPreferencesService preferencesService,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        var result = await preferencesService.GetPreferencesSummaryAsync(userId, cancellationToken);
        return result.ToApiResult();
    }

    #endregion

    #region Send Notification Endpoints

    private static async Task<IResult> SendEmailNotification(
        EmailRequest request,
        ClaimsPrincipal user,
        IEmailService emailService,
        INotificationPreferencesService preferencesService,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        
        // Check if user can receive email notifications
        var canReceive = await preferencesService.CanReceiveNotificationAsync(
            userId, NotificationType.SystemAnnouncement, NotificationChannel.Email, cancellationToken);
        
        if (!canReceive.Success || !canReceive.Value)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Email notifications disabled",
                Detail = "User has disabled email notifications",
                Status = 400
            });
        }

        var result = await emailService.SendEmailAsync(request, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> SendSmsNotification(
        SmsRequest request,
        ClaimsPrincipal user,
        ISmsService smsService,
        INotificationPreferencesService preferencesService,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        
        // Check if user can receive SMS notifications
        var canReceive = await preferencesService.CanReceiveNotificationAsync(
            userId, NotificationType.SystemAnnouncement, NotificationChannel.Sms, cancellationToken);
        
        if (!canReceive.Success || !canReceive.Value)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "SMS notifications disabled",
                Detail = "User has disabled SMS notifications",
                Status = 400
            });
        }

        var result = await smsService.SendSmsAsync(request, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> SendPushNotification(
        PushNotificationRequest request,
        ClaimsPrincipal user,
        IPushNotificationService pushService,
        INotificationPreferencesService preferencesService,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        
        // Check if user can receive push notifications
        var canReceive = await preferencesService.CanReceiveNotificationAsync(
            userId, NotificationType.SystemAnnouncement, NotificationChannel.Push, cancellationToken);
        
        if (!canReceive.Success || !canReceive.Value)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Push notifications disabled",
                Detail = "User has disabled push notifications",
                Status = 400
            });
        }

        var result = await pushService.SendPushNotificationAsync(request, cancellationToken);
        return result.ToApiResult();
    }

    #endregion
}
