namespace Getir.Domain.Entities;

/// <summary>
/// Restoran ürün seçenekleri - Ekstra malzemeler, boyut seçenekleri vb.
/// </summary>
public class RestaurantProductOption
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? OptionGroupId { get; set; }
    
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; } // Ekstra ücret
    public bool IsRequired { get; set; } // Zorunlu seçenek mi?
    public bool IsAvailable { get; set; }
    public int DisplayOrder { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual RestaurantProduct Product { get; set; } = default!;
    public virtual RestaurantProductOptionGroup? OptionGroup { get; set; }
}
