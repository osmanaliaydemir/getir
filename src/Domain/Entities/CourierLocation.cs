namespace Getir.Domain.Entities;

/// <summary>
/// Courier GPS location tracking entity
/// Stores real-time courier location updates for delivery tracking
/// </summary>
public class CourierLocation
{
    public Guid Id { get; set; }
    public Guid CourierId { get; set; }
    public Guid? OrderId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
    public double? Speed { get; set; } // km/h
    public double? Accuracy { get; set; } // meters
    public double? Bearing { get; set; } // degrees
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual User? Courier { get; set; }
    public virtual Order? Order { get; set; }
}

