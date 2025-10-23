namespace Getir.Application.Common;

/// <summary>
/// Centralized cache key naming strategy (Önbellek anahtarı adlandırma stratejisi)
/// Redis en iyi uygulamalarını takip eder: namespace:entity:identifier
/// </summary>
public static class CacheKeys
{
    // Time-to-Live constants (saniye)
    public static class TTL
    {
        public const int VeryShort = 2;      // 2 saniye - volatil veri için
        public const int Short = 5;          // 5 saniye - dinamik veri için
        public const int Medium = 15;        // 15 saniye - yarı statik veri için
        public const int Long = 30;          // 30 saniye - statik veri için
        public const int VeryLong = 60;      // 1 saat - nadir değişen veri için
        public const int ExtraLong = 240;    // 4 saat - yapılandırma veri için
    }

    #region Products

    /// <summary>
    /// Tek bir ürün: "product:123e4567-e89b-12d3-a456-426614174000"
    /// </summary>
    public static string Product(Guid productId) => $"product:{productId}";

    /// <summary>
    /// Satıcının ürünleri: "products:merchant:123e4567:page:1:size:20"
    /// </summary>
    public static string ProductsByMerchant(Guid merchantId, int page, int pageSize)
        => $"products:merchant:{merchantId}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Kategoriye göre ürünler: "products:category:123e4567:page:1:size:20"
    /// </summary>
    public static string ProductsByCategory(Guid categoryId, int page, int pageSize)
        => $"products:category:{categoryId}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Tüm ürünler pattern için geçersiz kılma: "product:*" or "products:merchant:123e4567:*"
    /// </summary>
    public static string AllProducts() => "product:*";
    public static string AllProductsByMerchant(Guid merchantId) => $"products:merchant:{merchantId}:*";

    #endregion

    #region Product Categories

    /// <summary>
    /// Tek bir kategori: "category:123e4567-e89b-12d3-a456-426614174000"
    /// </summary>
    public static string ProductCategory(Guid categoryId) => $"category:{categoryId}";

    /// <summary>
    /// Tüm kategoriler listesi: "categories:all"
    /// </summary>
    public static string AllProductCategories() => "categories:all";

    /// <summary>
    /// Satıcının kategorileri: "categories:merchant:123e4567"
    /// </summary>
    public static string CategoriesByMerchant(Guid merchantId) => $"categories:merchant:{merchantId}";

    /// <summary>
    /// Tüm kategoriler pattern: "category:*" or "categories:*"
    /// </summary>
    public static string AllCategoriesPattern() => "categor*";

    #endregion

    #region Merchants

    /// <summary>
    /// Tek bir satıcı: "merchant:123e4567-e89b-12d3-a456-426614174000"
    /// </summary>
    public static string Merchant(Guid merchantId) => $"merchant:{merchantId}";

    /// <summary>
    /// Satıcının sahibi: "merchant:owner:123e4567"
    /// </summary>
    public static string MerchantByOwner(Guid ownerId) => $"merchant:owner:{ownerId}";

    /// <summary>
    /// Teslimat bölgesindeki satıcılar: "merchants:zone:123e4567:page:1"
    /// </summary>
    public static string MerchantsByZone(Guid zoneId, int page, int pageSize)
        => $"merchants:zone:{zoneId}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Aktif satıcılar (sayfalama): "merchants:active:page:1:size:20"
    /// </summary>
    public static string ActiveMerchants(int page, int pageSize)
        => $"merchants:active:page:{page}:size:{pageSize}";

    /// <summary>
    /// Tüm satıcılar pattern: "merchant:*" or "merchants:*"
    /// </summary>
    public static string AllMerchants() => "merchant*";

    #endregion

    #region Delivery Zones

    /// <summary>
    /// Tüm teslimat bölgeleri: "zones:all"
    /// </summary>
    public static string AllDeliveryZones() => "zones:all";

    /// <summary>
    /// Aktif teslimat bölgeleri: "zones:active"
    /// </summary>
    public static string ActiveDeliveryZones() => "zones:active";

    /// <summary>
    /// Koordinatlar (geo-cache) bazında teslimat bölgeleri: "zones:geo:lat:41.0082:lon:28.9784"
    /// </summary>
    public static string ZonesByCoordinates(double latitude, double longitude)
        => $"zones:geo:lat:{latitude:F4}:lon:{longitude:F4}";

    /// <summary>
    /// Satıcının teslimat bölgeleri: "zones:merchant:{merchantId}"
    /// </summary>
    public static string DeliveryZonesByMerchant(Guid merchantId) => $"zones:merchant:{merchantId}";

    /// <summary>
    /// Tek bir teslimat bölgesi: "zone:{zoneId}"
    /// </summary>
    public static string DeliveryZone(Guid zoneId) => $"zone:{zoneId}";

    /// <summary>
    /// Tüm teslimat bölgeleri pattern: "zone*"
    /// </summary>
    public static string AllDeliveryZonesPattern() => "zone*";

    #endregion

    #region Reviews

    /// <summary>
    /// Ürün incelemeleri (sayfalama): "reviews:product:123e4567:page:1:size:10"
    /// </summary>
    public static string ProductReviews(Guid productId, int page, int pageSize)
        => $"reviews:product:{productId}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Satıcı incelemeleri: "reviews:merchant:123e4567:page:1:size:10"
    /// </summary>
    public static string MerchantReviews(Guid merchantId, int page, int pageSize)
        => $"reviews:merchant:{merchantId}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Ürün ortalama puanı: "rating:product:123e4567"
    /// </summary>
    public static string ProductRating(Guid productId) => $"rating:product:{productId}";

    #endregion

    #region Search

    /// <summary>
    /// Arama sonuçları (konum bazında): "search:query:pizza:location:123e4567:page:1"
    /// </summary>
    public static string SearchResults(string query, Guid? locationId, int page, int pageSize)
        => $"search:query:{NormalizeSearchQuery(query)}:location:{locationId?.ToString() ?? "all"}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Popüler aramalar: "search:popular:limit:10"
    /// </summary>
    public static string PopularSearches(int limit) => $"search:popular:limit:{limit}";

    /// <summary>
    /// Tüm arama cache'ini temizle: "search:*"
    /// </summary>
    public static string AllSearchResults() => "search:*";

    private static string NormalizeSearchQuery(string query)
        => query.ToLowerInvariant().Replace(" ", "-").Replace(":", "");

    #endregion

    #region Cart (Short TTL)

    /// <summary>
    /// Kullanıcı sepeti: "cart:user:123e4567-e89b-12d3-a456-426614174000"
    /// WARNING: Use with caution, short TTL recommended
    /// </summary>
    public static string UserCart(Guid userId) => $"cart:user:{userId}";

    #endregion

    #region Coupons

    /// <summary>
    /// Mevcut kuponlar listesi: "coupons:available:page:1:size:20"
    /// </summary>
    public static string AvailableCoupons(int page, int pageSize)
        => $"coupons:available:page:{page}:size:{pageSize}";

    /// <summary>
    /// Kullanıcının kuponları: "coupons:user:123e4567"
    /// </summary>
    public static string UserCoupons(Guid userId) => $"coupons:user:{userId}";

    #endregion

    #region Working Hours

    /// <summary>
    /// Satıcı çalışma saatleri: "hours:merchant:123e4567"
    /// </summary>
    public static string MerchantWorkingHours(Guid merchantId) => $"hours:merchant:{merchantId}";

    #endregion

    #region Statistics & Analytics

    /// <summary>
    /// Satıcı dashboard istatistikleri: "stats:merchant:123e4567:date:2025-01-15"
    /// </summary>
    public static string MerchantStats(Guid merchantId, DateTime date)
        => $"stats:merchant:{merchantId}:date:{date:yyyy-MM-dd}";

    /// <summary>
    /// Platform genel istatistikleri: "stats:platform:date:2025-01-15"
    /// </summary>
    public static string PlatformStats(DateTime date)
        => $"stats:platform:date:{date:yyyy-MM-dd}";

    #endregion

    #region User Preferences

    /// <summary>
    /// Kullanıcı dil tercihi: "pref:user:123e4567:language"
    /// </summary>
    public static string UserLanguagePreference(Guid userId) => $"pref:user:{userId}:language";

    /// <summary>
    /// Kullanıcı bildirim tercihleri: "pref:user:123e4567:notifications"
    /// </summary>
    public static string UserNotificationPreferences(Guid userId) => $"pref:user:{userId}:notifications";

    #endregion

    #region System Configuration

    /// <summary>
    /// Sistem ayarları: "config:system:setting-name" (örneğin: "payment_gateway", "notification_channels", "delivery_options")
    /// </summary>
    public static string SystemConfig(string settingName) => $"config:system:{settingName}";

    /// <summary>
    /// Özellik flag'leri: "config:feature:{featureName}" (örneğin: "new_payment_gateway", "sms_notifications", "geo_location_tracking")
    /// </summary>
    public static string FeatureFlag(string featureName) => $"config:feature:{featureName}";

    #endregion

    #region Service Categories

    /// <summary>
    /// Tüm hizmet kategorileri: "service-categories:all:page:{page}:size:{size}"
    /// </summary>
    public static string AllServiceCategories(int page, int pageSize)
        => $"service-categories:all:page:{page}:size:{pageSize}";

    /// <summary>
    /// Tek bir hizmet kategorisi: "service-category:{categoryId}"
    /// </summary>
    public static string ServiceCategory(Guid categoryId) => $"service-category:{categoryId}";

    /// <summary>
    /// Hizmet kategorileri (tür bazında): "service-categories:type:{type}:page:{page}"
    /// </summary>
    public static string ServiceCategoriesByType(string type, int page, int pageSize)
        => $"service-categories:type:{type}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Aktif hizmet kategorileri (tür bazında): "service-categories:active:type:{type}"
    /// </summary>
    public static string ActiveServiceCategoriesByType(string type)
        => $"service-categories:active:type:{type}";

    /// <summary>
    /// Tüm hizmet kategorileri pattern: "service-categor*"
    /// </summary>
    public static string AllServiceCategoriesPattern() => "service-categor*";

    #endregion

    #region Special Holidays

    /// <summary>
    /// Tüm özel tatiller: "holidays:all"
    /// </summary>
    public static string AllSpecialHolidays() => "holidays:all";

    /// <summary>
    /// Özel tatiller (satıcı bazında): "holidays:merchant:{merchantId}"
    /// </summary>
    public static string SpecialHolidaysByMerchant(Guid merchantId) => $"holidays:merchant:{merchantId}";

    /// <summary>
    /// Özel tatiller (tarih aralığı bazında): "holidays:merchant:{merchantId}:{start}:{end}"
    /// </summary>
    public static string SpecialHolidaysByDateRange(Guid merchantId, DateTime startDate, DateTime endDate)
        => $"holidays:merchant:{merchantId}:{startDate:yyyy-MM-dd}:{endDate:yyyy-MM-dd}";

    /// <summary>
    /// Tek bir özel tatil: "holiday:{holidayId}"
    /// </summary>
    public static string SpecialHoliday(Guid holidayId) => $"holiday:{holidayId}";

    /// <summary>
    /// Yaklaşan özel tatiller: "holidays:upcoming:merchant:{merchantId}"
    /// </summary>
    public static string UpcomingHolidays(Guid merchantId) => $"holidays:upcoming:merchant:{merchantId}";

    /// <summary>
    /// Tüm özel tatiller pattern: "holiday*"
    /// </summary>
    public static string AllHolidaysPattern() => "holiday*";

    #endregion

    #region Reviews & Ratings

    /// <summary>
    /// Incelemeler (varlık bazında): "reviews:{entityType}:{entityId}:page:{page}"
    /// </summary>
    public static string ReviewsByEntity(string entityType, Guid entityId, int page, int pageSize)
        => $"reviews:{entityType}:{entityId}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Puan istatistikleri: "rating:stats:{entityType}:{entityId}"
    /// </summary>
    public static string RatingStats(string entityType, Guid entityId)
        => $"rating:stats:{entityType}:{entityId}";

    /// <summary>
    /// Tüm incelemeler pattern (varlık bazında): "reviews:{entityType}:{entityId}:*"
    /// </summary>
    public static string AllReviewsByEntity(string entityType, Guid entityId)
        => $"reviews:{entityType}:{entityId}:*";

    #endregion

    #region Languages & Translations

    /// <summary>
    /// Tüm çeviriler (dil bazında): "translations:{languageCode}:all"
    /// </summary>
    public static string AllTranslations(string languageCode) => $"translations:{languageCode}:all";

    /// <summary>
    /// Tek bir çeviri: "translation:{languageCode}:{key}"
    /// </summary>
    public static string Translation(string languageCode, string key)
        => $"translation:{languageCode}:{key.Replace(":", "-")}";

    /// <summary>
    /// Desteklenen diller: "languages:supported"
    /// </summary>
    public static string SupportedLanguages() => "languages:supported";

    /// <summary>
    /// Tüm çeviriler pattern: "translation*"
    /// </summary>
    public static string AllTranslationsPattern() => "translation*";

    #endregion

    #region Campaigns

    /// <summary>
    /// Aktif kampanyalar: "campaigns:active:page:{page}"
    /// </summary>
    public static string ActiveCampaigns(int page, int pageSize)
        => $"campaigns:active:page:{page}:size:{pageSize}";

    /// <summary>
    /// Kampanya (id bazında): "campaign:{campaignId}"
    /// </summary>
    public static string Campaign(Guid campaignId) => $"campaign:{campaignId}";

    /// <summary>
    /// Tüm kampanyalar pattern: "campaign*"
    /// </summary>
    public static string AllCampaignsPattern() => "campaign*";

    #endregion

    #region Working Hours

    /// <summary>
    /// Satıcının çalışma saatleri: "working-hours:merchant:{merchantId}"
    /// </summary>
    public static string WorkingHoursByMerchant(Guid merchantId) => $"working-hours:merchant:{merchantId}";

    /// <summary>
    /// Tüm çalışma saatleri pattern: "working-hours:*"
    /// </summary>
    public static string AllWorkingHoursPattern() => "working-hours:*";

    #endregion
}

