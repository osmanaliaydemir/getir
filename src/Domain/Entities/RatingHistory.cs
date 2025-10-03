using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class RatingHistory
{
    public Guid Id { get; set; }
    public Guid EntityId { get; set; } // Merchant or Courier ID
    public string EntityType { get; set; } = default!; // Merchant, Courier
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int NewReviews { get; set; }
    public DateTime SnapshotDate { get; set; }
    public decimal? FoodQualityRating { get; set; }
    public decimal? DeliverySpeedRating { get; set; }
    public decimal? ServiceRating { get; set; }
    public decimal? ValueRating { get; set; }
    public decimal? PackagingRating { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual Merchant? Merchant { get; set; }
    public virtual Courier? Courier { get; set; }
}
