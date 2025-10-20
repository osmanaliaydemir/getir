using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.GeoLocation;

/// <summary>
/// Coğrafi konum servisi: mesafe hesaplama, yakındaki merchant bulma, teslimat alanı kontrolü.
/// </summary>
public interface IGeoLocationService
{
    /// <summary>İki koordinat arasındaki mesafeyi kilometre cinsinden hesaplar (Haversine formula).</summary>
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
    /// <summary>Belirtilen yarıçap içindeki merchantları bulur.</summary>
    Task<Result<IEnumerable<NearbyMerchantResponse>>> GetNearbyMerchantsAsync(double userLatitude, double userLongitude, double radiusKm = 10.0, CancellationToken cancellationToken = default);
    /// <summary>Belirtilen yarıçap ve kategori tipine göre merchantları bulur.</summary>
    Task<Result<IEnumerable<NearbyMerchantResponse>>> GetNearbyMerchantsByCategoryAsync(double userLatitude, double userLongitude, ServiceCategoryType categoryType, double radiusKm = 10.0, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcının belirtilen merchant'ın teslimat alanında olup olmadığını kontrol eder.</summary>
    Task<Result<bool>> IsInDeliveryZoneAsync(Guid merchantId, double userLatitude, double userLongitude, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcı konumuna göre teslimat süresini hesaplar.</summary>
    Task<Result<DeliveryEstimateResponse>> CalculateDeliveryEstimateAsync(Guid merchantId, double userLatitude, double userLongitude, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcı konumuna göre teslimat ücretini hesaplar.</summary>
    Task<Result<decimal>> CalculateDeliveryFeeAsync(Guid merchantId, double userLatitude, double userLongitude, CancellationToken cancellationToken = default);
    /// <summary>Belirtilen alan içindeki tüm merchantları bulur (polygon intersection).</summary>
    Task<Result<IEnumerable<NearbyMerchantResponse>>> GetMerchantsInAreaAsync(IEnumerable<GeoPoint> areaPoints, CancellationToken cancellationToken = default);
    /// <summary>Konum tabanlı otomatik tamamlama önerileri.</summary>
    Task<Result<IEnumerable<LocationSuggestionResponse>>> GetLocationSuggestionsAsync(string query, double? latitude = null, double? longitude = null, CancellationToken cancellationToken = default);
    // Additional methods for GeoLocationController
    /// <summary>Kullanıcı konumunu kaydeder.</summary>
    Task<Result> SaveUserLocationAsync(SaveUserLocationRequest request, Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcı konum geçmişini getirir.</summary>
    Task<Result<PagedResult<UserLocationResponse>>> GetUserLocationHistoryAsync(Guid userId, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Belirtilen alan içindeki merchantları getirir.</summary>
    Task<Result<IEnumerable<MerchantInAreaResponse>>> GetMerchantsInAreaAsync(GetMerchantsInAreaRequest request, CancellationToken cancellationToken = default);
    /// <summary>Konum analizlerini getirir.</summary>
    Task<Result<LocationAnalyticsResponse>> GetLocationAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    /// <summary>Teslimat bölgesi kapsama alanını getirir.</summary>
    Task<Result<DeliveryZoneCoverageResponse>> GetDeliveryZoneCoverageAsync(CancellationToken cancellationToken = default);

}
