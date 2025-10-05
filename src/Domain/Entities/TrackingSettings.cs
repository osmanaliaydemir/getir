using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class TrackingSettings
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; } // User-specific settings
    public Guid? MerchantId { get; set; } // Merchant-specific settings
    public bool EnableLocationTracking { get; set; } = true;
    public bool EnablePushNotifications { get; set; } = true;
    public bool EnableSMSNotifications { get; set; } = true;
    public bool EnableEmailNotifications { get; set; } = true;
    public int LocationUpdateInterval { get; set; } = 30; // seconds
    public int NotificationInterval { get; set; } = 300; // seconds
    public double LocationAccuracyThreshold { get; set; } = 100; // meters
    public bool EnableETAUpdates { get; set; } = true;
    public int ETAUpdateInterval { get; set; } = 60; // seconds
    public bool EnableDelayAlerts { get; set; } = true;
    public int DelayThresholdMinutes { get; set; } = 15; // minutes
    public bool EnableNearbyAlerts { get; set; } = true;
    public double NearbyDistanceMeters { get; set; } = 500; // meters
    public string? PreferredLanguage { get; set; } = "tr";
    public string? TimeZone { get; set; } = "Europe/Istanbul";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual Merchant? Merchant { get; set; }
    public virtual User? CreatedByUser { get; set; }
    public virtual User? UpdatedByUser { get; set; }
}
