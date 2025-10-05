using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class OrderTracking
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? CourierId { get; set; }
    public TrackingStatus Status { get; set; }
    public string? StatusMessage { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public LocationUpdateType LocationUpdateType { get; set; }
    public double? Accuracy { get; set; } // GPS accuracy in meters
    public DateTime? EstimatedArrivalTime { get; set; }
    public DateTime? ActualArrivalTime { get; set; }
    public int? EstimatedMinutesRemaining { get; set; }
    public double? DistanceFromDestination { get; set; } // in kilometers
    public bool IsActive { get; set; } = true;
    public DateTime LastUpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public virtual Order? Order { get; set; }
    public virtual Courier? Courier { get; set; }
    public virtual User? UpdatedByUser { get; set; }
    public virtual ICollection<LocationHistory> LocationHistory { get; set; } = new List<LocationHistory>();
    public virtual ICollection<TrackingNotification> Notifications { get; set; } = new List<TrackingNotification>();
}
