using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Getir.Application.DTO;
using Getir.Application.Services.GeoLocation;
using Getir.WebApi.Extensions;
using Getir.Application.Common;
using System.Security.Claims;

namespace Getir.WebApi.Endpoints;

public static class GeoLocationEndpoints
{
    public static void MapGeoLocationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/geo")
            .WithTags("GeoLocation");

        // === PUBLIC ENDPOINTS (Authentication gerekmez) ===
        
        // Yakındaki merchantları getir
        group.MapGet("/merchants/nearby", GetNearbyMerchants)
            .WithName("GetNearbyMerchants")
            .WithSummary("Get nearby merchants within specified radius")
            .Produces<IEnumerable<NearbyMerchantResponse>>()
            .Produces<ProblemDetails>(400);

        // Konum önerileri
        group.MapGet("/suggestions", GetLocationSuggestions)
            .WithName("GetLocationSuggestions")
            .WithSummary("Get location suggestions for autocomplete")
            .Produces<IEnumerable<LocationSuggestionResponse>>()
            .Produces<ProblemDetails>(400);

        // Teslimat tahmini hesapla
        group.MapGet("/delivery/estimate", CalculateDeliveryEstimate)
            .WithName("CalculateDeliveryEstimate")
            .WithSummary("Calculate delivery estimate for merchant")
            .Produces<DeliveryEstimateResponse>()
            .Produces<ProblemDetails>(400);

        // Teslimat ücreti hesapla
        group.MapGet("/delivery/fee", CalculateDeliveryFee)
            .WithName("CalculateDeliveryFee")
            .WithSummary("Calculate delivery fee for merchant")
            .Produces<decimal>()
            .Produces<ProblemDetails>(400);

        // === CUSTOMER ENDPOINTS ===
        
        // Kullanıcı konumunu kaydet
        group.MapPost("/location/save", SaveUserLocation)
            .WithName("SaveUserLocation")
            .WithSummary("Save user's current location")
            .RequireAuthorization()
            .Produces(204)
            .Produces<ProblemDetails>(400);

        // === MERCHANT ENDPOINTS ===
        // Delivery zone yönetimi DeliveryZoneEndpoints'te yapılıyor

        // === ADMIN ENDPOINTS ===
        
        // Tüm merchantları alan içinde ara
        group.MapPost("/merchants/in-area", GetMerchantsInArea)
            .WithName("GetMerchantsInArea")
            .WithSummary("Get merchants within specified area")
            .RequireAuthorization()
            .Produces<IEnumerable<NearbyMerchantResponse>>()
            .Produces<ProblemDetails>(400);
    }

    #region Public Endpoints

    private static async Task<IResult> GetNearbyMerchants(
        [AsParameters] NearbyMerchantsQuery query,
        IGeoLocationService geoLocationService,
        CancellationToken cancellationToken)
    {
        var result = await geoLocationService.GetNearbyMerchantsAsync(
            query.Latitude,
            query.Longitude,
            query.RadiusKm ?? 10.0,
            cancellationToken);

        return result.ToApiResult();
    }

    private static async Task<IResult> GetLocationSuggestions(
        [AsParameters] LocationSearchQuery query,
        IGeoLocationService geoLocationService,
        CancellationToken cancellationToken)
    {
        var result = await geoLocationService.GetLocationSuggestionsAsync(
            query.Query,
            query.Latitude,
            query.Longitude,
            cancellationToken);

        return result.ToApiResult();
    }

    private static async Task<IResult> CalculateDeliveryEstimate(
        Guid merchantId,
        double latitude,
        double longitude,
        IGeoLocationService geoLocationService,
        CancellationToken cancellationToken)
    {
        var result = await geoLocationService.CalculateDeliveryEstimateAsync(
            merchantId,
            latitude,
            longitude,
            cancellationToken);

        return result.ToApiResult();
    }

    private static async Task<IResult> CalculateDeliveryFee(
        Guid merchantId,
        double latitude,
        double longitude,
        IGeoLocationService geoLocationService,
        CancellationToken cancellationToken)
    {
        var result = await geoLocationService.CalculateDeliveryFeeAsync(
            merchantId,
            latitude,
            longitude,
            cancellationToken);

        return result.ToApiResult();
    }

    #endregion

    #region Customer Endpoints

    private static async Task<IResult> SaveUserLocation(
        GeoPoint location,
        ClaimsPrincipal user,
        IGeoLocationService geoLocationService,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        
        // TODO: UserLocation entity'si eklenip database'e kaydedilecek
        // Şimdilik sadece log olarak kaydediyoruz
        
        return Results.NoContent();
    }

    #endregion

    #region Merchant Endpoints
    // Delivery zone yönetimi DeliveryZoneEndpoints'te yapılıyor
    #endregion

    #region Admin Endpoints

    private static async Task<IResult> GetMerchantsInArea(
        IEnumerable<GeoPoint> areaPoints,
        IGeoLocationService geoLocationService,
        CancellationToken cancellationToken)
    {
        var result = await geoLocationService.GetMerchantsInAreaAsync(
            areaPoints,
            cancellationToken);

        return result.ToApiResult();
    }

    #endregion
}
