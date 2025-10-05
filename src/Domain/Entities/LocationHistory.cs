using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class LocationHistory
{
    public Guid Id { get; set; }
    public Guid OrderTrackingId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public LocationUpdateType UpdateType { get; set; }
    public double? Accuracy { get; set; } // GPS accuracy in meters
    public double? Speed { get; set; } // Speed in km/h
    public double? Bearing { get; set; } // Direction in degrees
    public double? Altitude { get; set; } // Altitude in meters
    public string? DeviceInfo { get; set; } // Device information
    public string? AppVersion { get; set; } // App version
    public DateTime RecordedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual OrderTracking? OrderTracking { get; set; }
}
