namespace Getir.Application.DTO;

/// <summary>
/// Yakındaki merchant bilgileri
/// </summary>
public record NearbyMerchantResponse(
    Guid Id,
    string Name,
    string? Description,
    string Address,
    double DistanceKm,
    decimal DeliveryFee,
    int EstimatedDeliveryTimeMinutes,
    decimal? Rating,
    int TotalReviews,
    bool IsOpen,
    string? LogoUrl);

/// <summary>
/// Teslimat tahmini bilgileri
/// </summary>
public record DeliveryEstimateResponse(
    int EstimatedDeliveryTimeMinutes,
    decimal DeliveryFee,
    bool IsInDeliveryZone,
    string? ZoneName,
    double DistanceKm);

/// <summary>
/// Konum önerileri
/// </summary>
public record LocationSuggestionResponse(
    string Address,
    double Latitude,
    double Longitude,
    string? District,
    string? City,
    double? DistanceKm);

/// <summary>
/// Coğrafi nokta
/// </summary>
public record GeoPoint(
    double Latitude,
    double Longitude);

/// <summary>
/// Yakındaki merchantları arama sorgusu
/// </summary>
public record NearbyMerchantsQuery(
    double Latitude,
    double Longitude,
    double? RadiusKm = 10.0,
    Guid? ServiceCategoryId = null,
    bool? IsOpen = null,
    decimal? MinRating = null);

/// <summary>
/// Konum tabanlı arama sorgusu
/// </summary>
public record LocationSearchQuery(
    string Query,
    double? Latitude = null,
    double? Longitude = null,
    int MaxResults = 10);

