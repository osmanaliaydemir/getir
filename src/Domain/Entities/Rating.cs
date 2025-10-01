using System.ComponentModel.DataAnnotations;

namespace Getir.Domain.Entities;

/// <summary>
/// Aggregated rating information for merchants and couriers
/// </summary>
public class Rating
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid EntityId { get; set; } // Merchant or Courier ID
    
    [Required]
    public string EntityType { get; set; } = string.Empty; // "Merchant" or "Courier"
    
    // Rating statistics
    public decimal AverageRating { get; set; } = 0m;
    public int TotalReviews { get; set; } = 0;
    public int FiveStarCount { get; set; } = 0;
    public int FourStarCount { get; set; } = 0;
    public int ThreeStarCount { get; set; } = 0;
    public int TwoStarCount { get; set; } = 0;
    public int OneStarCount { get; set; } = 0;
    
    // Recent performance (last 30 days)
    public decimal RecentAverageRating { get; set; } = 0m;
    public int RecentReviewCount { get; set; } = 0;
    
    // Quality metrics
    public decimal ResponseRate { get; set; } = 0m; // Percentage of orders with reviews
    public decimal PositiveReviewRate { get; set; } = 0m; // Percentage of 4-5 star reviews
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReviewDate { get; set; }
    
    // Navigation properties
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}

/// <summary>
/// Rating history for tracking rating changes over time
/// </summary>
public class RatingHistory
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid EntityId { get; set; }
    
    [Required]
    public string EntityType { get; set; } = string.Empty;
    
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int NewReviews { get; set; } // Reviews since last snapshot
    
    public DateTime SnapshotDate { get; set; } = DateTime.UtcNow;
    
    // Optional: Track specific metrics
    public decimal? FoodQualityRating { get; set; }
    public decimal? DeliverySpeedRating { get; set; }
    public decimal? ServiceRating { get; set; }
}
