using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.GeoLocation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// GeoLocation controller for location-based operations
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
    /// Get nearby merchants within specified radius
    /// </summary>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    /// <param name="radius">Radius in kilometers</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Nearby merchants</returns>
    [HttpGet("merchants/nearby")]
    [ProducesResponseType(typeof(IEnumerable<NearbyMerchantResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNearbyMerchants(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radius = 5.0,
        CancellationToken ct = default)
    {
        var result = await _geoLocationService.GetNearbyMerchantsAsync(latitude, longitude, radius, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get location suggestions for autocomplete
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Location suggestions</returns>
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
    /// Calculate delivery estimate for merchant
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="latitude">Customer latitude</param>
    /// <param name="longitude">Customer longitude</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Delivery estimate</returns>
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
    /// Calculate delivery fee for merchant
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="latitude">Customer latitude</param>
    /// <param name="longitude">Customer longitude</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Delivery fee</returns>
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
    /// Save user location
    /// </summary>
    /// <param name="request">Save location request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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

        return BadRequest("Save user location functionality not implemented yet");
    }

    /// <summary>
    /// Get user location history
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged location history</returns>
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

        return BadRequest("Get user location history functionality not implemented yet");
    }

    /// <summary>
    /// Get merchants in area
    /// </summary>
    /// <param name="request">Area search request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Merchants in area</returns>
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

        return BadRequest("Get merchants in area functionality not implemented yet");
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Get location analytics
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Location analytics</returns>
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
        return BadRequest("Get location analytics functionality not implemented yet");
    }

    /// <summary>
    /// Get delivery zone coverage
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Delivery zone coverage</returns>
    [HttpGet("delivery-zones/coverage")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DeliveryZoneCoverageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDeliveryZoneCoverage(CancellationToken ct = default)
    {
        return BadRequest("Get delivery zone coverage functionality not implemented yet");
    }

    #endregion
}