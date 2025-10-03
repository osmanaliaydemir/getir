using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Restoran entity'si - Yemek siparişi için özel özellikler
/// </summary>
public class Restaurant
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    
    // Restoran özel bilgileri
    public string CuisineType { get; set; } = default!; // Türk, İtalyan, Çin, vb.
    public int AveragePreparationTime { get; set; } // Dakika cinsinden
    public bool IsHalal { get; set; } // Helal sertifikası
    public bool IsVegetarianFriendly { get; set; } // Vejetaryen dostu
    public bool IsVeganFriendly { get; set; } // Vegan dostu
    public bool IsGlutenFree { get; set; } // Glutensiz seçenekler
    public bool IsLactoseFree { get; set; } // Laktozsuz seçenekler
    
    // Çalışma saatleri
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public bool IsOpen24Hours { get; set; }
    
    // Teslimat bilgileri
    public decimal MinimumOrderAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public int DeliveryRadius { get; set; } // KM cinsinden
    public int AverageDeliveryTime { get; set; } // Dakika cinsinden
    
    // Restoran özellikleri
    public bool HasOutdoorSeating { get; set; }
    public bool HasIndoorSeating { get; set; }
    public bool HasTakeaway { get; set; }
    public bool HasDelivery { get; set; }
    public bool HasDriveThrough { get; set; }
    
    // Sertifikalar ve ödüller
    public string? Certifications { get; set; } // JSON formatında
    public string? Awards { get; set; } // JSON formatında
    
    // Sosyal medya
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Merchant Merchant { get; set; } = default!;
    public virtual ICollection<RestaurantProduct> Products { get; set; } = new List<RestaurantProduct>();
    public virtual ICollection<RestaurantMenuCategory> MenuCategories { get; set; } = new List<RestaurantMenuCategory>();
}
