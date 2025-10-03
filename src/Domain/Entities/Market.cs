using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Market entity'si - Gıda ve temizlik ürünleri için özel özellikler
/// </summary>
public class Market
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    
    // Market özel bilgileri
    public string MarketType { get; set; } = default!; // Süpermarket, Bakkal, Organik Market, vb.
    public bool IsOrganic { get; set; } // Organik ürünler
    public bool IsLocal { get; set; } // Yerel ürünler
    public bool IsInternational { get; set; } // Uluslararası ürünler
    
    // Çalışma saatleri
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public bool IsOpen24Hours { get; set; }
    
    // Teslimat bilgileri
    public decimal MinimumOrderAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public int DeliveryRadius { get; set; } // KM cinsinden
    public int AverageDeliveryTime { get; set; } // Dakika cinsinden
    
    // Market özellikleri
    public bool HasFreshProducts { get; set; } // Taze ürünler
    public bool HasFrozenProducts { get; set; } // Dondurulmuş ürünler
    public bool HasPharmacy { get; set; } // Eczane bölümü
    public bool HasBakery { get; set; } // Fırın bölümü
    public bool HasButcher { get; set; } // Kasap bölümü
    public bool HasDeli { get; set; } // Şarküteri bölümü
    
    // Özel hizmetler
    public bool HasOnlineShopping { get; set; }
    public bool HasClickAndCollect { get; set; }
    public bool HasHomeDelivery { get; set; }
    public bool HasExpressDelivery { get; set; } // 30 dakika teslimat
    
    // Sertifikalar
    public string? Certifications { get; set; } // JSON formatında (ISO, HACCP, vb.)
    public string? QualityStandards { get; set; } // JSON formatında
    
    // Sosyal medya
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Merchant Merchant { get; set; } = default!;
    public virtual ICollection<MarketProduct> Products { get; set; } = new List<MarketProduct>();
    public virtual ICollection<MarketCategory> Categories { get; set; } = new List<MarketCategory>();
}
