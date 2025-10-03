namespace Getir.Domain.Entities;

/// <summary>
/// Restoran ürün entity'si - Yemek için özel özellikler
/// </summary>
public class RestaurantProduct
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid? MenuCategoryId { get; set; }
    
    // Temel ürün bilgileri
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    
    // Restoran özel özellikleri
    public int PreparationTimeMinutes { get; set; } // Hazırlık süresi
    public string? PortionSize { get; set; } // Porsiyon bilgisi (1 porsiyon, 2 kişilik, vb.)
    public string? SpiceLevel { get; set; } // Acılık seviyesi (Mild, Medium, Hot, Extra Hot)
    public bool IsSpicy { get; set; } // Acılı mı?
    public bool IsVegetarian { get; set; } // Vejetaryen mi?
    public bool IsVegan { get; set; } // Vegan mı?
    public bool IsGlutenFree { get; set; } // Glutensiz mi?
    public bool IsLactoseFree { get; set; } // Laktozsuz mu?
    public bool IsHalal { get; set; } // Helal mi?
    
    // Alerjen bilgileri
    public string? Allergens { get; set; } // JSON formatında alerjen listesi
    public string? AllergenWarnings { get; set; } // Alerjen uyarıları
    
    // Besin değerleri
    public string? NutritionInfo { get; set; } // JSON formatında besin değerleri
    public int? Calories { get; set; } // Kalori
    public decimal? Protein { get; set; } // Protein (gram)
    public decimal? Carbs { get; set; } // Karbonhidrat (gram)
    public decimal? Fat { get; set; } // Yağ (gram)
    public decimal? Fiber { get; set; } // Lif (gram)
    public decimal? Sugar { get; set; } // Şeker (gram)
    public decimal? Sodium { get; set; } // Sodyum (mg)
    
    // Ürün özellikleri
    public bool IsPopular { get; set; } // Popüler ürün mü?
    public bool IsNew { get; set; } // Yeni ürün mü?
    public bool IsChefSpecial { get; set; } // Şef özel mi?
    public bool IsSeasonal { get; set; } // Mevsimlik mi?
    
    // Stok bilgileri
    public int StockQuantity { get; set; } // Stok miktarı
    public bool IsUnlimitedStock { get; set; } // Sınırsız stok mu?
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Restaurant Restaurant { get; set; } = default!;
    public virtual RestaurantMenuCategory? MenuCategory { get; set; }
    public virtual ICollection<RestaurantProductOption> Options { get; set; } = new List<RestaurantProductOption>();
    public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
}
