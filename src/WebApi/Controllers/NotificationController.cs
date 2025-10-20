using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Notifications;
using Getir.Application.Abstractions;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationChannel = Getir.Application.DTO.NotificationChannel;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Bildirimleri yönetmek için bildirim controller'ı
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

    public NotificationController(INotificationService notificationService, INotificationPreferencesService preferencesService,
        IEmailService emailService, ISmsService smsService, IPushNotificationService pushService)
    {
        _notificationService = notificationService;
        _preferencesService = preferencesService;
        _emailService = emailService;
        _smsService = smsService;
        _pushService = pushService;
    }

    /// <summary>
    /// Kullanıcı bildirimlerini getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış bildirimler</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications([FromQuery] PaginationQuery query, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _notificationService.GetUserNotificationsAsync(userId, query, ct);
        return ToActionResult<PagedResult<NotificationResponse>>(result);
    }

    /// <summary>
    /// Bildirimleri okundu olarak işaretle
    /// </summary>
    /// <param name="request">Okundu işaretleme isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("mark-as-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request, CancellationToken ct = default)
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
    /// Kullanıcı bildirim tercihlerini getir
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Bildirim tercihleri</returns>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(NotificationPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotificationPreferences(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _preferencesService.GetUserPreferencesAsync(userId, ct);
        return ToActionResult<NotificationPreferencesResponse>(result);
    }

    /// <summary>
    /// Kullanıcı bildirim tercihlerini güncelle
    /// </summary>
    /// <param name="request">Tercih güncelleme isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPut("preferences")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateNotificationPreferences([FromBody] UpdateNotificationPreferencesRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _preferencesService.UpdateUserPreferencesAsync(userId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Bildirim tercihlerini varsayılanlara sıfırla
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("preferences/reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResetNotificationPreferences(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _preferencesService.ResetToDefaultsAsync(userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Bildirim tercihleri özetini getir
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Tercih özeti</returns>
    [HttpGet("preferences/summary")]
    [ProducesResponseType(typeof(NotificationPreferencesSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPreferencesSummary(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _preferencesService.GetPreferencesSummaryAsync(userId, ct);
        return ToActionResult<NotificationPreferencesSummary>(result);
    }

    #endregion

    #region Send Notifications

    /// <summary>
    /// E-posta bildirimi gönder
    /// </summary>
    /// <param name="request">E-posta isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("send/email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendEmailNotification([FromBody] EmailRequest request, CancellationToken ct = default)
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
        return ToActionResult(result);
    }

    /// <summary>
    /// SMS bildirimi gönder
    /// </summary>
    /// <param name="request">SMS isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("send/sms")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendSmsNotification([FromBody] SmsRequest request, CancellationToken ct = default)
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
        return ToActionResult(result);
    }

    /// <summary>
    /// Push bildirimi gönder
    /// </summary>
    /// <param name="request">Push bildirim isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("send/push")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendPushNotification([FromBody] PushNotificationRequest request, CancellationToken ct = default)
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
        return ToActionResult(result);
    }

    #endregion
}
