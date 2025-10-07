using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.GeoLocation;

public class GeoLocationService : BaseService, IGeoLocationService
{
    private const double EarthRadiusKm = 6371.0; // Dünya yarıçapı (km)

    public GeoLocationService(
        IUnitOfWork unitOfWork,
        ILogger<GeoLocationService> logger,
        ILoggingService loggingService,
        ICacheService cacheService) 
        : base(unitOfWork, logger, loggingService, cacheService)
    {
    }

    /// <summary>
    /// Haversine formula ile iki koordinat arasındaki mesafeyi hesaplar
    /// </summary>
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Dereceyi radyana çevir
        var lat1Rad = DegreesToRadians(lat1);
        var lon1Rad = DegreesToRadians(lon1);
        var lat2Rad = DegreesToRadians(lat2);
        var lon2Rad = DegreesToRadians(lon2);

        // Farkları hesapla
        var deltaLat = lat2Rad - lat1Rad;
        var deltaLon = lon2Rad - lon1Rad;

        // Haversine formula
        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = EarthRadiusKm * c;

        return Math.Round(distance, 2); // 2 ondalık basamak
    }

    /// <summary>
    /// Belirtilen yarıçap içindeki merchantları bulur
    /// </summary>
    public async Task<Result<IEnumerable<NearbyMerchantResponse>>> GetNearbyMerchantsAsync(
        double userLatitude, 
        double userLongitude, 
        double radiusKm = 10.0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var merchants = await _unitOfWork.ReadRepository<Merchant>()
                .ListAsync(
                    filter: m => m.IsActive,
                    include: "ServiceCategory",
                    cancellationToken: cancellationToken);

            var nearbyMerchants = new List<NearbyMerchantResponse>();

            foreach (var merchant in merchants)
            {
                var distance = CalculateDistance(
                    userLatitude, 
                    userLongitude, 
                    (double)merchant.Latitude, 
                    (double)merchant.Longitude);

                if (distance <= radiusKm)
                {
                    // Delivery zone kontrolü
                    var isInDeliveryZone = await IsInDeliveryZoneAsync(
                        merchant.Id, 
                        userLatitude, 
                        userLongitude, 
                        cancellationToken);

                    if (isInDeliveryZone.Success && isInDeliveryZone.Value)
                    {
                        var deliveryEstimate = await CalculateDeliveryEstimateAsync(
                            merchant.Id, 
                            userLatitude, 
                            userLongitude, 
                            cancellationToken);

                        nearbyMerchants.Add(new NearbyMerchantResponse(
                            merchant.Id,
                            merchant.Name,
                            merchant.Description,
                            merchant.Address,
                            distance,
                            deliveryEstimate.Success ? deliveryEstimate.Value?.DeliveryFee ?? merchant.DeliveryFee : merchant.DeliveryFee,
                            deliveryEstimate.Success ? deliveryEstimate.Value?.EstimatedDeliveryTimeMinutes ?? merchant.AverageDeliveryTime : merchant.AverageDeliveryTime,
                            merchant.Rating,
                            merchant.TotalReviews,
                            merchant.IsOpen,
                            merchant.LogoUrl,
                            merchant.ServiceCategory?.Type
                        ));
                    }
                }
            }

            // Mesafeye göre sırala
            var sortedMerchants = nearbyMerchants
                .OrderBy(m => m.DistanceKm)
                .ThenByDescending(m => m.Rating ?? 0)
                .ToList();

            _loggingService.LogBusinessEvent("NearbyMerchantsFound", new { 
                sortedMerchants.Count, 
                radiusKm 
            });

            return Result.Ok(sortedMerchants.AsEnumerable());
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting nearby merchants", ex, 
                new { userLatitude, userLongitude, radiusKm });
            return Result.Fail<IEnumerable<NearbyMerchantResponse>>("Error getting nearby merchants", "GEO_LOCATION_ERROR");
        }
    }

    /// <summary>
    /// Belirtilen yarıçap ve kategori tipine göre merchantları bulur
    /// </summary>
    public async Task<Result<IEnumerable<NearbyMerchantResponse>>> GetNearbyMerchantsByCategoryAsync(
        double userLatitude,
        double userLongitude,
        ServiceCategoryType categoryType,
        double radiusKm = 10.0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (userLatitude < -90 || userLatitude > 90)
            {
                return Result.Fail<IEnumerable<NearbyMerchantResponse>>(
                    "Invalid latitude. Must be between -90 and 90", 
                    "INVALID_LATITUDE");
            }

            if (userLongitude < -180 || userLongitude > 180)
            {
                return Result.Fail<IEnumerable<NearbyMerchantResponse>>(
                    "Invalid longitude. Must be between -180 and 180", 
                    "INVALID_LONGITUDE");
            }

            if (radiusKm <= 0 || radiusKm > 50)
            {
                return Result.Fail<IEnumerable<NearbyMerchantResponse>>(
                    "Radius must be between 0 and 50 km", 
                    "INVALID_RADIUS");
            }

            // Get merchants filtered by category type and active status
            var merchants = await _unitOfWork.ReadRepository<Merchant>()
                .ListAsync(
                    filter: m => m.IsActive && m.ServiceCategory.Type == categoryType,
                    include: "ServiceCategory",
                    cancellationToken: cancellationToken);

            var nearbyMerchants = new List<NearbyMerchantResponse>();

            foreach (var merchant in merchants)
            {
                var distance = CalculateDistance(
                    userLatitude, 
                    userLongitude, 
                    (double)merchant.Latitude, 
                    (double)merchant.Longitude);

                if (distance <= radiusKm)
                {
                    // Delivery zone kontrolü
                    var isInDeliveryZone = await IsInDeliveryZoneAsync(
                        merchant.Id, 
                        userLatitude, 
                        userLongitude, 
                        cancellationToken);

                    if (isInDeliveryZone.Success && isInDeliveryZone.Value)
                    {
                        var deliveryEstimate = await CalculateDeliveryEstimateAsync(
                            merchant.Id, 
                            userLatitude, 
                            userLongitude, 
                            cancellationToken);

                        nearbyMerchants.Add(new NearbyMerchantResponse(
                            merchant.Id,
                            merchant.Name,
                            merchant.Description,
                            merchant.Address,
                            distance,
                            deliveryEstimate.Success ? deliveryEstimate.Value?.DeliveryFee ?? merchant.DeliveryFee : merchant.DeliveryFee,
                            deliveryEstimate.Success ? deliveryEstimate.Value?.EstimatedDeliveryTimeMinutes ?? merchant.AverageDeliveryTime : merchant.AverageDeliveryTime,
                            merchant.Rating,
                            merchant.TotalReviews,
                            merchant.IsOpen,
                            merchant.LogoUrl,
                            merchant.ServiceCategory?.Type));
                    }
                }
            }

            // Mesafeye göre sırala
            var sortedMerchants = nearbyMerchants
                .OrderBy(m => m.DistanceKm)
                .ThenByDescending(m => m.Rating ?? 0)
                .ToList();

            _loggingService.LogBusinessEvent("NearbyMerchantsByCategoryFound", new { 
                sortedMerchants.Count, 
                categoryType = categoryType.ToString(),
                radiusKm 
            });

            return Result.Ok(sortedMerchants.AsEnumerable());
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting nearby merchants by category", ex, 
                new { userLatitude, userLongitude, categoryType = categoryType.ToString(), radiusKm });
            return Result.Fail<IEnumerable<NearbyMerchantResponse>>(
                "Error getting nearby merchants by category", 
                "GEO_LOCATION_ERROR");
        }
    }

    /// <summary>
    /// Kullanıcının belirtilen merchant'ın teslimat alanında olup olmadığını kontrol eder
    /// </summary>
    public async Task<Result<bool>> IsInDeliveryZoneAsync(
        Guid merchantId, 
        double userLatitude, 
        double userLongitude,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deliveryZones = await _unitOfWork.ReadRepository<DeliveryZone>()
                .ListAsync(
                    filter: dz => dz.MerchantId == merchantId && dz.IsActive,
                    include: "Points",
                    cancellationToken: cancellationToken);

            if (!deliveryZones.Any())
            {
                // Delivery zone yoksa, merchant'ın varsayılan alanını kullan (10km radius)
                var merchant = await _unitOfWork.ReadRepository<Merchant>()
                    .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken: cancellationToken);

                if (merchant == null)
                {
                    return Result.Fail<bool>("Merchant not found", ErrorCodes.MERCHANT_NOT_FOUND);
                }

                var distance = CalculateDistance(
                    userLatitude, 
                    userLongitude, 
                    (double)merchant.Latitude, 
                    (double)merchant.Longitude);

                return Result.Ok(distance <= 10.0); // 10km default radius
            }

            // Point-in-polygon algoritması
            foreach (var zone in deliveryZones)
            {
                var points = zone.Points
                    .OrderBy(p => p.Order)
                    .Select(p => new { p.Latitude, p.Longitude })
                    .ToList();

                if (IsPointInPolygon(userLatitude, userLongitude, points))
                {
                    return Result.Ok(true);
                }
            }

            return Result.Ok(false);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error checking delivery zone", ex, 
                new { merchantId, userLatitude, userLongitude });
            return Result.Fail<bool>("Error checking delivery zone", "GEO_LOCATION_ERROR");
        }
    }

    /// <summary>
    /// Kullanıcı konumuna göre teslimat süresini hesaplar
    /// </summary>
    public async Task<Result<DeliveryEstimateResponse>> CalculateDeliveryEstimateAsync(
        Guid merchantId, 
        double userLatitude, 
        double userLongitude,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var merchant = await _unitOfWork.ReadRepository<Merchant>()
                .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken: cancellationToken);

            if (merchant == null)
            {
                return Result.Fail<DeliveryEstimateResponse>("Merchant not found", ErrorCodes.MERCHANT_NOT_FOUND);
            }

            var distance = CalculateDistance(
                userLatitude, 
                userLongitude, 
                (double)merchant.Latitude, 
                (double)merchant.Longitude);

            var isInDeliveryZone = await IsInDeliveryZoneAsync(merchantId, userLatitude, userLongitude, cancellationToken);
            
            if (!isInDeliveryZone.Success)
            {
                return Result.Fail<DeliveryEstimateResponse>(isInDeliveryZone.Error ?? "Unknown error", isInDeliveryZone.ErrorCode);
            }

            if (!isInDeliveryZone.Value)
            {
                return Result.Fail<DeliveryEstimateResponse>(
                    "Location is outside delivery zone", 
                    "OUTSIDE_DELIVERY_ZONE");
            }

            // Delivery zone'a göre özel ayarları al
            var deliveryZones = await _unitOfWork.ReadRepository<DeliveryZone>()
                .ListAsync(
                    filter: dz => dz.MerchantId == merchantId && dz.IsActive,
                    include: "Points",
                    cancellationToken: cancellationToken);

            DeliveryZone? matchedZone = null;
            if (deliveryZones.Any())
            {
                foreach (var zone in deliveryZones)
                {
                    var points = zone.Points
                        .OrderBy(p => p.Order)
                        .Select(p => new { p.Latitude, p.Longitude })
                        .ToList();

                    if (IsPointInPolygon(userLatitude, userLongitude, points))
                    {
                        matchedZone = zone;
                        break;
                    }
                }
            }

            var deliveryFee = matchedZone?.DeliveryFee ?? merchant.DeliveryFee;
            var estimatedTime = matchedZone?.EstimatedDeliveryTime ?? merchant.AverageDeliveryTime;

            // Mesafeye göre teslimat süresini ayarla (her 1km için +2 dakika)
            var timeAdjustment = (int)(distance * 2);
            estimatedTime += timeAdjustment;

            var response = new DeliveryEstimateResponse(
                estimatedTime,
                deliveryFee,
                true,
                matchedZone?.Name,
                distance);

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error calculating delivery estimate", ex, 
                new { merchantId, userLatitude, userLongitude });
            return Result.Fail<DeliveryEstimateResponse>("Error calculating delivery estimate", "GEO_LOCATION_ERROR");
        }
    }

    /// <summary>
    /// Kullanıcı konumuna göre teslimat ücretini hesaplar
    /// </summary>
    public async Task<Result<decimal>> CalculateDeliveryFeeAsync(
        Guid merchantId, 
        double userLatitude, 
        double userLongitude,
        CancellationToken cancellationToken = default)
    {
        var estimate = await CalculateDeliveryEstimateAsync(merchantId, userLatitude, userLongitude, cancellationToken);
        
        if (!estimate.Success)
        {
            return Result.Fail<decimal>(estimate.Error ?? "Unknown error", estimate.ErrorCode);
        }

        return Result.Ok(estimate.Value?.DeliveryFee ?? 0);
    }

    /// <summary>
    /// Belirtilen alan içindeki tüm merchantları bulur
    /// </summary>
    public async Task<Result<IEnumerable<NearbyMerchantResponse>>> GetMerchantsInAreaAsync(
        IEnumerable<GeoPoint> areaPoints,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var merchants = await _unitOfWork.ReadRepository<Merchant>()
                .ListAsync(
                    filter: m => m.IsActive,
                    cancellationToken: cancellationToken);

            var merchantsInArea = new List<NearbyMerchantResponse>();
            var areaPointsList = areaPoints.ToList();
            var polygonPoints = areaPointsList.Select(p => new { p.Latitude, p.Longitude }).ToList();

            foreach (var merchant in merchants)
            {
                if (IsPointInPolygon((double)merchant.Latitude, (double)merchant.Longitude, polygonPoints))
                {
                    merchantsInArea.Add(new NearbyMerchantResponse(
                        merchant.Id,
                        merchant.Name,
                        merchant.Description,
                        merchant.Address,
                        0, // Polygon içinde olduğu için mesafe 0
                        merchant.DeliveryFee,
                        merchant.AverageDeliveryTime,
                        merchant.Rating,
                        merchant.TotalReviews,
                        merchant.IsOpen,
                        merchant.LogoUrl));
                }
            }

            return Result.Ok(merchantsInArea.AsEnumerable());
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting merchants in area", ex, new { AreaPointCount = areaPoints.Count() });
            return Result.Fail<IEnumerable<NearbyMerchantResponse>>("Error getting merchants in area", "GEO_LOCATION_ERROR");
        }
    }

    /// <summary>
    /// Konum tabanlı otomatik tamamlama önerileri
    /// </summary>
    public Task<Result<IEnumerable<LocationSuggestionResponse>>> GetLocationSuggestionsAsync(
        string query, 
        double? latitude = null, 
        double? longitude = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Bu basit bir implementasyon - gerçek uygulamada Google Places API kullanılır
            // var suggestions = new List<LocationSuggestionResponse>();

            // Örnek adresler (gerçek uygulamada database'den gelecek)
            var sampleAddresses = new[]
            {
                new LocationSuggestionResponse("Beşiktaş, İstanbul", 41.0426, 29.0077, "Beşiktaş", "İstanbul", null),
                new LocationSuggestionResponse("Kadıköy, İstanbul", 40.9898, 29.0257, "Kadıköy", "İstanbul", null),
                new LocationSuggestionResponse("Şişli, İstanbul", 41.0606, 28.9877, "Şişli", "İstanbul", null),
                new LocationSuggestionResponse("Beyoğlu, İstanbul", 41.0370, 28.9853, "Beyoğlu", "İstanbul", null),
                new LocationSuggestionResponse("Üsküdar, İstanbul", 41.0214, 29.0158, "Üsküdar", "İstanbul", null)
            };

            var filteredSuggestions = sampleAddresses
                .Where(addr => addr.Address.ToLower().Contains(query.ToLower()))
                .Take(5)
                .ToList();

            // Eğer kullanıcı konumu varsa mesafeyi hesapla
            if (latitude.HasValue && longitude.HasValue)
            {
                filteredSuggestions = filteredSuggestions
                    .Select(addr => addr with { 
                        DistanceKm = CalculateDistance(latitude.Value, longitude.Value, addr.Latitude, addr.Longitude)
                    })
                    .OrderBy(addr => addr.DistanceKm)
                    .ToList();
            }

            return Task.FromResult(Result.Ok<IEnumerable<LocationSuggestionResponse>>(filteredSuggestions));
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting location suggestions", ex, new { query, latitude, longitude });
            return Task.FromResult(Result.Fail<IEnumerable<LocationSuggestionResponse>>("Error getting location suggestions", "GEO_LOCATION_ERROR"));
        }
    }

    #region Helper Methods

    /// <summary>
    /// Dereceyi radyana çevirir
    /// </summary>
    private static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    /// <summary>
    /// Point-in-polygon algoritması (Ray casting algorithm)
    /// </summary>
    private static bool IsPointInPolygon(double pointLat, double pointLon, IEnumerable<dynamic> polygon)
    {
        var polygonPoints = polygon.ToList();
        if (polygonPoints.Count < 3) return false;

        bool inside = false;
        int j = polygonPoints.Count - 1;

        for (int i = 0; i < polygonPoints.Count; i++)
        {
            var pi = polygonPoints[i];
            var pj = polygonPoints[j];

            if (((pi.Latitude > pointLat) != (pj.Latitude > pointLat)) &&
                (pointLon < (pj.Longitude - pi.Longitude) * (pointLat - pi.Latitude) / (pj.Latitude - pi.Latitude) + pi.Longitude))
            {
                inside = !inside;
            }
            j = i;
        }

        return inside;
    }

    #endregion

    #region Additional GeoLocationController Methods

    public async Task<Result> SaveUserLocationAsync(
        SaveUserLocationRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await SaveUserLocationInternalAsync(request, userId, cancellationToken),
            "SaveUserLocation",
            new { userId, request.Latitude, request.Longitude },
            cancellationToken);
    }

    private async Task<Result> SaveUserLocationInternalAsync(
        SaveUserLocationRequest request,
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Simplified user location saving
            // In a real implementation, you would save to UserLocation entity
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving user location for user {UserId}", userId);
            return Result.Fail("Error saving user location");
        }
    }

    public async Task<Result<PagedResult<UserLocationResponse>>> GetUserLocationHistoryAsync(
        Guid userId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetUserLocationHistoryInternalAsync(userId, query, cancellationToken),
            "GetUserLocationHistory",
            new { userId, query.Page, query.PageSize },
            cancellationToken);
    }

    private async Task<Result<PagedResult<UserLocationResponse>>> GetUserLocationHistoryInternalAsync(
        Guid userId,
        PaginationQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            // Simplified location history retrieval
            var response = new PagedResult<UserLocationResponse>
            {
                Items = new List<UserLocationResponse>(),
                Page = query.Page,
                PageSize = query.PageSize
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user location history for user {UserId}", userId);
            return Result.Fail<PagedResult<UserLocationResponse>>("Error getting user location history");
        }
    }

    public async Task<Result<IEnumerable<MerchantInAreaResponse>>> GetMerchantsInAreaAsync(
        GetMerchantsInAreaRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetMerchantsInAreaInternalAsync(request, cancellationToken),
            "GetMerchantsInArea",
            new { request.LatitudeMin, request.LongitudeMin, request.LatitudeMax, request.LongitudeMax },
            cancellationToken);
    }

    private async Task<Result<IEnumerable<MerchantInAreaResponse>>> GetMerchantsInAreaInternalAsync(
        GetMerchantsInAreaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Simplified merchants in area retrieval
            var response = new List<MerchantInAreaResponse>();
            return Result.Ok<IEnumerable<MerchantInAreaResponse>>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchants in area");
            return Result.Fail<IEnumerable<MerchantInAreaResponse>>("Error getting merchants in area");
        }
    }

    public async Task<Result<LocationAnalyticsResponse>> GetLocationAnalyticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetLocationAnalyticsInternalAsync(startDate, endDate, cancellationToken),
            "GetLocationAnalytics",
            new { startDate, endDate },
            cancellationToken);
    }

    private async Task<Result<LocationAnalyticsResponse>> GetLocationAnalyticsInternalAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            // Simplified location analytics
            var response = new LocationAnalyticsResponse(
                TotalLocations: 0,
                UniqueUsers: 0,
                HeatmapData: new List<LocationHeatmapData>()
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location analytics");
            return Result.Fail<LocationAnalyticsResponse>("Error getting location analytics");
        }
    }

    public async Task<Result<DeliveryZoneCoverageResponse>> GetDeliveryZoneCoverageAsync(
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetDeliveryZoneCoverageInternalAsync(cancellationToken),
            "GetDeliveryZoneCoverage",
            new { },
            cancellationToken);
    }

    private async Task<Result<DeliveryZoneCoverageResponse>> GetDeliveryZoneCoverageInternalAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            // Simplified delivery zone coverage
            var response = new DeliveryZoneCoverageResponse(
                TotalZones: 0,
                Zones: new List<DeliveryZoneSummary>()
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delivery zone coverage");
            return Result.Fail<DeliveryZoneCoverageResponse>("Error getting delivery zone coverage");
        }
    }

    #endregion
}
