using Getir.Domain.Enums;

namespace Getir.Application.DTO;

public class OrderTrackingDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? CourierId { get; set; }
    public string? CourierName { get; set; }
    public string? CourierPhone { get; set; }
    public TrackingStatus Status { get; set; }
    public string StatusDisplayName { get; set; } = default!;
    public string? StatusMessage { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public LocationUpdateType LocationUpdateType { get; set; }
    public double? Accuracy { get; set; }
    public DateTime? EstimatedArrivalTime { get; set; }
    public DateTime? ActualArrivalTime { get; set; }
    public int? EstimatedMinutesRemaining { get; set; }
    public double? DistanceFromDestination { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LocationUpdateRequest
{
    public Guid OrderTrackingId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public LocationUpdateType UpdateType { get; set; }
    public double? Accuracy { get; set; }
    public double? Speed { get; set; }
    public double? Bearing { get; set; }
    public double? Altitude { get; set; }
    public string? DeviceInfo { get; set; }
    public string? AppVersion { get; set; }
}

public class LocationUpdateResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public DateTime UpdatedAt { get; set; }
    public double? DistanceFromDestination { get; set; }
    public int? EstimatedMinutesRemaining { get; set; }
}

public class StatusUpdateRequest
{
    public Guid OrderTrackingId { get; set; }
    public TrackingStatus Status { get; set; }
    public string? StatusMessage { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
}

public class StatusUpdateResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool NotificationSent { get; set; }
}

public class ETAEstimationDto
{
    public Guid Id { get; set; }
    public Guid OrderTrackingId { get; set; }
    public DateTime EstimatedArrivalTime { get; set; }
    public int EstimatedMinutesRemaining { get; set; }
    public double? DistanceRemaining { get; set; }
    public double? AverageSpeed { get; set; }
    public string? CalculationMethod { get; set; }
    public double? Confidence { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateETAEstimationRequest
{
    public Guid OrderTrackingId { get; set; }
    public DateTime EstimatedArrivalTime { get; set; }
    public int EstimatedMinutesRemaining { get; set; }
    public double? DistanceRemaining { get; set; }
    public double? AverageSpeed { get; set; }
    public string? CalculationMethod { get; set; }
    public double? Confidence { get; set; }
    public string? Notes { get; set; }
}

public class UpdateETAEstimationRequest
{
    public DateTime EstimatedArrivalTime { get; set; }
    public int EstimatedMinutesRemaining { get; set; }
    public double? DistanceRemaining { get; set; }
    public double? AverageSpeed { get; set; }
    public string? CalculationMethod { get; set; }
    public double? Confidence { get; set; }
    public string? Notes { get; set; }
}

public class TrackingNotificationDto
{
    public Guid Id { get; set; }
    public Guid OrderTrackingId { get; set; }
    public Guid? UserId { get; set; }
    public NotificationType Type { get; set; }
    public string TypeDisplayName { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? Data { get; set; }
    public bool IsSent { get; set; }
    public bool IsRead { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? DeliveryMethod { get; set; }
    public string? DeliveryStatus { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendNotificationRequest
{
    public Guid OrderTrackingId { get; set; }
    public Guid? UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? Data { get; set; }
    public string? DeliveryMethod { get; set; } // push, sms, email
}

public class SendNotificationResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid NotificationId { get; set; }
    public DateTime SentAt { get; set; }
}

public class LocationHistoryDto
{
    public Guid Id { get; set; }
    public Guid OrderTrackingId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public LocationUpdateType UpdateType { get; set; }
    public string UpdateTypeDisplayName { get; set; } = default!;
    public double? Accuracy { get; set; }
    public double? Speed { get; set; }
    public double? Bearing { get; set; }
    public double? Altitude { get; set; }
    public string? DeviceInfo { get; set; }
    public string? AppVersion { get; set; }
    public DateTime RecordedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TrackingSettingsDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? MerchantId { get; set; }
    public bool EnableLocationTracking { get; set; }
    public bool EnablePushNotifications { get; set; }
    public bool EnableSMSNotifications { get; set; }
    public bool EnableEmailNotifications { get; set; }
    public int LocationUpdateInterval { get; set; }
    public int NotificationInterval { get; set; }
    public double LocationAccuracyThreshold { get; set; }
    public bool EnableETAUpdates { get; set; }
    public int ETAUpdateInterval { get; set; }
    public bool EnableDelayAlerts { get; set; }
    public int DelayThresholdMinutes { get; set; }
    public bool EnableNearbyAlerts { get; set; }
    public double NearbyDistanceMeters { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? TimeZone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateTrackingSettingsRequest
{
    public bool EnableLocationTracking { get; set; }
    public bool EnablePushNotifications { get; set; }
    public bool EnableSMSNotifications { get; set; }
    public bool EnableEmailNotifications { get; set; }
    public int LocationUpdateInterval { get; set; }
    public int NotificationInterval { get; set; }
    public double LocationAccuracyThreshold { get; set; }
    public bool EnableETAUpdates { get; set; }
    public int ETAUpdateInterval { get; set; }
    public bool EnableDelayAlerts { get; set; }
    public int DelayThresholdMinutes { get; set; }
    public bool EnableNearbyAlerts { get; set; }
    public double NearbyDistanceMeters { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? TimeZone { get; set; }
}

public class TrackingSearchRequest
{
    public Guid? OrderId { get; set; }
    public Guid? CourierId { get; set; }
    public Guid? UserId { get; set; }
    public TrackingStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class TrackingSearchResponse
{
    public List<OrderTrackingDto> Trackings { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class RealtimeTrackingData
{
    public Guid OrderTrackingId { get; set; }
    public Guid OrderId { get; set; }
    public TrackingStatus Status { get; set; }
    public string StatusDisplayName { get; set; } = default!;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public DateTime? EstimatedArrivalTime { get; set; }
    public int? EstimatedMinutesRemaining { get; set; }
    public double? DistanceFromDestination { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class TrackingStatisticsDto
{
    public int TotalTrackings { get; set; }
    public int ActiveTrackings { get; set; }
    public int CompletedTrackings { get; set; }
    public int CancelledTrackings { get; set; }
    public double AverageDeliveryTime { get; set; } // in minutes
    public double AverageDistance { get; set; } // in kilometers
    public Dictionary<TrackingStatus, int> StatusCounts { get; set; } = new();
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
