namespace Getir.Domain.Entities;

public class Merchant
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; } // Merchant sahibi (User)
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid ServiceCategoryId { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Address { get; set; } = default!;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string PhoneNumber { get; set; } = default!;
    public string? Email { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public int AverageDeliveryTime { get; set; }
    public decimal? Rating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsActive { get; set; }
    public bool IsBusy { get; set; }
    public bool IsOpen { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual User Owner { get; set; } = default!;
    public virtual ServiceCategory ServiceCategory { get; set; } = default!;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<WorkingHours> WorkingHours { get; set; } = new List<WorkingHours>();
    public virtual ICollection<DeliveryZone> DeliveryZones { get; set; } = new List<DeliveryZone>();
    public virtual MerchantOnboarding? Onboarding { get; set; }
    public virtual ICollection<RatingHistory> RatingHistories { get; set; } = new List<RatingHistory>();
}
