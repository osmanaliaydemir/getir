namespace Getir.Domain.Entities;

/// <summary>
/// Restoran ürün seçenek grupları - Boyut, Ekstra malzemeler vb.
/// </summary>
public class RestaurantProductOptionGroup
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsRequired { get; set; } // Zorunlu grup mu?
    public bool IsMultipleSelection { get; set; } // Çoklu seçim mi?
    public int MaxSelections { get; set; } // Maksimum seçim sayısı
    public int DisplayOrder { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual RestaurantProduct Product { get; set; } = default!;
    public virtual ICollection<RestaurantProductOption> Options { get; set; } = new List<RestaurantProductOption>();
}
