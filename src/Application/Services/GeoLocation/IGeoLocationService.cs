using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.GeoLocation;

public interface IGeoLocationService
{
    /// <summary>
    /// İki koordinat arasındaki mesafeyi kilometre cinsinden hesaplar (Haversine formula)
    /// </summary>
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);

    /// <summary>
    /// Belirtilen yarıçap içindeki merchantları bulur
    /// </summary>
    Task<Result<IEnumerable<NearbyMerchantResponse>>> GetNearbyMerchantsAsync(
        double userLatitude, 
        double userLongitude, 
        double radiusKm = 10.0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının belirtilen merchant'ın teslimat alanında olup olmadığını kontrol eder
    /// </summary>
    Task<Result<bool>> IsInDeliveryZoneAsync(
        Guid merchantId, 
        double userLatitude, 
        double userLongitude,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcı konumuna göre teslimat süresini hesaplar
    /// </summary>
    Task<Result<DeliveryEstimateResponse>> CalculateDeliveryEstimateAsync(
        Guid merchantId, 
        double userLatitude, 
        double userLongitude,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcı konumuna göre teslimat ücretini hesaplar
    /// </summary>
    Task<Result<decimal>> CalculateDeliveryFeeAsync(
        Guid merchantId, 
        double userLatitude, 
        double userLongitude,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen alan içindeki tüm merchantları bulur (polygon intersection)
    /// </summary>
    Task<Result<IEnumerable<NearbyMerchantResponse>>> GetMerchantsInAreaAsync(
        IEnumerable<GeoPoint> areaPoints,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Konum tabanlı otomatik tamamlama önerileri
    /// </summary>
    Task<Result<IEnumerable<LocationSuggestionResponse>>> GetLocationSuggestionsAsync(
        string query, 
        double? latitude = null, 
        double? longitude = null,
        CancellationToken cancellationToken = default);
}
