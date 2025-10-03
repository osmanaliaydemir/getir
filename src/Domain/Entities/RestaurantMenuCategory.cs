namespace Getir.Domain.Entities;

/// <summary>
/// Restoran menü kategorisi - Yemek kategorileri için
/// </summary>
public class RestaurantMenuCategory
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid? ParentCategoryId { get; set; }
    
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Restaurant Restaurant { get; set; } = default!;
    public virtual RestaurantMenuCategory? ParentCategory { get; set; }
    public virtual ICollection<RestaurantMenuCategory> SubCategories { get; set; } = new List<RestaurantMenuCategory>();
    public virtual ICollection<RestaurantProduct> Products { get; set; } = new List<RestaurantProduct>();
}
