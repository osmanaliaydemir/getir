using Getir.Application.DTO;
using Getir.Application.Services.RealtimeTracking;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
/// <summary>
/// Gerçek zamanlı sipariş takibi, ETA hesaplama, bildirimler ve ayarlar için API
/// </summary>
public class RealtimeTrackingController : BaseController
{
    private readonly IOrderTrackingService _orderTrackingService;
    private readonly IETAEstimationService _etaEstimationService;
    private readonly ITrackingNotificationService _notificationService;
    private readonly ITrackingSettingsService _settingsService;
    private readonly IRealtimeTrackingService _realtimeTrackingService;

    public RealtimeTrackingController(
        IOrderTrackingService orderTrackingService,
        IETAEstimationService etaEstimationService,
        ITrackingNotificationService notificationService,
        ITrackingSettingsService settingsService,
        IRealtimeTrackingService realtimeTrackingService)
    {
        _orderTrackingService = orderTrackingService;
        _etaEstimationService = etaEstimationService;
        _notificationService = notificationService;
        _settingsService = settingsService;
        _realtimeTrackingService = realtimeTrackingService;
    }

    #region Order Tracking

    /// <summary>
    /// Sipariş ID'ye göre takip bilgisini getirir
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <returns>Takip bilgisi</returns>
    [HttpGet("order/{orderId}")]
    [ProducesResponseType(typeof(OrderTrackingDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTrackingByOrderId(Guid orderId)
    {
        try
        {
            var tracking = await _orderTrackingService.GetTrackingByOrderIdAsync(orderId);
            if (tracking == null)
            {
                return NotFound(new { message = "Tracking not found for this order" });
            }

            return Ok(tracking);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting tracking data", error = ex.Message });
        }
    }

    /// <summary>
    /// Takip ID'ye göre takip bilgisini getirir
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <returns>Takip bilgisi</returns>
    [HttpGet("{trackingId}")]
    [ProducesResponseType(typeof(OrderTrackingDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTrackingById(Guid trackingId)
    {
        try
        {
            var tracking = await _orderTrackingService.GetTrackingByIdAsync(trackingId);
            if (tracking == null)
            {
                return NotFound(new { message = "Tracking not found" });
            }

            return Ok(tracking);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting tracking data", error = ex.Message });
        }
    }

    /// <summary>
    /// Yeni takip kaydı oluşturur
    /// </summary>
    /// <param name="request">Takip oluşturma isteği</param>
    /// <returns>Oluşturulan takip</returns>
    [HttpPost("create")]
    [ProducesResponseType(typeof(OrderTrackingDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateTracking([FromBody] CreateTrackingRequest request)
    {
        try
        {
            var tracking = await _orderTrackingService.CreateTrackingAsync(request.OrderId, request.CourierId);
            return CreatedAtAction(nameof(GetTrackingById), new { trackingId = tracking.Id }, tracking);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating tracking", error = ex.Message });
        }
    }

    /// <summary>
    /// Konum güncellemesi gönderir
    /// </summary>
    /// <param name="request">Konum güncelleme isteği</param>
    /// <returns>Konum güncelleme yanıtı</returns>
    [HttpPost("location/update")]
    [ProducesResponseType(typeof(LocationUpdateResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateRequest request)
    {
        try
        {
            var response = await _orderTrackingService.UpdateLocationAsync(request);
            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating location", error = ex.Message });
        }
    }

    /// <summary>
    /// Sipariş durumunu günceller
    /// </summary>
    /// <param name="request">Durum güncelleme isteği</param>
    /// <returns>Durum güncelleme yanıtı</returns>
    [HttpPost("status/update")]
    [ProducesResponseType(typeof(StatusUpdateResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateStatus([FromBody] StatusUpdateRequest request)
    {
        try
        {
            var response = await _orderTrackingService.UpdateStatusAsync(request);
            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating status", error = ex.Message });
        }
    }

    /// <summary>
    /// Takibi siler
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <returns>İçerik yok</returns>
    [HttpDelete("{trackingId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteTracking(Guid trackingId)
    {
        try
        {
            var success = await _orderTrackingService.DeleteTrackingAsync(trackingId);
            if (!success)
            {
                return NotFound(new { message = "Tracking not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting tracking", error = ex.Message });
        }
    }

    /// <summary>
    /// Aktif takip kayıtlarını getirir
    /// </summary>
    /// <returns>Takip listesi</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<OrderTrackingDto>), 200)]
    public async Task<IActionResult> GetActiveTrackings()
    {
        try
        {
            var trackings = await _orderTrackingService.GetActiveTrackingsAsync();
            return Ok(trackings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting active trackings", error = ex.Message });
        }
    }

    /// <summary>
    /// Kurye ID'ye göre takip kayıtlarını getirir
    /// </summary>
    /// <param name="courierId">Kurye ID</param>
    /// <returns>Takip listesi</returns>
    [HttpGet("courier/{courierId}")]
    [ProducesResponseType(typeof(List<OrderTrackingDto>), 200)]
    public async Task<IActionResult> GetTrackingsByCourier(Guid courierId)
    {
        try
        {
            var trackings = await _orderTrackingService.GetTrackingsByCourierAsync(courierId);
            return Ok(trackings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting courier trackings", error = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcı ID'ye göre takip kayıtlarını getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <returns>Takip listesi</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<OrderTrackingDto>), 200)]
    public async Task<IActionResult> GetTrackingsByUser(Guid userId)
    {
        try
        {
            var trackings = await _orderTrackingService.GetTrackingsByUserAsync(userId);
            return Ok(trackings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting user trackings", error = ex.Message });
        }
    }

    /// <summary>
    /// Takip kayıtlarında arama yapar
    /// </summary>
    /// <param name="request">Arama isteği</param>
    /// <returns>Arama sonucu</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(TrackingSearchResponse), 200)]
    public async Task<IActionResult> SearchTrackings([FromBody] TrackingSearchRequest request)
    {
        try
        {
            var response = await _orderTrackingService.SearchTrackingsAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while searching trackings", error = ex.Message });
        }
    }

    /// <summary>
    /// Takip konum geçmişini getirir
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <param name="count">Kayıt adedi</param>
    /// <returns>Konum geçmişi</returns>
    [HttpGet("{trackingId}/history")]
    [ProducesResponseType(typeof(List<LocationHistoryDto>), 200)]
    public async Task<IActionResult> GetLocationHistory(Guid trackingId, [FromQuery] int count = 50)
    {
        try
        {
            var history = await _orderTrackingService.GetLocationHistoryAsync(trackingId, count);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting location history", error = ex.Message });
        }
    }

    /// <summary>
    /// Takip istatistiklerini getirir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <returns>İstatistikler</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(TrackingStatisticsDto), 200)]
    public async Task<IActionResult> GetTrackingStatistics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var statistics = await _orderTrackingService.GetTrackingStatisticsAsync(startDate, endDate);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting tracking statistics", error = ex.Message });
        }
    }

    /// <summary>
    /// Mevcut durum geçişlerini getirir
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <returns>Durum geçişleri</returns>
    [HttpGet("{trackingId}/transitions")]
    [ProducesResponseType(typeof(List<TrackingStatus>), 200)]
    public async Task<IActionResult> GetAvailableTransitions(Guid trackingId)
    {
        try
        {
            var transitions = await _orderTrackingService.GetAvailableTransitionsAsync(trackingId);
            return Ok(transitions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting available transitions", error = ex.Message });
        }
    }

    /// <summary>
    /// Bir durum geçişini doğrular
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <param name="request">Geçiş doğrulama isteği</param>
    /// <returns>Doğrulama sonucu</returns>
    [HttpPost("{trackingId}/validate-transition")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> ValidateTransition(Guid trackingId, [FromBody] ValidateTransitionRequest request)
    {
        try
        {
            var canTransition = await _orderTrackingService.CanTransitionToStatusAsync(trackingId, request.Status);
            return Ok(new { CanTransition = canTransition });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while validating transition", error = ex.Message });
        }
    }

    #endregion

    #region ETA Estimation

    /// <summary>
    /// Güncel ETA bilgisini getirir
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <returns>ETA bilgisi</returns>
    [HttpGet("{trackingId}/eta")]
    [ProducesResponseType(typeof(ETAEstimationDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCurrentETA(Guid trackingId)
    {
        try
        {
            var eta = await _etaEstimationService.GetCurrentETAAsync(trackingId);
            if (eta == null)
            {
                return NotFound(new { message = "ETA estimation not found" });
            }

            return Ok(eta);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting ETA", error = ex.Message });
        }
    }

    /// <summary>
    /// Yeni ETA tahmini oluşturur
    /// </summary>
    /// <param name="request">ETA oluşturma isteği</param>
    /// <returns>Oluşturulan ETA</returns>
    [HttpPost("eta")]
    [ProducesResponseType(typeof(ETAEstimationDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateETAEstimation([FromBody] CreateETAEstimationRequest request)
    {
        try
        {
            var eta = await _etaEstimationService.CreateETAEstimationAsync(request);
            return CreatedAtAction(nameof(GetCurrentETA), new { trackingId = request.OrderTrackingId }, eta);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating ETA estimation", error = ex.Message });
        }
    }

    /// <summary>
    /// ETA tahminini günceller
    /// </summary>
    /// <param name="id">ETA ID</param>
    /// <param name="request">ETA güncelleme isteği</param>
    /// <returns>Güncellenen ETA</returns>
    [HttpPut("eta/{id}")]
    [ProducesResponseType(typeof(ETAEstimationDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateETAEstimation(Guid id, [FromBody] UpdateETAEstimationRequest request)
    {
        try
        {
            var eta = await _etaEstimationService.UpdateETAEstimationAsync(id, request);
            return Ok(eta);
        }
        catch (ArgumentException)
        {
            return NotFound(new { message = "ETA estimation not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating ETA estimation", error = ex.Message });
        }
    }

    /// <summary>
    /// ETA tahminini siler
    /// </summary>
    /// <param name="id">ETA ID</param>
    /// <returns>İçerik yok</returns>
    [HttpDelete("eta/{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteETAEstimation(Guid id)
    {
        try
        {
            var success = await _etaEstimationService.DeleteETAEstimationAsync(id);
            if (!success)
            {
                return NotFound(new { message = "ETA estimation not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting ETA estimation", error = ex.Message });
        }
    }

    /// <summary>
    /// ETA geçmişini getirir
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <returns>ETA listesi</returns>
    [HttpGet("{trackingId}/eta/history")]
    [ProducesResponseType(typeof(List<ETAEstimationDto>), 200)]
    public async Task<IActionResult> GetETAHistory(Guid trackingId)
    {
        try
        {
            var history = await _etaEstimationService.GetETAHistoryAsync(trackingId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting ETA history", error = ex.Message });
        }
    }

    /// <summary>
    /// ETA hesaplar
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <param name="request">Hesaplama isteği</param>
    /// <returns>ETA</returns>
    [HttpPost("{trackingId}/eta/calculate")]
    [ProducesResponseType(typeof(ETAEstimationDto), 200)]
    public async Task<IActionResult> CalculateETA(Guid trackingId, [FromBody] CalculateETARequest? request = null)
    {
        try
        {
            var eta = await _etaEstimationService.CalculateETAAsync(
                trackingId, 
                request?.Latitude, 
                request?.Longitude);
            return Ok(eta);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while calculating ETA", error = ex.Message });
        }
    }

    /// <summary>
    /// ETA doğrular
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <param name="request">Doğrulama isteği</param>
    /// <returns>Doğrulama sonucu</returns>
    [HttpPost("{trackingId}/eta/validate")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> ValidateETA(Guid trackingId, [FromBody] ValidateETARequest request)
    {
        try
        {
            var isValid = await _etaEstimationService.ValidateETAAsync(trackingId, request.EstimatedArrivalTime);
            return Ok(new { IsValid = isValid });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while validating ETA", error = ex.Message });
        }
    }

    /// <summary>
    /// Aktif ETA kayıtlarını getirir
    /// </summary>
    /// <returns>ETA listesi</returns>
    [HttpGet("eta/active")]
    [ProducesResponseType(typeof(List<ETAEstimationDto>), 200)]
    public async Task<IActionResult> GetActiveETAEstimations()
    {
        try
        {
            var etas = await _etaEstimationService.GetActiveETAEstimationsAsync();
            return Ok(etas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting active ETA estimations", error = ex.Message });
        }
    }

    /// <summary>
    /// İki nokta arası mesafeyi hesaplar
    /// </summary>
    /// <param name="request">Mesafe hesaplama isteği</param>
    /// <returns>Mesafe (km)</returns>
    [HttpPost("distance/calculate")]
    [ProducesResponseType(typeof(double), 200)]
    public async Task<IActionResult> CalculateDistance([FromBody] CalculateDistanceRequest request)
    {
        try
        {
            var distance = await _etaEstimationService.CalculateDistanceAsync(
                request.Latitude1, 
                request.Longitude1, 
                request.Latitude2, 
                request.Longitude2);
            return Ok(new { Distance = distance });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while calculating distance", error = ex.Message });
        }
    }

    /// <summary>
    /// Tahmini dakika hesaplar
    /// </summary>
    /// <param name="request">Dakika hesaplama isteği</param>
    /// <returns>Dakika</returns>
    [HttpPost("minutes/calculate")]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<IActionResult> CalculateEstimatedMinutes([FromBody] CalculateMinutesRequest request)
    {
        try
        {
            var minutes = await _etaEstimationService.CalculateEstimatedMinutesAsync(
                request.DistanceKm, 
                request.AverageSpeed);
            return Ok(new { Minutes = minutes });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while calculating estimated minutes", error = ex.Message });
        }
    }

    #endregion

    #region Notifications

    /// <summary>
    /// Takip bildirimi gönderir
    /// </summary>
    /// <param name="request">Bildirim gönderme isteği</param>
    /// <returns>Bildirim yanıtı</returns>
    [HttpPost("notifications/send")]
    [ProducesResponseType(typeof(SendNotificationResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
    {
        try
        {
            var response = await _notificationService.SendNotificationAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while sending notification", error = ex.Message });
        }
    }

    /// <summary>
    /// Takip ID'ye göre bildirimleri getirir
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <returns>Bildirimler</returns>
    [HttpGet("{trackingId}/notifications")]
    [ProducesResponseType(typeof(List<TrackingNotificationDto>), 200)]
    public async Task<IActionResult> GetNotificationsByTrackingId(Guid trackingId)
    {
        try
        {
            var notifications = await _notificationService.GetNotificationsByTrackingIdAsync(trackingId);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting notifications", error = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcı ID'ye göre bildirimleri getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <returns>Bildirimler</returns>
    [HttpGet("notifications/user/{userId}")]
    [ProducesResponseType(typeof(List<TrackingNotificationDto>), 200)]
    public async Task<IActionResult> GetNotificationsByUserId(Guid userId)
    {
        try
        {
            var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting user notifications", error = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcının okunmamış bildirimlerini getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <returns>Bildirimler</returns>
    [HttpGet("notifications/user/{userId}/unread")]
    [ProducesResponseType(typeof(List<TrackingNotificationDto>), 200)]
    public async Task<IActionResult> GetUnreadNotifications(Guid userId)
    {
        try
        {
            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting unread notifications", error = ex.Message });
        }
    }

    /// <summary>
    /// Bildirimi okundu işaretler
    /// </summary>
    /// <param name="notificationId">Bildirim ID</param>
    /// <returns>İçerik yok</returns>
    [HttpPut("notifications/{notificationId}/read")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarkNotificationAsRead(Guid notificationId)
    {
        try
        {
            var success = await _notificationService.MarkNotificationAsReadAsync(notificationId);
            if (!success)
            {
                return NotFound(new { message = "Notification not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while marking notification as read", error = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcının tüm bildirimlerini okundu işaretler
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <returns>İçerik yok</returns>
    [HttpPut("notifications/user/{userId}/read-all")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkAllNotificationsAsRead(Guid userId)
    {
        try
        {
            await _notificationService.MarkAllNotificationsAsReadAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while marking all notifications as read", error = ex.Message });
        }
    }

    /// <summary>
    /// Bildirimi siler
    /// </summary>
    /// <param name="notificationId">Bildirim ID</param>
    /// <returns>İçerik yok</returns>
    [HttpDelete("notifications/{notificationId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteNotification(Guid notificationId)
    {
        try
        {
            var success = await _notificationService.DeleteNotificationAsync(notificationId);
            if (!success)
            {
                return NotFound(new { message = "Notification not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting notification", error = ex.Message });
        }
    }

    /// <summary>
    /// Türe göre bildirimleri getirir
    /// </summary>
    /// <param name="type">Bildirim türü</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <returns>Bildirimler</returns>
    [HttpGet("notifications/type/{type}")]
    [ProducesResponseType(typeof(List<TrackingNotificationDto>), 200)]
    public async Task<IActionResult> GetNotificationsByType(NotificationType type, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var notifications = await _notificationService.GetNotificationsByTypeAsync(type, startDate, endDate);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting notifications by type", error = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcının okunmamış bildirim sayısını getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <returns>Sayı</returns>
    [HttpGet("notifications/user/{userId}/count")]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<IActionResult> GetUnreadNotificationCount(Guid userId)
    {
        try
        {
            var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
            return Ok(new { Count = count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting unread notification count", error = ex.Message });
        }
    }

    #endregion

    #region Settings

    /// <summary>
    /// Kullanıcı ayarlarını getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <returns>Ayarlar</returns>
    [HttpGet("settings/user/{userId}")]
    [ProducesResponseType(typeof(TrackingSettingsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserSettings(Guid userId)
    {
        try
        {
            var settings = await _settingsService.GetUserSettingsAsync(userId);
            if (settings == null)
            {
                return NotFound(new { message = "User settings not found" });
            }

            return Ok(settings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting user settings", error = ex.Message });
        }
    }

    /// <summary>
    /// Mağaza ayarlarını getirir
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <returns>Ayarlar</returns>
    [HttpGet("settings/merchant/{merchantId}")]
    [ProducesResponseType(typeof(TrackingSettingsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMerchantSettings(Guid merchantId)
    {
        try
        {
            var settings = await _settingsService.GetMerchantSettingsAsync(merchantId);
            if (settings == null)
            {
                return NotFound(new { message = "Merchant settings not found" });
            }

            return Ok(settings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting merchant settings", error = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcı ayarları oluşturur
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="request">Ayar güncelleme isteği</param>
    /// <returns>Oluşturulan ayarlar</returns>
    [HttpPost("settings/user/{userId}")]
    [ProducesResponseType(typeof(TrackingSettingsDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateUserSettings(Guid userId, [FromBody] UpdateTrackingSettingsRequest request)
    {
        try
        {
            var isValid = await _settingsService.ValidateSettingsAsync(request);
            if (!isValid)
            {
                return BadRequest(new { message = "Invalid settings" });
            }

            var settings = await _settingsService.CreateUserSettingsAsync(userId, request);
            return CreatedAtAction(nameof(GetUserSettings), new { userId }, settings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating user settings", error = ex.Message });
        }
    }

    /// <summary>
    /// Mağaza ayarları oluşturur
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="request">Ayar güncelleme isteği</param>
    /// <returns>Oluşturulan ayarlar</returns>
    [HttpPost("settings/merchant/{merchantId}")]
    [ProducesResponseType(typeof(TrackingSettingsDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateMerchantSettings(Guid merchantId, [FromBody] UpdateTrackingSettingsRequest request)
    {
        try
        {
            var isValid = await _settingsService.ValidateSettingsAsync(request);
            if (!isValid)
            {
                return BadRequest(new { message = "Invalid settings" });
            }

            var settings = await _settingsService.CreateMerchantSettingsAsync(merchantId, request);
            return CreatedAtAction(nameof(GetMerchantSettings), new { merchantId }, settings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating merchant settings", error = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcı ayarlarını günceller
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="request">Ayar güncelleme isteği</param>
    /// <returns>Güncellenen ayarlar</returns>
    [HttpPut("settings/user/{userId}")]
    [ProducesResponseType(typeof(TrackingSettingsDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateUserSettings(Guid userId, [FromBody] UpdateTrackingSettingsRequest request)
    {
        try
        {
            var isValid = await _settingsService.ValidateSettingsAsync(request);
            if (!isValid)
            {
                return BadRequest(new { message = "Invalid settings" });
            }

            var settings = await _settingsService.UpdateUserSettingsAsync(userId, request);
            return Ok(settings);
        }
        catch (ArgumentException)
        {
            return NotFound(new { message = "User settings not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating user settings", error = ex.Message });
        }
    }

    /// <summary>
    /// Mağaza ayarlarını günceller
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <param name="request">Ayar güncelleme isteği</param>
    /// <returns>Güncellenen ayarlar</returns>
    [HttpPut("settings/merchant/{merchantId}")]
    [ProducesResponseType(typeof(TrackingSettingsDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateMerchantSettings(Guid merchantId, [FromBody] UpdateTrackingSettingsRequest request)
    {
        try
        {
            var isValid = await _settingsService.ValidateSettingsAsync(request);
            if (!isValid)
            {
                return BadRequest(new { message = "Invalid settings" });
            }

            var settings = await _settingsService.UpdateMerchantSettingsAsync(merchantId, request);
            return Ok(settings);
        }
        catch (ArgumentException)
        {
            return NotFound(new { message = "Merchant settings not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating merchant settings", error = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcı ayarlarını siler
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <returns>İçerik yok</returns>
    [HttpDelete("settings/user/{userId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteUserSettings(Guid userId)
    {
        try
        {
            var success = await _settingsService.DeleteUserSettingsAsync(userId);
            if (!success)
            {
                return NotFound(new { message = "User settings not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting user settings", error = ex.Message });
        }
    }

    /// <summary>
    /// Mağaza ayarlarını siler
    /// </summary>
    /// <param name="merchantId">Mağaza ID</param>
    /// <returns>İçerik yok</returns>
    [HttpDelete("settings/merchant/{merchantId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteMerchantSettings(Guid merchantId)
    {
        try
        {
            var success = await _settingsService.DeleteMerchantSettingsAsync(merchantId);
            if (!success)
            {
                return NotFound(new { message = "Merchant settings not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting merchant settings", error = ex.Message });
        }
    }

    /// <summary>
    /// Varsayılan ayarları getirir
    /// </summary>
    /// <returns>Ayarlar</returns>
    [HttpGet("settings/default")]
    [ProducesResponseType(typeof(TrackingSettingsDto), 200)]
    public async Task<IActionResult> GetDefaultSettings()
    {
        try
        {
            var settings = await _settingsService.GetDefaultSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting default settings", error = ex.Message });
        }
    }

    /// <summary>
    /// Ayarları doğrular
    /// </summary>
    /// <param name="request">Ayar doğrulama isteği</param>
    /// <returns>Doğrulama sonucu</returns>
    [HttpPost("settings/validate")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> ValidateSettings([FromBody] UpdateTrackingSettingsRequest request)
    {
        try
        {
            var isValid = await _settingsService.ValidateSettingsAsync(request);
            return Ok(new { IsValid = isValid });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while validating settings", error = ex.Message });
        }
    }

    #endregion

    #region Realtime Data

    /// <summary>
    /// Gerçek zamanlı takip verisini getirir
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <returns>Gerçek zamanlı veri</returns>
    [HttpGet("realtime/{trackingId}")]
    [ProducesResponseType(typeof(RealtimeTrackingData), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRealtimeData(Guid trackingId)
    {
        try
        {
            var data = await _realtimeTrackingService.GetRealtimeDataAsync(trackingId);
            if (data == null)
            {
                return NotFound(new { message = "Realtime tracking data not found" });
            }

            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting realtime data", error = ex.Message });
        }
    }

    /// <summary>
    /// Aktif gerçek zamanlı verileri getirir
    /// </summary>
    /// <returns>Veri listesi</returns>
    [HttpGet("realtime/active")]
    [ProducesResponseType(typeof(List<RealtimeTrackingData>), 200)]
    public async Task<IActionResult> GetActiveRealtimeData()
    {
        try
        {
            var data = await _realtimeTrackingService.GetActiveRealtimeDataAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting active realtime data", error = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcının gerçek zamanlı verilerini getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <returns>Veri listesi</returns>
    [HttpGet("realtime/user/{userId}")]
    [ProducesResponseType(typeof(List<RealtimeTrackingData>), 200)]
    public async Task<IActionResult> GetRealtimeDataByUser(Guid userId)
    {
        try
        {
            var data = await _realtimeTrackingService.GetRealtimeDataByUserAsync(userId);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting user realtime data", error = ex.Message });
        }
    }

    /// <summary>
    /// Kuryenin gerçek zamanlı verilerini getirir
    /// </summary>
    /// <param name="courierId">Kurye ID</param>
    /// <returns>Veri listesi</returns>
    [HttpGet("realtime/courier/{courierId}")]
    [ProducesResponseType(typeof(List<RealtimeTrackingData>), 200)]
    public async Task<IActionResult> GetRealtimeDataByCourier(Guid courierId)
    {
        try
        {
            var data = await _realtimeTrackingService.GetRealtimeDataByCourierAsync(courierId);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting courier realtime data", error = ex.Message });
        }
    }

    /// <summary>
    /// Takibi başlatır
    /// </summary>
    /// <param name="request">Başlatma isteği</param>
    /// <returns>Başarı</returns>
    [HttpPost("start")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> StartTracking([FromBody] StartTrackingRequest request)
    {
        try
        {
            var success = await _realtimeTrackingService.StartTrackingAsync(request.OrderId, request.CourierId);
            return Ok(new { Success = success });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while starting tracking", error = ex.Message });
        }
    }

    /// <summary>
    /// Takibi durdurur
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <returns>Başarı</returns>
    [HttpPost("{trackingId}/stop")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> StopTracking(Guid trackingId)
    {
        try
        {
            var success = await _realtimeTrackingService.StopTrackingAsync(trackingId);
            return Ok(new { Success = success });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while stopping tracking", error = ex.Message });
        }
    }

    /// <summary>
    /// Takibi duraklatır
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <returns>Başarı</returns>
    [HttpPost("{trackingId}/pause")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> PauseTracking(Guid trackingId)
    {
        try
        {
            var success = await _realtimeTrackingService.PauseTrackingAsync(trackingId);
            return Ok(new { Success = success });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while pausing tracking", error = ex.Message });
        }
    }

    /// <summary>
    /// Takibi devam ettirir
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <returns>Başarı</returns>
    [HttpPost("{trackingId}/resume")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> ResumeTracking(Guid trackingId)
    {
        try
        {
            var success = await _realtimeTrackingService.ResumeTrackingAsync(trackingId);
            return Ok(new { Success = success });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while resuming tracking", error = ex.Message });
        }
    }

    /// <summary>
    /// Takip aktif mi kontrol eder
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <returns>Aktiflik</returns>
    [HttpGet("{trackingId}/active")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> IsTrackingActive(Guid trackingId)
    {
        try
        {
            var isActive = await _realtimeTrackingService.IsTrackingActiveAsync(trackingId);
            return Ok(new { IsActive = isActive });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while checking tracking status", error = ex.Message });
        }
    }

    /// <summary>
    /// Takip metriklerini getirir
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <returns>Metrikler</returns>
    [HttpGet("{trackingId}/metrics")]
    [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
    public async Task<IActionResult> GetTrackingMetrics(Guid trackingId)
    {
        try
        {
            var metrics = await _realtimeTrackingService.GetTrackingMetricsAsync(trackingId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting tracking metrics", error = ex.Message });
        }
    }

    /// <summary>
    /// Konumu doğrular
    /// </summary>
    /// <param name="request">Konum doğrulama isteği</param>
    /// <returns>Doğrulama sonucu</returns>
    [HttpPost("location/validate")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> ValidateLocation([FromBody] ValidateLocationRequest request)
    {
        try
        {
            var isValid = await _realtimeTrackingService.ValidateLocationAsync(request.Latitude, request.Longitude);
            return Ok(new { IsValid = isValid });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while validating location", error = ex.Message });
        }
    }

    /// <summary>
    /// Hedefe olan mesafeyi hesaplar
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <param name="request">Mesafe isteği</param>
    /// <returns>Mesafe (km)</returns>
    [HttpPost("{trackingId}/distance")]
    [ProducesResponseType(typeof(double), 200)]
    public async Task<IActionResult> CalculateDistanceToDestination(Guid trackingId, [FromBody] CalculateDistanceToDestinationRequest request)
    {
        try
        {
            var distance = await _realtimeTrackingService.CalculateDistanceToDestinationAsync(trackingId, request.Latitude, request.Longitude);
            return Ok(new { Distance = distance });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while calculating distance to destination", error = ex.Message });
        }
    }

    /// <summary>
    /// Hedefe yakınlığı kontrol eder
    /// </summary>
    /// <param name="trackingId">Takip ID</param>
    /// <param name="request">Yakınlık isteği</param>
    /// <returns>Yakın mı</returns>
    [HttpPost("{trackingId}/nearby")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> IsNearDestination(Guid trackingId, [FromBody] IsNearDestinationRequest request)
    {
        try
        {
            var isNear = await _realtimeTrackingService.IsNearDestinationAsync(trackingId, request.Latitude, request.Longitude, request.ThresholdMeters);
            return Ok(new { IsNear = isNear });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while checking if near destination", error = ex.Message });
        }
    }

    #endregion
}

// Request DTOs
public class CreateTrackingRequest
{
    public Guid OrderId { get; set; }
    public Guid? CourierId { get; set; }
}

public class ValidateTransitionRequest
{
    public TrackingStatus Status { get; set; }
}

public class CalculateETARequest
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class ValidateETARequest
{
    public DateTime EstimatedArrivalTime { get; set; }
}

public class CalculateDistanceRequest
{
    public double Latitude1 { get; set; }
    public double Longitude1 { get; set; }
    public double Latitude2 { get; set; }
    public double Longitude2 { get; set; }
}

public class CalculateMinutesRequest
{
    public double DistanceKm { get; set; }
    public double? AverageSpeed { get; set; }
}

public class StartTrackingRequest
{
    public Guid OrderId { get; set; }
    public Guid? CourierId { get; set; }
}

public class ValidateLocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class CalculateDistanceToDestinationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class IsNearDestinationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double ThresholdMeters { get; set; } = 500;
}
