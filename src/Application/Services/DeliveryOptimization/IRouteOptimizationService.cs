using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.DeliveryOptimization;

/// <summary>
/// Rota optimizasyonu servisi
/// </summary>
public interface IRouteOptimizationService
{
    /// <summary>
    /// Alternatif rotalar önerir
    /// </summary>
    Task<Result<RouteOptimizationResponse>> GetAlternativeRoutesAsync(RouteOptimizationRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// En iyi rotayı seçer
    /// </summary>
    Task<Result<DeliveryRouteResponse>> SelectBestRouteAsync(RouteOptimizationRequest request, RoutePreferences? preferences = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Rota seçimini kaydeder
    /// </summary>
    Task<Result> SelectRouteAsync(RouteSelectionRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Rota durumunu günceller
    /// </summary>
    Task<Result> UpdateRouteStatusAsync(RouteUpdateRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Sipariş için rota oluşturur
    /// </summary>
    Task<Result<DeliveryRouteResponse>> CreateRouteForOrderAsync(Guid orderId, RouteOptimizationRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Rota geçmişini getirir
    /// </summary>
    Task<Result<List<DeliveryRouteResponse>>> GetRouteHistoryAsync(Guid orderId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Rota performansını analiz eder
    /// </summary>
    Task<Result<DeliveryPerformanceResponse>> AnalyzeRoutePerformanceAsync(DeliveryPerformanceRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gerçek zamanlı rota güncellemesi
    /// </summary>
    Task<Result> UpdateRouteInRealTimeAsync(Guid routeId, double currentLatitude, double currentLongitude, CancellationToken cancellationToken = default);
    /// <summary>
    /// Trafik durumuna göre rota önerir
    /// </summary>
    Task<Result<RouteOptimizationResponse>> GetTrafficOptimizedRoutesAsync(RouteOptimizationRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Çoklu nokta rota optimizasyonu
    /// </summary>
    Task<Result<RouteOptimizationResponse>> OptimizeMultiPointRouteAsync(List<RouteWaypoint> waypoints, RoutePreferences? preferences = null, CancellationToken cancellationToken = default);
    // SignalR Hub methods
    /// <summary>
    /// Calculate ETA for order
    /// </summary>
    Task<Result<int>> CalculateETAAsync(Guid orderId, double latitude, double longitude, CancellationToken cancellationToken = default);
    /// <summary>
    /// Update ETA
    /// </summary>
    Task<Result> UpdateETAAsync(UpdateETARequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Get optimized route for courier's assigned orders
    /// </summary>
    Task<Result<RouteOptimizationResponse>> GetOptimizedRouteForCourierAsync(Guid courierId, CancellationToken cancellationToken = default);
}
