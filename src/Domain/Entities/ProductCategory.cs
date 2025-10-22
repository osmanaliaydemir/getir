namespace Getir.Domain.Entities;

/// <summary>
/// Ürün Kategorileri (Merchant-specific, hiyerarşik yapı)
/// Örnek: Migros → "Meyve-Sebze" → "Organik Meyveler"
/// </summary>
public class ProductCategory
{
    public Guid Id { get; set; }
    public Guid? MerchantId { get; set; } // Nullable for standard categories
    public Guid? ServiceCategoryId { get; set; } // For standard categories
    public Guid? ParentCategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Merchant? Merchant { get; set; } // Nullable for standard categories
    public virtual ServiceCategory? ServiceCategory { get; set; }
    public virtual ProductCategory? ParentCategory { get; set; }
    public virtual ICollection<ProductCategory> SubCategories { get; set; } = new List<ProductCategory>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

