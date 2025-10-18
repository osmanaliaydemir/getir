namespace Getir.Application.DTO;

/// <summary>
/// Teslimat kapasitesi yönetimi DTO'ları
/// </summary>
public record DeliveryCapacityRequest(
    Guid MerchantId,
    Guid? DeliveryZoneId = null,
    int MaxConcurrentDeliveries = 10,
    int MaxDailyDeliveries = 100,
    int MaxWeeklyDeliveries = 500,
    TimeSpan? PeakStartTime = null,
    TimeSpan? PeakEndTime = null,
    int PeakHourCapacityReduction = 0,
    double? MaxDeliveryDistanceKm = null,
    decimal? DistanceBasedFeeMultiplier = 1.0m,
    bool IsDynamicCapacityEnabled = true);

public record DeliveryCapacityResponse(
    Guid Id,
    Guid MerchantId,
    Guid? DeliveryZoneId,
    int MaxConcurrentDeliveries,
    int MaxDailyDeliveries,
    int MaxWeeklyDeliveries,
    TimeSpan? PeakStartTime,
    TimeSpan? PeakEndTime,
    int PeakHourCapacityReduction,
    double? MaxDeliveryDistanceKm,
    decimal? DistanceBasedFeeMultiplier,
    bool IsDynamicCapacityEnabled,
    int CurrentActiveDeliveries,
    int TodayDeliveryCount,
    int ThisWeekDeliveryCount,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record DeliveryCapacityCheckRequest(
    Guid MerchantId,
    Guid? DeliveryZoneId = null,
    double? DeliveryDistanceKm = null,
    DateTime? RequestedDeliveryTime = null);

public record DeliveryCapacityCheckResponse(
    bool CanAcceptDelivery,
    string? Reason,
    int AvailableCapacity,
    int CurrentLoad,
    decimal? AdjustedDeliveryFee,
    int? EstimatedWaitTimeMinutes,
    DateTime? NextAvailableSlot);

/// <summary>
/// Rota optimizasyonu DTO'ları
/// </summary>
public record RouteOptimizationRequest(
    double StartLatitude,
    double StartLongitude,
    double EndLatitude,
    double EndLongitude,
    List<RouteWaypoint>? Waypoints = null,
    RoutePreferences? Preferences = null);

public record RouteWaypoint(
    double Latitude,
    double Longitude,
    string? Name = null,
    bool IsRequired = true);

public record RoutePreferences(
    bool AvoidTollRoads = false,
    bool AvoidHighways = false,
    bool AvoidFerries = false,
    string TravelMode = "DRIVING", // DRIVING, WALKING, BICYCLING, TRANSIT
    string TrafficModel = "BEST_GUESS", // BEST_GUESS, PESSIMISTIC, OPTIMISTIC
    int MaxAlternatives = 3);

public record RouteOptimizationResponse(
    List<DeliveryRouteResponse> Routes,
    string Status,
    string? ErrorMessage = null);

public record DeliveryRouteResponse(
    Guid Id,
    string RouteName,
    string RouteType,
    List<RouteWaypoint> Waypoints,
    string Polyline,
    double TotalDistanceKm,
    int EstimatedDurationMinutes,
    int EstimatedTrafficDelayMinutes,
    decimal EstimatedFuelCost,
    decimal RouteScore,
    bool HasTollRoads,
    bool HasHighTrafficAreas,
    bool IsHighwayPreferred,
    bool IsSelected,
    bool IsCompleted,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? Notes,
    DateTime CreatedAt);

public record RouteSelectionRequest(
    Guid OrderId,
    Guid RouteId,
    string? Notes = null);

public record RouteUpdateRequest(
    Guid RouteId,
    string? Status = null, // "started", "completed", "cancelled"
    List<RouteWaypoint>? CurrentLocation = null,
    int? ActualDurationMinutes = null,
    string? Notes = null);

/// <summary>
/// Teslimat performans analizi DTO'ları
/// </summary>
public record DeliveryPerformanceRequest(
    Guid MerchantId,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? DeliveryZoneId = null);

public record DeliveryPerformanceResponse(
    Guid MerchantId,
    Guid? DeliveryZoneId,
    int TotalDeliveries,
    int SuccessfulDeliveries,
    int FailedDeliveries,
    decimal AverageDeliveryTimeMinutes,
    decimal AverageDistanceKm,
    decimal AverageDeliveryFee,
    decimal TotalRevenue,
    decimal SuccessRate,
    List<DeliveryTimeSlotPerformance> TimeSlotPerformance,
    List<DeliveryDistancePerformance> DistancePerformance,
    DateTime GeneratedAt);

public record DeliveryTimeSlotPerformance(
    string TimeSlot, // "06:00-12:00", "12:00-18:00", "18:00-24:00"
    int DeliveryCount,
    decimal AverageTimeMinutes,
    decimal SuccessRate);

public record DeliveryDistancePerformance(
    string DistanceRange, // "0-2km", "2-5km", "5-10km", "10km+"
    int DeliveryCount,
    decimal AverageTimeMinutes,
    decimal AverageFee,
    decimal SuccessRate);

/// <summary>
/// Dinamik kapasite ayarları
/// </summary>
public record DynamicCapacityAdjustmentRequest(
    Guid MerchantId,
    Guid? DeliveryZoneId = null,
    int CapacityAdjustment = 0, // Pozitif: artır, Negatif: azalt
    string Reason = "Manual adjustment",
    DateTime? ValidUntil = null);

public record CapacityAlertRequest(
    Guid MerchantId,
    string AlertType, // "HIGH_LOAD", "CAPACITY_EXCEEDED", "PEAK_HOUR_WARNING"
    string Message,
    int CurrentLoad,
    int MaxCapacity,
    DateTime AlertTime);

/// <summary>
/// Update ETA (Estimated Time of Arrival) request
/// </summary>
public record UpdateETARequest(
    Guid OrderId,
    Guid CourierId,
    int EstimatedMinutes,
    DateTime Timestamp,
    string? Reason = null);

/// <summary>
/// Calculate ETA request
/// </summary>
public record CalculateETARequest(
    Guid OrderId,
    double CurrentLatitude,
    double CurrentLongitude,
    double DestinationLatitude,
    double DestinationLongitude,
    string? TrafficCondition = "NORMAL");

/// <summary>
/// ETA response
/// </summary>
public record ETAResponse(
    Guid OrderId,
    int EstimatedMinutes,
    double DistanceKm,
    DateTime EstimatedArrivalTime,
    string TrafficCondition,
    DateTime CalculatedAt);