namespace Getir.Domain.Entities;

/// <summary>
/// Market ürün entity'si - Gıda ve temizlik ürünleri için özel özellikler
/// </summary>
public class MarketProduct
{
    public Guid Id { get; set; }
    public Guid MarketId { get; set; }
    public Guid? CategoryId { get; set; }
    
    // Temel ürün bilgileri
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    
    // Market özel özellikleri
    public string? Brand { get; set; } // Marka
    public string? Barcode { get; set; } // Barkod
    public string? SKU { get; set; } // Stok kodu
    public string? Unit { get; set; } // Birim (kg, adet, litre, vb.)
    public decimal? Weight { get; set; } // Ağırlık
    public decimal? Volume { get; set; } // Hacim
    public string? Size { get; set; } // Boyut (S, M, L, XL)
    public string? Color { get; set; } // Renk
    
    // Tarih bilgileri
    public DateTime? ExpiryDate { get; set; } // Son kullanma tarihi
    public DateTime? ProductionDate { get; set; } // Üretim tarihi
    public DateTime? BestBeforeDate { get; set; } // Son tüketim tarihi
    
    // Menşei bilgileri
    public string? Origin { get; set; } // Menşei ülke
    public string? OriginCity { get; set; } // Menşei şehir
    public bool IsLocal { get; set; } // Yerel ürün mü?
    public bool IsOrganic { get; set; } // Organik mi?
    public bool IsFairTrade { get; set; } // Adil ticaret mi?
    
    // Alerjen bilgileri
    public string? Allergens { get; set; } // JSON formatında alerjen listesi
    public string? AllergenWarnings { get; set; } // Alerjen uyarıları
    
    // Besin değerleri
    public string? NutritionInfo { get; set; } // JSON formatında besin değerleri
    public int? Calories { get; set; } // Kalori (100g için)
    public decimal? Protein { get; set; } // Protein (100g için)
    public decimal? Carbs { get; set; } // Karbonhidrat (100g için)
    public decimal? Fat { get; set; } // Yağ (100g için)
    public decimal? Fiber { get; set; } // Lif (100g için)
    public decimal? Sugar { get; set; } // Şeker (100g için)
    public decimal? Sodium { get; set; } // Sodyum (100g için)
    
    // Ürün özellikleri
    public bool IsPopular { get; set; } // Popüler ürün mü?
    public bool IsNew { get; set; } // Yeni ürün mü?
    public bool IsOnSale { get; set; } // İndirimde mi?
    public bool IsSeasonal { get; set; } // Mevsimlik mi?
    public bool IsLimitedEdition { get; set; } // Sınırlı sayıda mı?
    
    // Stok bilgileri
    public int StockQuantity { get; set; } // Stok miktarı
    public int? MinOrderQuantity { get; set; } // Minimum sipariş miktarı
    public int? MaxOrderQuantity { get; set; } // Maksimum sipariş miktarı
    public bool IsUnlimitedStock { get; set; } // Sınırsız stok mu?
    
    // Depolama bilgileri
    public string? StorageConditions { get; set; } // Depolama koşulları
    public string? Temperature { get; set; } // Saklama sıcaklığı
    public bool RequiresRefrigeration { get; set; } // Soğutma gerektirir mi?
    public bool RequiresFreezing { get; set; } // Dondurma gerektirir mi?
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Market Market { get; set; } = default!;
    public virtual MarketCategory? Category { get; set; }
    public virtual ICollection<MarketProductVariant> Variants { get; set; } = new List<MarketProductVariant>();
    public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
}
