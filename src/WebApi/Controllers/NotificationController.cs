using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Notifications;
using Getir.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationType = Getir.Application.DTO.NotificationType;
using NotificationChannel = Getir.Application.DTO.NotificationChannel;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Notification controller for managing notifications
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Notifications")]
[Authorize]
public class NotificationController : BaseController
{
    private readonly INotificationService _notificationService;
    private readonly INotificationPreferencesService _preferencesService;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IPushNotificationService _pushService;

    public NotificationController(
        INotificationService notificationService,
        INotificationPreferencesService preferencesService,
        IEmailService emailService,
        ISmsService smsService,
        IPushNotificationService pushService)
    {
        _notificationService = notificationService;
        _preferencesService = preferencesService;
        _emailService = emailService;
        _smsService = smsService;
        _pushService = pushService;
    }

    /// <summary>
    /// Get user notifications
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged notifications</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _notificationService.GetUserNotificationsAsync(userId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mark notifications as read
    /// </summary>
    /// <param name="request">Mark as read request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("mark-as-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAsRead(
        [FromBody] MarkAsReadRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _notificationService.MarkAsReadAsync(userId, request, ct);
        return ToActionResult(result);
    }

    #region Notification Preferences

    /// <summary>
    /// Get user notification preferences
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Notification preferences</returns>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(NotificationPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotificationPreferences(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _preferencesService.GetUserPreferencesAsync(userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update user notification preferences
    /// </summary>
    /// <param name="request">Update preferences request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPut("preferences")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateNotificationPreferences(
        [FromBody] UpdateNotificationPreferencesRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _preferencesService.UpdateUserPreferencesAsync(userId, request, ct);
        if (result.Success)
        {
            return NoContent();
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Reset notification preferences to defaults
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("preferences/reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResetNotificationPreferences(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _preferencesService.ResetToDefaultsAsync(userId, ct);
        if (result.Success)
        {
            return NoContent();
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Get notification preferences summary
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Preferences summary</returns>
    [HttpGet("preferences/summary")]
    [ProducesResponseType(typeof(NotificationPreferencesSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPreferencesSummary(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _preferencesService.GetPreferencesSummaryAsync(userId, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Send Notifications

    /// <summary>
    /// Send email notification
    /// </summary>
    /// <param name="request">Email request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("send/email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendEmailNotification(
        [FromBody] EmailRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // Check if user can receive email notifications
        var canReceive = await _preferencesService.CanReceiveNotificationAsync(
            userId, NotificationType.SystemAnnouncement, NotificationChannel.Email, ct);
        
        if (!canReceive.Success || !canReceive.Value)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Email notifications disabled",
                Detail = "User has disabled email notifications",
                Status = 400
            });
        }

        var result = await _emailService.SendEmailAsync(request, ct);
        if (result.Success)
        {
            return NoContent();
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Send SMS notification
    /// </summary>
    /// <param name="request">SMS request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("send/sms")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendSmsNotification(
        [FromBody] SmsRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // Check if user can receive SMS notifications
        var canReceive = await _preferencesService.CanReceiveNotificationAsync(
            userId, NotificationType.SystemAnnouncement, NotificationChannel.Sms, ct);
        
        if (!canReceive.Success || !canReceive.Value)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "SMS notifications disabled",
                Detail = "User has disabled SMS notifications",
                Status = 400
            });
        }

        var result = await _smsService.SendSmsAsync(request, ct);
        if (result.Success)
        {
            return NoContent();
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Send push notification
    /// </summary>
    /// <param name="request">Push notification request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("send/push")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendPushNotification(
        [FromBody] PushNotificationRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // Check if user can receive push notifications
        var canReceive = await _preferencesService.CanReceiveNotificationAsync(
            userId, NotificationType.SystemAnnouncement, NotificationChannel.Push, ct);
        
        if (!canReceive.Success || !canReceive.Value)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Push notifications disabled",
                Detail = "User has disabled push notifications",
                Status = 400
            });
        }

        var result = await _pushService.SendPushNotificationAsync(request, ct);
        if (result.Success)
        {
            return NoContent();
        }
        return ToActionResult(result);
    }

    #endregion
}
