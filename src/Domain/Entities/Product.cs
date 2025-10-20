namespace Getir.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Unit { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? ExternalId { get; set; } // External system ID for synchronization
    
    // Rating & Review fields
    public decimal? Rating { get; set; } // Average rating (1-5), calculated from reviews
    public int? ReviewCount { get; set; } // Total number of reviews
    
    public byte[] RowVersion { get; set; } = default!; // Optimistic locking için
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Merchant Merchant { get; set; } = default!;
    public virtual ProductCategory? ProductCategory { get; set; }
    public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    public virtual ICollection<ProductOptionGroup> OptionGroups { get; set; } = new List<ProductOptionGroup>();
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
}
