using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class ETAEstimation
{
    public Guid Id { get; set; }
    public Guid OrderTrackingId { get; set; }
    public DateTime EstimatedArrivalTime { get; set; }
    public int EstimatedMinutesRemaining { get; set; }
    public double? DistanceRemaining { get; set; } // in kilometers
    public double? AverageSpeed { get; set; } // in km/h
    public string? CalculationMethod { get; set; } // algorithm, historical, manual
    public double? Confidence { get; set; } // confidence level 0-1
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    // Navigation properties
    public virtual OrderTracking? OrderTracking { get; set; }
    public virtual User? CreatedByUser { get; set; }
}
