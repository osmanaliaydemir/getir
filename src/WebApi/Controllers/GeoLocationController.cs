using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.GeoLocation;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Konum tabanlı işlemler için GeoLocation controller'ı
/// </summary>
[ApiController]
[Route("api/v1/geo")]
[Tags("GeoLocation")]
public class GeoLocationController : BaseController
{
    private readonly IGeoLocationService _geoLocationService;

    public GeoLocationController(IGeoLocationService geoLocationService)
    {
        _geoLocationService = geoLocationService;
    }

    #region Public Endpoints

    /// <summary>
    /// Belirtilen yarıçap içindeki yakındaki merchant'ları alır, isteğe bağlı olarak kategori türüne göre filtrelenebilir
    /// </summary>
    /// <param name="latitude">Enlem</param>
    /// <param name="longitude">Boylam</param>
    /// <param name="categoryType">İsteğe bağlı: Hizmet kategori türü (Restaurant=1, Market=2, Pharmacy=3, Water=4, Cafe=5, Bakery=6, Other=99)</param>
    /// <param name="radius">Kilometre cinsinden yarıçap</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Yakındaki merchant'lar</returns>
    [HttpGet("merchants/nearby")]
    [ProducesResponseType(typeof(IEnumerable<NearbyMerchantResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNearbyMerchants(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] ServiceCategoryType? categoryType = null,
        [FromQuery] double radius = 5.0,
        CancellationToken ct = default)
    {
        Result<IEnumerable<NearbyMerchantResponse>> result;
        
        if (categoryType.HasValue)
        {
            result = await _geoLocationService.GetNearbyMerchantsByCategoryAsync(
                latitude, longitude, categoryType.Value, radius, ct);
        }
        else
        {
            result = await _geoLocationService.GetNearbyMerchantsAsync(
                latitude, longitude, radius, ct);
        }
        
        return ToActionResult(result);
    }

    /// <summary>
    /// Otomatik tamamlama için konum önerilerini al
    /// </summary>
    /// <param name="query">Arama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Konum önerileri</returns>
    [HttpGet("suggestions")]
    [ProducesResponseType(typeof(IEnumerable<LocationSuggestionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLocationSuggestions(
        [FromQuery] string query,
        CancellationToken ct = default)
    {
        var result = await _geoLocationService.GetLocationSuggestionsAsync(query, null, null, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Merchant için teslimat tahmini hesapla
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="latitude">Müşteri enlemi</param>
    /// <param name="longitude">Müşteri boylamı</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Teslimat tahmini</returns>
    [HttpGet("delivery/estimate")]
    [ProducesResponseType(typeof(DeliveryEstimateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculateDeliveryEstimate(
        [FromQuery] Guid merchantId,
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        CancellationToken ct = default)
    {
        var result = await _geoLocationService.CalculateDeliveryEstimateAsync(merchantId, latitude, longitude, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Merchant için teslimat ücreti hesapla
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="latitude">Müşteri enlemi</param>
    /// <param name="longitude">Müşteri boylamı</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Teslimat ücreti</returns>
    [HttpGet("delivery/fee")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculateDeliveryFee(
        [FromQuery] Guid merchantId,
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        CancellationToken ct = default)
    {
        var result = await _geoLocationService.CalculateDeliveryFeeAsync(merchantId, latitude, longitude, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Customer Endpoints

    /// <summary>
    /// Kullanıcı konumunu kaydet
    /// </summary>
    /// <param name="request">Konum kaydetme isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("location")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveUserLocation(
        [FromBody] SaveUserLocationRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _geoLocationService.SaveUserLocationAsync(request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kullanıcı konum geçmişini al
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış konum geçmişi</returns>
    [HttpGet("location/history")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<UserLocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserLocationHistory(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _geoLocationService.GetUserLocationHistoryAsync(userId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Bölgedeki merchant'ları al
    /// </summary>
    /// <param name="request">Bölge arama isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Bölgedeki merchant'lar</returns>
    [HttpPost("merchants/area")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<MerchantInAreaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMerchantsInArea(
        [FromBody] GetMerchantsInAreaRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _geoLocationService.GetMerchantsInAreaAsync(request, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Konum analitiklerini al
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Konum analitikleri</returns>
    [HttpGet("analytics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(LocationAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLocationAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var result = await _geoLocationService.GetLocationAnalyticsAsync(startDate, endDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Teslimat bölgesi kapsamını al
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Teslimat bölgesi kapsamı</returns>
    [HttpGet("delivery-zones/coverage")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DeliveryZoneCoverageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDeliveryZoneCoverage(CancellationToken ct = default)
    {
        var result = await _geoLocationService.GetDeliveryZoneCoverageAsync(ct);
        return ToActionResult(result);
    }

    #endregion
}