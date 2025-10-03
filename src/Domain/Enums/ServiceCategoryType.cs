namespace Getir.Domain.Enums;

/// <summary>
/// Hizmet kategori tipleri - Restoran ve Market ayrımı için
/// </summary>
public enum ServiceCategoryType
{
    /// <summary>
    /// Restoran - Yemek siparişi
    /// </summary>
    Restaurant = 1,
    
    /// <summary>
    /// Market - Gıda ve temizlik ürünleri
    /// </summary>
    Market = 2,
    
    /// <summary>
    /// Eczane - İlaç ve sağlık ürünleri
    /// </summary>
    Pharmacy = 3,
    
    /// <summary>
    /// Su - Su teslimatı
    /// </summary>
    Water = 4,
    
    /// <summary>
    /// Kafe - Kahve ve atıştırmalık
    /// </summary>
    Cafe = 5,
    
    /// <summary>
    /// Pastane - Tatlı ve hamur işi
    /// </summary>
    Bakery = 6,
    
    /// <summary>
    /// Diğer - Diğer hizmetler
    /// </summary>
    Other = 99
}

public static class ServiceCategoryTypeExtensions
{
    public static string GetDisplayName(this ServiceCategoryType type)
    {
        return type switch
        {
            ServiceCategoryType.Restaurant => "Restoran",
            ServiceCategoryType.Market => "Market",
            ServiceCategoryType.Pharmacy => "Eczane",
            ServiceCategoryType.Water => "Su",
            ServiceCategoryType.Cafe => "Kafe",
            ServiceCategoryType.Bakery => "Pastane",
            ServiceCategoryType.Other => "Diğer",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    public static string GetDescription(this ServiceCategoryType type)
    {
        return type switch
        {
            ServiceCategoryType.Restaurant => "Yemek siparişi ve teslimatı",
            ServiceCategoryType.Market => "Gıda ve temizlik ürünleri",
            ServiceCategoryType.Pharmacy => "İlaç ve sağlık ürünleri",
            ServiceCategoryType.Water => "Su teslimatı",
            ServiceCategoryType.Cafe => "Kahve ve atıştırmalık",
            ServiceCategoryType.Bakery => "Tatlı ve hamur işi",
            ServiceCategoryType.Other => "Diğer hizmetler",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    public static bool IsFoodRelated(this ServiceCategoryType type)
    {
        return type is ServiceCategoryType.Restaurant or ServiceCategoryType.Cafe or ServiceCategoryType.Bakery;
    }
    
    public static bool IsProductRelated(this ServiceCategoryType type)
    {
        return type is ServiceCategoryType.Market or ServiceCategoryType.Pharmacy or ServiceCategoryType.Water;
    }
}
