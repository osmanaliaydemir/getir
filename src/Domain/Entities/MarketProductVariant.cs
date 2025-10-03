namespace Getir.Domain.Entities;

/// <summary>
/// Market ürün varyantları - Boyut, renk, tat vb.
/// </summary>
public class MarketProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    
    public string Name { get; set; } = default!; // "Küçük", "Büyük", "Kırmızı", "Mavi" vb.
    public string? Description { get; set; }
    public string? SKU { get; set; } // Varyant stok kodu
    public decimal Price { get; set; } // Varyant fiyatı
    public decimal? DiscountedPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public int DisplayOrder { get; set; }
    
    // Varyant özellikleri
    public string? Size { get; set; } // S, M, L, XL
    public string? Color { get; set; } // Renk
    public string? Flavor { get; set; } // Tat
    public string? Material { get; set; } // Malzeme
    public string? Weight { get; set; } // Ağırlık
    public string? Volume { get; set; } // Hacim
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual MarketProduct Product { get; set; } = default!;
}
