using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.DeliveryOptimization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Teslimat optimizasyonu controller'ı
/// Teslimat kapasitesi yönetimi ve rota optimizasyonu endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeliveryOptimizationController : BaseController
{
    private readonly IDeliveryCapacityService _deliveryCapacityService;
    private readonly IRouteOptimizationService _routeOptimizationService;

    public DeliveryOptimizationController(
        IDeliveryCapacityService deliveryCapacityService,
        IRouteOptimizationService routeOptimizationService)
    {
        _deliveryCapacityService = deliveryCapacityService;
        _routeOptimizationService = routeOptimizationService;
    }

    #region Delivery Capacity Management

    /// <summary>
    /// Teslimat kapasitesi ayarlarını oluşturur
    /// </summary>
    [HttpPost("capacity")]
    [ProducesResponseType(typeof(DeliveryCapacityResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateCapacity([FromBody] DeliveryCapacityRequest request)
    {
        var result = await _deliveryCapacityService.CreateCapacityAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to create delivery capacity", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Teslimat kapasitesi ayarlarını günceller
    /// </summary>
    [HttpPut("capacity/{capacityId}")]
    [ProducesResponseType(typeof(DeliveryCapacityResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateCapacity(
        [FromRoute] Guid capacityId,
        [FromBody] DeliveryCapacityRequest request)
    {
        var result = await _deliveryCapacityService.UpdateCapacityAsync(capacityId, request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to update delivery capacity", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Teslimat kapasitesini kontrol eder
    /// </summary>
    [HttpPost("capacity/check")]
    [ProducesResponseType(typeof(DeliveryCapacityCheckResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CheckCapacity([FromBody] DeliveryCapacityCheckRequest request)
    {
        var result = await _deliveryCapacityService.CheckCapacityAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to check delivery capacity", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Merchant'ın teslimat kapasitesini getirir
    /// </summary>
    [HttpGet("capacity/merchant/{merchantId}")]
    [ProducesResponseType(typeof(DeliveryCapacityResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCapacity(
        [FromRoute] Guid merchantId,
        [FromQuery] Guid? deliveryZoneId = null)
    {
        var result = await _deliveryCapacityService.GetCapacityAsync(merchantId, deliveryZoneId);
        
        if (!result.Success)
        {
            return NotFound(new { Error = result.Error ?? "Delivery capacity not found", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Dinamik kapasite ayarı yapar
    /// </summary>
    [HttpPost("capacity/adjust")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AdjustCapacity([FromBody] DynamicCapacityAdjustmentRequest request)
    {
        var result = await _deliveryCapacityService.AdjustCapacityAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to adjust capacity", ErrorCode = result.ErrorCode });
        }

        return Ok();
    }

    /// <summary>
    /// Kapasite uyarısı gönderir
    /// </summary>
    [HttpPost("capacity/alert")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SendCapacityAlert([FromBody] CapacityAlertRequest request)
    {
        var result = await _deliveryCapacityService.SendCapacityAlertAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to send capacity alert", ErrorCode = result.ErrorCode });
        }

        return Ok();
    }

    #endregion

    #region Route Optimization

    /// <summary>
    /// Alternatif rotalar önerir
    /// </summary>
    [HttpPost("routes/alternatives")]
    [ProducesResponseType(typeof(RouteOptimizationResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetAlternativeRoutes([FromBody] RouteOptimizationRequest request)
    {
        var result = await _routeOptimizationService.GetAlternativeRoutesAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get alternative routes", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// En iyi rotayı seçer
    /// </summary>
    [HttpPost("routes/best")]
    [ProducesResponseType(typeof(DeliveryRouteResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SelectBestRoute(
        [FromBody] RouteOptimizationRequest request,
        [FromQuery] bool? avoidTollRoads = null,
        [FromQuery] string? travelMode = null)
    {
        var preferences = new RoutePreferences(
            AvoidTollRoads: avoidTollRoads ?? false,
            TravelMode: travelMode ?? "DRIVING");

        var result = await _routeOptimizationService.SelectBestRouteAsync(request, preferences);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to select best route", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Rota seçimini kaydeder
    /// </summary>
    [HttpPost("routes/select")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SelectRoute([FromBody] RouteSelectionRequest request)
    {
        var result = await _routeOptimizationService.SelectRouteAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to select route", ErrorCode = result.ErrorCode });
        }

        return Ok();
    }

    /// <summary>
    /// Rota durumunu günceller
    /// </summary>
    [HttpPut("routes/{routeId}/status")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateRouteStatus(
        [FromRoute] Guid routeId,
        [FromBody] RouteUpdateRequest request)
    {
        var result = await _routeOptimizationService.UpdateRouteStatusAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to update route status", ErrorCode = result.ErrorCode });
        }

        return Ok();
    }

    /// <summary>
    /// Sipariş için rota oluşturur
    /// </summary>
    [HttpPost("routes/order/{orderId}")]
    [ProducesResponseType(typeof(DeliveryRouteResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateRouteForOrder(
        [FromRoute] Guid orderId,
        [FromBody] RouteOptimizationRequest request)
    {
        var result = await _routeOptimizationService.CreateRouteForOrderAsync(orderId, request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to create route for order", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Rota geçmişini getirir
    /// </summary>
    [HttpGet("routes/order/{orderId}/history")]
    [ProducesResponseType(typeof(List<DeliveryRouteResponse>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetRouteHistory([FromRoute] Guid orderId)
    {
        var result = await _routeOptimizationService.GetRouteHistoryAsync(orderId);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get route history", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gerçek zamanlı rota güncellemesi
    /// </summary>
    [HttpPut("routes/{routeId}/realtime")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateRouteRealTime(
        [FromRoute] Guid routeId,
        [FromQuery] double latitude,
        [FromQuery] double longitude)
    {
        var result = await _routeOptimizationService.UpdateRouteInRealTimeAsync(routeId, latitude, longitude);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to update route in real time", ErrorCode = result.ErrorCode });
        }

        return Ok();
    }

    /// <summary>
    /// Trafik durumuna göre rota önerir
    /// </summary>
    [HttpPost("routes/traffic-optimized")]
    [ProducesResponseType(typeof(RouteOptimizationResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetTrafficOptimizedRoutes([FromBody] RouteOptimizationRequest request)
    {
        var result = await _routeOptimizationService.GetTrafficOptimizedRoutesAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to get traffic optimized routes", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Çoklu nokta rota optimizasyonu
    /// </summary>
    [HttpPost("routes/multi-point")]
    [ProducesResponseType(typeof(RouteOptimizationResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> OptimizeMultiPointRoute(
        [FromBody] List<RouteWaypoint> waypoints,
        [FromQuery] bool? avoidTollRoads = null,
        [FromQuery] string? travelMode = null)
    {
        if (waypoints.Count < 2)
        {
            return BadRequest(new { Error = "At least 2 waypoints required", ErrorCode = "INSUFFICIENT_WAYPOINTS" });
        }

        var preferences = new RoutePreferences(
            AvoidTollRoads: avoidTollRoads ?? false,
            TravelMode: travelMode ?? "DRIVING");

        var result = await _routeOptimizationService.OptimizeMultiPointRouteAsync(waypoints, preferences);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to optimize multi-point route", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    #endregion

    #region Performance Analysis

    /// <summary>
    /// Rota performansını analiz eder
    /// </summary>
    [HttpPost("performance/analyze")]
    [ProducesResponseType(typeof(DeliveryPerformanceResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AnalyzeRoutePerformance([FromBody] DeliveryPerformanceRequest request)
    {
        var result = await _routeOptimizationService.AnalyzeRoutePerformanceAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to analyze route performance", ErrorCode = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    #endregion

    #region Capacity Management Operations

    /// <summary>
    /// Aktif teslimat sayısını artırır
    /// </summary>
    [HttpPost("capacity/increment-active")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> IncrementActiveDeliveries(
        [FromQuery] Guid merchantId,
        [FromQuery] Guid? deliveryZoneId = null)
    {
        var result = await _deliveryCapacityService.IncrementActiveDeliveriesAsync(merchantId, deliveryZoneId);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to increment active deliveries", ErrorCode = result.ErrorCode });
        }

        return Ok();
    }

    /// <summary>
    /// Aktif teslimat sayısını azaltır
    /// </summary>
    [HttpPost("capacity/decrement-active")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> DecrementActiveDeliveries(
        [FromQuery] Guid merchantId,
        [FromQuery] Guid? deliveryZoneId = null)
    {
        var result = await _deliveryCapacityService.DecrementActiveDeliveriesAsync(merchantId, deliveryZoneId);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to decrement active deliveries", ErrorCode = result.ErrorCode });
        }

        return Ok();
    }

    /// <summary>
    /// Günlük teslimat sayısını artırır
    /// </summary>
    [HttpPost("capacity/increment-daily")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> IncrementDailyDeliveries(
        [FromQuery] Guid merchantId,
        [FromQuery] Guid? deliveryZoneId = null)
    {
        var result = await _deliveryCapacityService.IncrementDailyDeliveriesAsync(merchantId, deliveryZoneId);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to increment daily deliveries", ErrorCode = result.ErrorCode });
        }

        return Ok();
    }

    /// <summary>
    /// Günlük kapasite sayaçlarını sıfırlar
    /// </summary>
    [HttpPost("capacity/reset-daily")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResetDailyCounters()
    {
        var result = await _deliveryCapacityService.ResetDailyCountersAsync();
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to reset daily counters", ErrorCode = result.ErrorCode });
        }

        return Ok();
    }

    /// <summary>
    /// Haftalık kapasite sayaçlarını sıfırlar
    /// </summary>
    [HttpPost("capacity/reset-weekly")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResetWeeklyCounters()
    {
        var result = await _deliveryCapacityService.ResetWeeklyCountersAsync();
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Error ?? "Failed to reset weekly counters", ErrorCode = result.ErrorCode });
        }

        return Ok();
    }

    #endregion
}
