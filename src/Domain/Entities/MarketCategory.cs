namespace Getir.Domain.Entities;

/// <summary>
/// Market kategorisi - Market ürün kategorileri için
/// </summary>
public class MarketCategory
{
    public Guid Id { get; set; }
    public Guid MarketId { get; set; }
    public Guid? ParentCategoryId { get; set; }
    
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Market Market { get; set; } = default!;
    public virtual MarketCategory? ParentCategory { get; set; }
    public virtual ICollection<MarketCategory> SubCategories { get; set; } = new List<MarketCategory>();
    public virtual ICollection<MarketProduct> Products { get; set; } = new List<MarketProduct>();
}
