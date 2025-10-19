using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Getir.Application.Services.DeliveryOptimization;

/// <summary>
/// Rota optimizasyonu servisi implementasyonu
/// </summary>
public class RouteOptimizationService : BaseService, IRouteOptimizationService
{
    public RouteOptimizationService(IUnitOfWork unitOfWork, ILogger<RouteOptimizationService> logger, ILoggingService loggingService, ICacheService cacheService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
    }
    public async Task<Result<RouteOptimizationResponse>> GetAlternativeRoutesAsync(RouteOptimizationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Gerçek uygulamada Google Maps Directions API kullanılacak
            // Şimdilik mock data ile alternatif rotalar oluşturuyoruz

            var routes = new List<DeliveryRouteResponse>();

            // Ana rota
            var primaryRoute = CreateMockRoute(
                "Primary Route",
                "Primary",
                request.StartLatitude,
                request.StartLongitude,
                request.EndLatitude,
                request.EndLongitude,
                request.Waypoints,
                isSelected: true);

            routes.Add(primaryRoute);

            // Alternatif rotalar
            if (request.Preferences?.MaxAlternatives > 1)
            {
                var alternative1 = CreateMockRoute(
                    "Alternative Route 1",
                    "Alternative",
                    request.StartLatitude,
                    request.StartLongitude,
                    request.EndLatitude,
                    request.EndLongitude,
                    request.Waypoints,
                    distanceMultiplier: 1.1,
                    durationMultiplier: 1.2,
                    hasTollRoads: false);

                routes.Add(alternative1);

                if (request.Preferences.MaxAlternatives > 2)
                {
                    var alternative2 = CreateMockRoute(
                        "Fastest Route",
                        "Fastest",
                        request.StartLatitude,
                        request.StartLongitude,
                        request.EndLatitude,
                        request.EndLongitude,
                        request.Waypoints,
                        distanceMultiplier: 1.3,
                        durationMultiplier: 0.9,
                        hasTollRoads: true,
                        isHighwayPreferred: true);

                    routes.Add(alternative2);
                }
            }

            var response = new RouteOptimizationResponse(
                Routes: routes,
                Status: "OK",
                ErrorMessage: null);

            _loggingService.LogBusinessEvent("AlternativeRoutesGenerated", new
            {
                RouteCount = routes.Count,
                StartLat = request.StartLatitude,
                StartLon = request.StartLongitude,
                EndLat = request.EndLatitude,
                EndLon = request.EndLongitude
            });

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating alternative routes");
            _loggingService.LogError("Generate alternative routes failed", ex);
            return Result.Fail<RouteOptimizationResponse>("Failed to generate alternative routes", "ROUTE_GENERATION_ERROR");
        }
    }
    public async Task<Result<DeliveryRouteResponse>> SelectBestRouteAsync(RouteOptimizationRequest request, RoutePreferences? preferences = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var routesResult = await GetAlternativeRoutesAsync(request, cancellationToken);
            if (!routesResult.Success)
            {
                return Result.Fail<DeliveryRouteResponse>(routesResult.Error ?? "Failed to get routes", routesResult.ErrorCode);
            }

            var routes = routesResult.Value.Routes;
            if (!routes.Any())
            {
                return Result.Fail<DeliveryRouteResponse>("No routes available", "NO_ROUTES_AVAILABLE");
            }

            // En iyi rotayı seç (basit algoritma)
            DeliveryRouteResponse bestRoute;

            if (preferences?.AvoidTollRoads == true)
            {
                bestRoute = routes.Where(r => !r.HasTollRoads).OrderBy(r => r.EstimatedDurationMinutes).FirstOrDefault()
                           ?? routes.First();
            }
            else if (preferences?.TravelMode == "WALKING")
            {
                bestRoute = routes.OrderBy(r => r.TotalDistanceKm).First();
            }
            else
            {
                // Genel olarak en hızlı rotayı seç
                bestRoute = routes.OrderBy(r => r.EstimatedDurationMinutes).First();
            }

            _loggingService.LogBusinessEvent("BestRouteSelected", new
            {
                RouteName = bestRoute.RouteName,
                RouteType = bestRoute.RouteType,
                Distance = bestRoute.TotalDistanceKm,
                Duration = bestRoute.EstimatedDurationMinutes,
                Score = bestRoute.RouteScore
            });

            return Result.Ok(bestRoute);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting best route");
            _loggingService.LogError("Select best route failed", ex);
            return Result.Fail<DeliveryRouteResponse>("Failed to select best route", "SELECT_BEST_ROUTE_ERROR");
        }
    }
    public async Task<Result> SelectRouteAsync(RouteSelectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = await _unitOfWork.ReadRepository<DeliveryRoute>()
                .FirstOrDefaultAsync(
                    filter: r => r.Id == request.RouteId && r.OrderId == request.OrderId,
                    cancellationToken: cancellationToken);

            if (route == null)
            {
                return Result.Fail("Route not found", "ROUTE_NOT_FOUND");
            }

            // Diğer rotaları seçili olmaktan çıkar
            var otherRoutes = await _unitOfWork.ReadRepository<DeliveryRoute>()
                .ListAsync(
                    filter: r => r.OrderId == request.OrderId && r.Id != request.RouteId,
                    cancellationToken: cancellationToken);

            foreach (var otherRoute in otherRoutes)
            {
                otherRoute.IsSelected = false;
                otherRoute.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<DeliveryRoute>().Update(otherRoute);
            }

            // Seçilen rotayı işaretle
            route.IsSelected = true;
            route.Notes = request.Notes;
            route.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<DeliveryRoute>().Update(route);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("RouteSelected", new
            {
                request.OrderId,
                request.RouteId,
                RouteName = route.RouteName,
                request.Notes
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting route {RouteId}", request.RouteId);
            _loggingService.LogError("Select route failed", ex, new { request.RouteId });
            return Result.Fail("Failed to select route", "SELECT_ROUTE_ERROR");
        }
    }
    public async Task<Result> UpdateRouteStatusAsync(RouteUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = await _unitOfWork.ReadRepository<DeliveryRoute>()
                .FirstOrDefaultAsync(
                    filter: r => r.Id == request.RouteId,
                    cancellationToken: cancellationToken);

            if (route == null)
            {
                return Result.Fail("Route not found", "ROUTE_NOT_FOUND");
            }

            // Durum güncelleme
            switch (request.Status?.ToLower())
            {
                case "started":
                    route.StartedAt = DateTime.UtcNow;
                    break;
                case "completed":
                    route.IsCompleted = true;
                    route.CompletedAt = DateTime.UtcNow;
                    break;
                case "cancelled":
                    route.IsCompleted = false;
                    route.CompletedAt = null;
                    break;
            }

            if (request.ActualDurationMinutes.HasValue)
            {
                // Gerçek süre kaydedilebilir (şimdilik notes'a ekliyoruz)
                route.Notes = $"{route.Notes}\nActual Duration: {request.ActualDurationMinutes} minutes".Trim();
            }

            if (!string.IsNullOrEmpty(request.Notes))
            {
                route.Notes = $"{route.Notes}\n{request.Notes}".Trim();
            }

            route.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<DeliveryRoute>().Update(route);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("RouteStatusUpdated", new
            {
                request.RouteId,
                request.Status,
                request.ActualDurationMinutes,
                StartedAt = route.StartedAt,
                CompletedAt = route.CompletedAt
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating route status {RouteId}", request.RouteId);
            _loggingService.LogError("Update route status failed", ex, new { request.RouteId });
            return Result.Fail("Failed to update route status", "UPDATE_ROUTE_STATUS_ERROR");
        }
    }
    public async Task<Result<DeliveryRouteResponse>> CreateRouteForOrderAsync(Guid orderId, RouteOptimizationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // En iyi rotayı seç
            var bestRouteResult = await SelectBestRouteAsync(request, cancellationToken: cancellationToken);
            if (!bestRouteResult.Success)
            {
                return Result.Fail<DeliveryRouteResponse>(bestRouteResult.Error ?? "Failed to select best route", bestRouteResult.ErrorCode);
            }

            var bestRoute = bestRouteResult.Value;

            // Veritabanına kaydet
            var route = new DeliveryRoute
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                RouteName = bestRoute.RouteName,
                RouteType = bestRoute.RouteType,
                Waypoints = JsonSerializer.Serialize(bestRoute.Waypoints),
                Polyline = bestRoute.Polyline,
                TotalDistanceKm = bestRoute.TotalDistanceKm,
                EstimatedDurationMinutes = bestRoute.EstimatedDurationMinutes,
                EstimatedTrafficDelayMinutes = bestRoute.EstimatedTrafficDelayMinutes,
                EstimatedFuelCost = bestRoute.EstimatedFuelCost,
                RouteScore = bestRoute.RouteScore,
                HasTollRoads = bestRoute.HasTollRoads,
                HasHighTrafficAreas = bestRoute.HasHighTrafficAreas,
                IsHighwayPreferred = bestRoute.IsHighwayPreferred,
                IsSelected = true,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<DeliveryRoute>().AddAsync(route, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new DeliveryRouteResponse(
                route.Id,
                route.RouteName,
                route.RouteType,
                bestRoute.Waypoints,
                route.Polyline,
                route.TotalDistanceKm,
                route.EstimatedDurationMinutes,
                route.EstimatedTrafficDelayMinutes,
                route.EstimatedFuelCost,
                route.RouteScore,
                route.HasTollRoads,
                route.HasHighTrafficAreas,
                route.IsHighwayPreferred,
                route.IsSelected,
                route.IsCompleted,
                route.StartedAt,
                route.CompletedAt,
                route.Notes,
                route.CreatedAt);

            _loggingService.LogBusinessEvent("RouteCreatedForOrder", new
            {
                orderId,
                route.Id,
                route.RouteName,
                route.TotalDistanceKm,
                route.EstimatedDurationMinutes
            });

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating route for order {OrderId}", orderId);
            _loggingService.LogError("Create route for order failed", ex, new { orderId });
            return Result.Fail<DeliveryRouteResponse>("Failed to create route for order", "CREATE_ROUTE_ERROR");
        }
    }
    public async Task<Result<List<DeliveryRouteResponse>>> GetRouteHistoryAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var routes = await _unitOfWork.ReadRepository<DeliveryRoute>()
                .ListAsync(
                    filter: r => r.OrderId == orderId,
                    orderBy: r => r.CreatedAt,
                    ascending: false,
                    cancellationToken: cancellationToken);

            var responses = routes.Select(r => new DeliveryRouteResponse(
                r.Id,
                r.RouteName,
                r.RouteType,
                JsonSerializer.Deserialize<List<RouteWaypoint>>(r.Waypoints) ?? new List<RouteWaypoint>(),
                r.Polyline,
                r.TotalDistanceKm,
                r.EstimatedDurationMinutes,
                r.EstimatedTrafficDelayMinutes,
                r.EstimatedFuelCost,
                r.RouteScore,
                r.HasTollRoads,
                r.HasHighTrafficAreas,
                r.IsHighwayPreferred,
                r.IsSelected,
                r.IsCompleted,
                r.StartedAt,
                r.CompletedAt,
                r.Notes,
                r.CreatedAt)).ToList();

            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting route history for order {OrderId}", orderId);
            _loggingService.LogError("Get route history failed", ex, new { orderId });
            return Result.Fail<List<DeliveryRouteResponse>>("Failed to get route history", "GET_ROUTE_HISTORY_ERROR");
        }
    }
    public async Task<Result<DeliveryPerformanceResponse>> AnalyzeRoutePerformanceAsync(DeliveryPerformanceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromDate = request.FromDate ?? DateTime.UtcNow.AddDays(-30);
            var toDate = request.ToDate ?? DateTime.UtcNow;

            // Siparişleri getir
            var orders = await _unitOfWork.ReadRepository<Order>()
                .ListAsync(
                    filter: o => o.MerchantId == request.MerchantId &&
                                o.CreatedAt >= fromDate && o.CreatedAt <= toDate,
                    cancellationToken: cancellationToken);

            // Rota performansını analiz et
            var totalDeliveries = orders.Count;
            var successfulDeliveries = orders.Count(o => o.Status == Domain.Enums.OrderStatus.Delivered);
            var failedDeliveries = orders.Count(o => o.Status == Domain.Enums.OrderStatus.Cancelled);

            var averageDeliveryTime = orders.Where(o => o.Status == Domain.Enums.OrderStatus.Delivered)
                .Average(o => (o.UpdatedAt - o.CreatedAt)?.TotalMinutes ?? 0);

            var averageDeliveryFee = orders.Where(o => o.DeliveryFee > 0)
                .Average(o => o.DeliveryFee);

            var totalRevenue = orders.Where(o => o.Status == Domain.Enums.OrderStatus.Delivered)
                .Sum(o => o.DeliveryFee);

            var successRate = totalDeliveries > 0 ? (decimal)successfulDeliveries / totalDeliveries * 100 : 0;

            // Basit zaman dilimi performansı
            var timeSlotPerformance = new List<DeliveryTimeSlotPerformance>
            {
                new("06:00-12:00",
                    orders.Count(o => o.CreatedAt.TimeOfDay >= TimeSpan.FromHours(6) && o.CreatedAt.TimeOfDay < TimeSpan.FromHours(12)),
                    (decimal)orders.Where(o => o.CreatedAt.TimeOfDay >= TimeSpan.FromHours(6) && o.CreatedAt.TimeOfDay < TimeSpan.FromHours(12))
                        .Average(o => (o.UpdatedAt - o.CreatedAt)?.TotalMinutes ?? 0),
                    (decimal)orders.Where(o => o.CreatedAt.TimeOfDay >= TimeSpan.FromHours(6) && o.CreatedAt.TimeOfDay < TimeSpan.FromHours(12))
                        .Count(o => o.Status == Domain.Enums.OrderStatus.Delivered) / Math.Max(1, orders.Count(o => o.CreatedAt.TimeOfDay >= TimeSpan.FromHours(6) && o.CreatedAt.TimeOfDay < TimeSpan.FromHours(12))) * 100),

                new("12:00-18:00",
                    orders.Count(o => o.CreatedAt.TimeOfDay >= TimeSpan.FromHours(12) && o.CreatedAt.TimeOfDay < TimeSpan.FromHours(18)),
                    (decimal)orders.Where(o => o.CreatedAt.TimeOfDay >= TimeSpan.FromHours(12) && o.CreatedAt.TimeOfDay < TimeSpan.FromHours(18))
                        .Average(o => (o.UpdatedAt - o.CreatedAt)?.TotalMinutes ?? 0),
                    (decimal)orders.Where(o => o.CreatedAt.TimeOfDay >= TimeSpan.FromHours(12) && o.CreatedAt.TimeOfDay < TimeSpan.FromHours(18))
                        .Count(o => o.Status == Domain.Enums.OrderStatus.Delivered) / Math.Max(1, orders.Count(o => o.CreatedAt.TimeOfDay >= TimeSpan.FromHours(12) && o.CreatedAt.TimeOfDay < TimeSpan.FromHours(18))) * 100),

                new("18:00-24:00",
                    orders.Count(o => o.CreatedAt.TimeOfDay >= TimeSpan.FromHours(18)),
                    (decimal)orders.Where(o => o.CreatedAt.TimeOfDay >= TimeSpan.FromHours(18))
                        .Average(o => (o.UpdatedAt - o.CreatedAt)?.TotalMinutes ?? 0),
                    (decimal)orders.Where(o => o.CreatedAt.TimeOfDay >= TimeSpan.FromHours(18))
                        .Count(o => o.Status == Domain.Enums.OrderStatus.Delivered) / Math.Max(1, orders.Count(o => o.CreatedAt.TimeOfDay >= TimeSpan.FromHours(18))) * 100)
            };

            // Basit mesafe performansı (mock data)
            var distancePerformance = new List<DeliveryDistancePerformance>
            {
                new("0-2km",
                    orders.Count / 4,
                    15.0m,
                    averageDeliveryFee,
                    95.0m),

                new("2-5km",
                    orders.Count / 4,
                    25.0m,
                    averageDeliveryFee * 1.2m,
                    90.0m),

                new("5-10km",
                    orders.Count / 4,
                    35.0m,
                    averageDeliveryFee * 1.5m,
                    85.0m),

                new("10km+",
                    orders.Count / 4,
                    45.0m,
                    averageDeliveryFee * 2.0m,
                    80.0m)
            };

            var response = new DeliveryPerformanceResponse(
                request.MerchantId,
                request.DeliveryZoneId,
                totalDeliveries,
                successfulDeliveries,
                failedDeliveries,
                (decimal)averageDeliveryTime,
                5.0m, // Mock average distance
                (decimal)averageDeliveryFee,
                totalRevenue,
                successRate,
                timeSlotPerformance,
                distancePerformance,
                DateTime.UtcNow);

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing route performance for merchant {MerchantId}", request.MerchantId);
            _loggingService.LogError("Analyze route performance failed", ex, new { request.MerchantId });
            return Result.Fail<DeliveryPerformanceResponse>("Failed to analyze route performance", "ANALYZE_PERFORMANCE_ERROR");
        }
    }
    public async Task<Result> UpdateRouteInRealTimeAsync(Guid routeId, double currentLatitude, double currentLongitude, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = await _unitOfWork.ReadRepository<DeliveryRoute>()
                .FirstOrDefaultAsync(
                    filter: r => r.Id == routeId,
                    cancellationToken: cancellationToken);

            if (route == null)
            {
                return Result.Fail("Route not found", "ROUTE_NOT_FOUND");
            }

            // Gerçek zamanlı güncelleme
            var currentLocation = new RouteWaypoint(currentLatitude, currentLongitude, "Current Location");
            var waypoints = JsonSerializer.Deserialize<List<RouteWaypoint>>(route.Waypoints) ?? new List<RouteWaypoint>();
            waypoints.Add(currentLocation);

            route.Waypoints = JsonSerializer.Serialize(waypoints);
            route.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<DeliveryRoute>().Update(route);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("RouteUpdatedRealTime", new
            {
                routeId,
                currentLatitude,
                currentLongitude,
                UpdatedAt = route.UpdatedAt
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating route in real time {RouteId}", routeId);
            _loggingService.LogError("Update route real time failed", ex, new { routeId });
            return Result.Fail("Failed to update route in real time", "UPDATE_ROUTE_REALTIME_ERROR");
        }
    }
    public Task<Result<RouteOptimizationResponse>> GetTrafficOptimizedRoutesAsync(RouteOptimizationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Trafik durumuna göre rota optimizasyonu
            // Gerçek uygulamada Google Maps Traffic API kullanılacak

            var routes = new List<DeliveryRouteResponse>();

            // Trafik durumuna göre farklı rotalar
            var lowTrafficRoute = CreateMockRoute(
                "Low Traffic Route",
                "TrafficOptimized",
                request.StartLatitude,
                request.StartLongitude,
                request.EndLatitude,
                request.EndLongitude,
                request.Waypoints,
                distanceMultiplier: 1.2,
                durationMultiplier: 0.8,
                trafficDelay: 5);

            var highTrafficRoute = CreateMockRoute(
                "High Traffic Route",
                "TrafficOptimized",
                request.StartLatitude,
                request.StartLongitude,
                request.EndLatitude,
                request.EndLongitude,
                request.Waypoints,
                distanceMultiplier: 0.9,
                durationMultiplier: 1.5,
                trafficDelay: 20);

            routes.Add(lowTrafficRoute);
            routes.Add(highTrafficRoute);

            var response = new RouteOptimizationResponse(
                Routes: routes,
                Status: "OK",
                ErrorMessage: null);

            return Task.FromResult(Result.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting traffic optimized routes");
            _loggingService.LogError("Get traffic optimized routes failed", ex);
            return Task.FromResult(Result.Fail<RouteOptimizationResponse>("Failed to get traffic optimized routes", "TRAFFIC_ROUTES_ERROR"));
        }
    }
    public Task<Result<RouteOptimizationResponse>> OptimizeMultiPointRouteAsync(List<RouteWaypoint> waypoints, RoutePreferences? preferences = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (waypoints.Count < 2)
            {
                return Task.FromResult(Result.Fail<RouteOptimizationResponse>("At least 2 waypoints required", "INSUFFICIENT_WAYPOINTS"));
            }

            // Çoklu nokta rota optimizasyonu
            // Gerçek uygulamada Traveling Salesman Problem (TSP) algoritması kullanılacak

            var optimizedWaypoints = OptimizeWaypointOrder(waypoints);

            var route = CreateMockRoute(
                "Multi-Point Optimized Route",
                "MultiPoint",
                optimizedWaypoints.First().Latitude,
                optimizedWaypoints.First().Longitude,
                optimizedWaypoints.Last().Latitude,
                optimizedWaypoints.Last().Longitude,
                optimizedWaypoints.Skip(1).Take(optimizedWaypoints.Count - 2).ToList(),
                distanceMultiplier: 1.0,
                durationMultiplier: 1.0);

            var response = new RouteOptimizationResponse(
                Routes: new List<DeliveryRouteResponse> { route },
                Status: "OK",
                ErrorMessage: null);

            return Task.FromResult(Result.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing multi-point route");
            _loggingService.LogError("Optimize multi-point route failed", ex);
            return Task.FromResult(Result.Fail<RouteOptimizationResponse>("Failed to optimize multi-point route", "MULTI_POINT_OPTIMIZATION_ERROR"));
        }
    }

    #region Helper Methods
    private DeliveryRouteResponse CreateMockRoute(string routeName, string routeType, double startLat, double startLon,
        double endLat, double endLon, List<RouteWaypoint>? waypoints = null, double distanceMultiplier = 1.0, double durationMultiplier = 1.0,
        int trafficDelay = 0, bool hasTollRoads = false, bool isHighwayPreferred = false, bool isSelected = false)
    {
        // Basit mesafe hesaplama (Haversine formula)
        var distance = CalculateDistance(startLat, startLon, endLat, endLon) * distanceMultiplier;
        var duration = (int)(distance * 2 * durationMultiplier) + trafficDelay; // Her km için 2 dakika + trafik gecikmesi

        var routeWaypoints = waypoints ?? new List<RouteWaypoint>();
        routeWaypoints.Insert(0, new RouteWaypoint(startLat, startLon, "Start"));
        routeWaypoints.Add(new RouteWaypoint(endLat, endLon, "End"));

        return new DeliveryRouteResponse(
            Guid.NewGuid(),
            routeName,
            routeType,
            routeWaypoints,
            "mock_polyline_string", // Gerçek uygulamada Google Maps polyline
            Math.Round(distance, 2),
            duration,
            trafficDelay,
            (decimal)(distance * 0.5), // Yakıt maliyeti (km başına 0.5 TL)
            CalculateRouteScore(distance, duration, hasTollRoads, isHighwayPreferred),
            hasTollRoads,
            trafficDelay > 10,
            isHighwayPreferred,
            isSelected,
            false,
            null,
            null,
            null,
            DateTime.UtcNow);
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadiusKm = 6371.0;

        var lat1Rad = DegreesToRadians(lat1);
        var lon1Rad = DegreesToRadians(lon1);
        var lat2Rad = DegreesToRadians(lat2);
        var lon2Rad = DegreesToRadians(lon2);

        var deltaLat = lat2Rad - lat1Rad;
        var deltaLon = lon2Rad - lon1Rad;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }
    private double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    private decimal CalculateRouteScore(double distance, int duration, bool hasTollRoads, bool isHighwayPreferred)
    {
        var score = 100m;

        // Mesafe faktörü (daha kısa mesafe = daha yüksek skor)
        if (distance > 10) score -= 20;
        else if (distance > 5) score -= 10;

        // Süre faktörü (daha kısa süre = daha yüksek skor)
        if (duration > 60) score -= 20;
        else if (duration > 30) score -= 10;

        // Ücretli yol faktörü
        if (hasTollRoads) score -= 15;

        // Otoyol tercihi
        if (isHighwayPreferred) score += 5;

        return Math.Max(0, Math.Min(100, score));
    }
    private List<RouteWaypoint> OptimizeWaypointOrder(List<RouteWaypoint> waypoints)
    {
        // Basit TSP implementasyonu (Nearest Neighbor)
        if (waypoints.Count <= 2) return waypoints;

        var optimized = new List<RouteWaypoint>();
        var remaining = new List<RouteWaypoint>(waypoints);

        // İlk noktayı seç
        var current = remaining.First();
        optimized.Add(current);
        remaining.Remove(current);

        // En yakın noktaları sırayla ekle
        while (remaining.Any())
        {
            var nearest = remaining
                .OrderBy(w => CalculateDistance(current.Latitude, current.Longitude, w.Latitude, w.Longitude))
                .First();

            optimized.Add(nearest);
            remaining.Remove(nearest);
            current = nearest;
        }

        return optimized;
    }
    // SignalR Hub-specific methods
    public async Task<Result<int>> CalculateETAAsync(Guid orderId, double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken: cancellationToken);

        if (order == null)
        {
            return Result.Fail<int>("Order not found", ErrorCodes.ORDER_NOT_FOUND);
        }

        // Calculate distance from current location to delivery address
        var distance = CalculateDistance(
            latitude,
            longitude,
            (double)order.DeliveryLatitude,
            (double)order.DeliveryLongitude);

        // Calculate ETA based on distance (assume 30 km/h average speed)
        var estimatedMinutes = (int)(distance / 30.0 * 60.0);

        // Add traffic buffer (20%)
        estimatedMinutes = (int)(estimatedMinutes * 1.2);

        _loggingService.LogBusinessEvent("ETACalculated", new
        {
            orderId,
            distance,
            estimatedMinutes
        });

        return Result.Ok(estimatedMinutes);
    }
    public async Task<Result> UpdateETAAsync(UpdateETARequest request, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Repository<Order>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, null, cancellationToken);

        if (order == null)
        {
            return Result.Fail("Order not found", ErrorCodes.ORDER_NOT_FOUND);
        }

        // Update estimated delivery time
        order.EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(request.EstimatedMinutes);
        order.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _loggingService.LogBusinessEvent("ETAUpdated", new
        {
            orderId = request.OrderId,
            courierId = request.CourierId,
            estimatedMinutes = request.EstimatedMinutes
        });

        return Result.Ok();
    }
    public async Task<Result<RouteOptimizationResponse>> GetOptimizedRouteForCourierAsync(Guid courierId, CancellationToken cancellationToken = default)
    {
        // Get courier's assigned orders
        var orders = await _unitOfWork.Repository<Order>()
            .GetPagedAsync(
                filter: o => o.CourierId == courierId &&
                            (o.Status == OrderStatus.Ready ||
                             o.Status == OrderStatus.PickedUp ||
                             o.Status == OrderStatus.OnTheWay),
                orderBy: o => o.CreatedAt,
                ascending: true,
                page: 1,
                pageSize: 20,
                cancellationToken: cancellationToken);

        if (!orders.Any())
        {
            return Result.Ok(new RouteOptimizationResponse(
                new List<DeliveryRouteResponse>(),
                "NO_ORDERS",
                "No active orders assigned"));
        }

        // Get courier's current location (latest)
        var latestLocation = await _unitOfWork.Repository<CourierLocation>()
            .GetPagedAsync(
                filter: l => l.CourierId == courierId,
                orderBy: l => l.Timestamp,
                ascending: false,
                page: 1,
                pageSize: 1,
                cancellationToken: cancellationToken);

        var currentLocation = latestLocation.FirstOrDefault();

        if (currentLocation == null)
        {
            return Result.Fail<RouteOptimizationResponse>(
                "Courier location not found",
                "LOCATION_NOT_FOUND");
        }

        // Create waypoints from order delivery addresses
        var waypoints = orders.Select(o => new RouteWaypoint(
            (double)o.DeliveryLatitude,
            (double)o.DeliveryLongitude,
            o.DeliveryAddress,
            true
        )).ToList();

        // Optimize route order (simple nearest neighbor algorithm)
        var optimizedWaypoints = OptimizeWaypointOrder(waypoints);

        // Calculate total distance and duration
        double totalDistance = 0;
        int totalDuration = 0;

        for (int i = 0; i < optimizedWaypoints.Count; i++)
        {
            var start = i == 0
                ? new { Lat = currentLocation.Latitude, Lon = currentLocation.Longitude }
                : new { Lat = optimizedWaypoints[i - 1].Latitude, Lon = optimizedWaypoints[i - 1].Longitude };

            var end = optimizedWaypoints[i];

            var distance = CalculateDistance(start.Lat, start.Lon, end.Latitude, end.Longitude);
            totalDistance += distance;
            totalDuration += (int)(distance / 30.0 * 60.0); // 30 km/h average speed
        }

        // Create optimized route response
        var route = new DeliveryRouteResponse(
            Id: Guid.NewGuid(),
            RouteName: $"Optimized Route - {orders.Count} deliveries",
            RouteType: "OPTIMIZED",
            Waypoints: optimizedWaypoints,
            Polyline: "encoded_polyline_here", // Would be actual polyline in production
            TotalDistanceKm: totalDistance,
            EstimatedDurationMinutes: totalDuration,
            EstimatedTrafficDelayMinutes: (int)(totalDuration * 0.2), // 20% traffic buffer
            EstimatedFuelCost: (decimal)(totalDistance * 1.5), // Rough estimate
            RouteScore: 0.95m,
            HasTollRoads: false,
            HasHighTrafficAreas: false,
            IsHighwayPreferred: false,
            IsSelected: true,
            IsCompleted: false,
            StartedAt: null,
            CompletedAt: null,
            Notes: $"{orders.Count} deliveries optimized for shortest distance",
            CreatedAt: DateTime.UtcNow
        );

        var response = new RouteOptimizationResponse(
            new List<DeliveryRouteResponse> { route },
            "SUCCESS",
            null);

        _loggingService.LogBusinessEvent("OptimizedRouteGenerated", new
        {
            courierId,
            orderCount = orders.Count,
            totalDistance,
            totalDuration
        });

        return Result.Ok(response);
    }
    #endregion
}
