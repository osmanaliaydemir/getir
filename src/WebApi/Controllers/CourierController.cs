using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Couriers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Kurye işlemlerini yönetmek için kurye controller'ı
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Courier Panel")]
[Authorize]
public class CourierController : BaseController
{
    private readonly ICourierService _courierService;

    public CourierController(ICourierService courierService)
    {
        _courierService = courierService;
    }

    /// <summary>
    /// Kurye dashboard'unu getir
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kurye dashboard verileri</returns>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(CourierDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCourierDashboard(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _courierService.GetCourierDashboardAsync(courierId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kurye istatistiklerini getir
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kurye istatistikleri</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(CourierStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCourierStats(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _courierService.GetCourierStatsAsync(courierId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kurye kazançlarını getir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kurye kazançları</returns>
    [HttpGet("earnings")]
    [ProducesResponseType(typeof(CourierEarningsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCourierEarnings(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _courierService.GetCourierEarningsAsync(courierId, startDate, endDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Atanmış siparişleri getir
    /// </summary>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış atanmış siparişler</returns>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(PagedResult<CourierOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCourierOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var query = new PaginationQuery { Page = page, PageSize = pageSize };
        var result = await _courierService.GetAssignedOrdersAsync(courierId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Siparişi kabul et
    /// </summary>
    /// <param name="request">Sipariş kabul talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("orders/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AcceptOrder(
        [FromBody] AcceptOrderRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _courierService.AcceptOrderAsync(courierId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Teslimatı başlat
    /// </summary>
    /// <param name="request">Teslimat başlatma talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("orders/start-delivery")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> StartDelivery(
        [FromBody] StartDeliveryRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _courierService.StartDeliveryAsync(courierId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Teslimatı tamamla
    /// </summary>
    /// <param name="request">Teslimat tamamlama talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("orders/complete-delivery")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CompleteDelivery(
        [FromBody] CompleteDeliveryRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _courierService.CompleteDeliveryAsync(courierId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kurye konumunu güncelle
    /// </summary>
    /// <param name="request">Konum güncelleme talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPut("location")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCourierLocation(
        [FromBody] UpdateCourierLocationRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var locationRequest = new CourierLocationUpdateRequest(
            request.Latitude,
            request.Longitude);
        var result = await _courierService.UpdateLocationAsync(courierId, locationRequest, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kurye uygunluğunu ayarla
    /// </summary>
    /// <param name="request">Uygunluk ayarlama talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPut("availability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetCourierAvailability(
        [FromBody] SetAvailabilityRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _courierService.SetAvailabilityAsync(courierId, request, ct);
        return ToActionResult(result);
    }
}

/// <summary>
/// Admin kurye yönetimi için admin kurye controller'ı
/// </summary>
[ApiController]
[Route("api/v1/admin/[controller]")]
[Tags("Admin - Courier Management")]
[Authorize(Policy = "Admin")]
public class AdminCourierController : BaseController
{
    private readonly ICourierService _courierService;

    public AdminCourierController(ICourierService courierService)
    {
        _courierService = courierService;
    }

    /// <summary>
    /// Siparişi kuryeye ata
    /// </summary>
    /// <param name="request">Sipariş atama talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Atama yanıtı</returns>
    [HttpPost("assign-order")]
    [ProducesResponseType(typeof(CourierAssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignOrder(
        [FromBody] AssignOrderRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _courierService.AssignOrderAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// En yakın kuryeleri bul
    /// </summary>
    /// <param name="request">En yakın kuryeleri bulma talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>En yakın kuryeler</returns>
    [HttpPost("find-nearest")]
    [ProducesResponseType(typeof(FindNearestCouriersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FindNearestCouriers(
        [FromBody] FindNearestCouriersRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _courierService.FindNearestCouriersAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// En iyi performans gösteren kuryeleri getir
    /// </summary>
    /// <param name="count">Döndürülecek kurye sayısı</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>En iyi performans gösterenler</returns>
    [HttpGet("top-performers")]
    [ProducesResponseType(typeof(List<CourierPerformanceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopPerformers(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var result = await _courierService.GetTopPerformersAsync(count, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kurye kazanç detayını getir
    /// </summary>
    /// <param name="courierId">Kurye ID'si</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kazanç detayı</returns>
    [HttpGet("earnings-detail")]
    [ProducesResponseType(typeof(CourierEarningsDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourierEarningsDetail(
        [FromQuery] Guid courierId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var query = new CourierEarningsQuery(courierId, startDate, endDate);
        var result = await _courierService.GetEarningsDetailAsync(query, ct);
        return ToActionResult(result);
    }
}
