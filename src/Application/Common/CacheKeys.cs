namespace Getir.Application.Common;

/// <summary>
/// Centralized cache key naming strategy
/// Follows Redis best practices: namespace:entity:identifier
/// </summary>
public static class CacheKeys
{
    // Time-to-Live constants (in minutes)
    public static class TTL
    {
        public const int VeryShort = 2;      // 2 minutes - for volatile data
        public const int Short = 5;          // 5 minutes - for dynamic data
        public const int Medium = 15;        // 15 minutes - for semi-static data
        public const int Long = 30;          // 30 minutes - for static data
        public const int VeryLong = 60;      // 1 hour - for rarely changing data
        public const int ExtraLong = 240;    // 4 hours - for configuration data
    }

    #region Products

    /// <summary>
    /// Single product by ID: "product:123e4567-e89b-12d3-a456-426614174000"
    /// </summary>
    public static string Product(Guid productId) => $"product:{productId}";

    /// <summary>
    /// Products by merchant with pagination: "products:merchant:123e4567:page:1:size:20"
    /// </summary>
    public static string ProductsByMerchant(Guid merchantId, int page, int pageSize)
        => $"products:merchant:{merchantId}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Products by category with pagination: "products:category:123e4567:page:1:size:20"
    /// </summary>
    public static string ProductsByCategory(Guid categoryId, int page, int pageSize)
        => $"products:category:{categoryId}:page:{page}:size:{pageSize}";

    /// <summary>
    /// All products pattern for invalidation: "product:*" or "products:merchant:123e4567:*"
    /// </summary>
    public static string AllProducts() => "product:*";
    public static string AllProductsByMerchant(Guid merchantId) => $"products:merchant:{merchantId}:*";

    #endregion

    #region Product Categories

    /// <summary>
    /// Single category: "category:123e4567-e89b-12d3-a456-426614174000"
    /// </summary>
    public static string ProductCategory(Guid categoryId) => $"category:{categoryId}";

    /// <summary>
    /// All categories list: "categories:all"
    /// </summary>
    public static string AllProductCategories() => "categories:all";

    /// <summary>
    /// Categories by merchant: "categories:merchant:123e4567"
    /// </summary>
    public static string CategoriesByMerchant(Guid merchantId) => $"categories:merchant:{merchantId}";

    /// <summary>
    /// All categories pattern: "category:*" or "categories:*"
    /// </summary>
    public static string AllCategoriesPattern() => "categor*";

    #endregion

    #region Merchants

    /// <summary>
    /// Single merchant: "merchant:123e4567-e89b-12d3-a456-426614174000"
    /// </summary>
    public static string Merchant(Guid merchantId) => $"merchant:{merchantId}";

    /// <summary>
    /// Merchant by owner ID: "merchant:owner:123e4567"
    /// </summary>
    public static string MerchantByOwner(Guid ownerId) => $"merchant:owner:{ownerId}";

    /// <summary>
    /// Merchants in delivery zone: "merchants:zone:123e4567:page:1"
    /// </summary>
    public static string MerchantsByZone(Guid zoneId, int page, int pageSize)
        => $"merchants:zone:{zoneId}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Active merchants: "merchants:active:page:1:size:20"
    /// </summary>
    public static string ActiveMerchants(int page, int pageSize)
        => $"merchants:active:page:{page}:size:{pageSize}";

    /// <summary>
    /// All merchants pattern: "merchant:*" or "merchants:*"
    /// </summary>
    public static string AllMerchants() => "merchant*";

    #endregion

    #region Delivery Zones

    /// <summary>
    /// All delivery zones: "zones:all"
    /// </summary>
    public static string AllDeliveryZones() => "zones:all";

    /// <summary>
    /// Active delivery zones: "zones:active"
    /// </summary>
    public static string ActiveDeliveryZones() => "zones:active";

    /// <summary>
    /// Zones by coordinates (geo-cache): "zones:geo:lat:41.0082:lon:28.9784"
    /// </summary>
    public static string ZonesByCoordinates(double latitude, double longitude)
        => $"zones:geo:lat:{latitude:F4}:lon:{longitude:F4}";

    /// <summary>
    /// Delivery zones by merchant: "zones:merchant:{merchantId}"
    /// </summary>
    public static string DeliveryZonesByMerchant(Guid merchantId) => $"zones:merchant:{merchantId}";

    /// <summary>
    /// Single delivery zone: "zone:{zoneId}"
    /// </summary>
    public static string DeliveryZone(Guid zoneId) => $"zone:{zoneId}";

    /// <summary>
    /// All delivery zones pattern: "zone*"
    /// </summary>
    public static string AllDeliveryZonesPattern() => "zone*";

    #endregion

    #region Reviews

    /// <summary>
    /// Product reviews with pagination: "reviews:product:123e4567:page:1:size:10"
    /// </summary>
    public static string ProductReviews(Guid productId, int page, int pageSize)
        => $"reviews:product:{productId}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Merchant reviews: "reviews:merchant:123e4567:page:1:size:10"
    /// </summary>
    public static string MerchantReviews(Guid merchantId, int page, int pageSize)
        => $"reviews:merchant:{merchantId}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Product average rating: "rating:product:123e4567"
    /// </summary>
    public static string ProductRating(Guid productId) => $"rating:product:{productId}";

    #endregion

    #region Search

    /// <summary>
    /// Search results: "search:query:pizza:location:123e4567:page:1"
    /// </summary>
    public static string SearchResults(string query, Guid? locationId, int page, int pageSize)
        => $"search:query:{NormalizeSearchQuery(query)}:location:{locationId?.ToString() ?? "all"}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Popular searches: "search:popular:limit:10"
    /// </summary>
    public static string PopularSearches(int limit) => $"search:popular:limit:{limit}";

    /// <summary>
    /// Clear all search cache: "search:*"
    /// </summary>
    public static string AllSearchResults() => "search:*";

    private static string NormalizeSearchQuery(string query)
        => query.ToLowerInvariant().Replace(" ", "-").Replace(":", "");

    #endregion

    #region Cart (Short TTL)

    /// <summary>
    /// User cart: "cart:user:123e4567-e89b-12d3-a456-426614174000"
    /// WARNING: Use with caution, short TTL recommended
    /// </summary>
    public static string UserCart(Guid userId) => $"cart:user:{userId}";

    #endregion

    #region Coupons

    /// <summary>
    /// Available coupons list: "coupons:available:page:1:size:20"
    /// </summary>
    public static string AvailableCoupons(int page, int pageSize)
        => $"coupons:available:page:{page}:size:{pageSize}";

    /// <summary>
    /// User's coupons: "coupons:user:123e4567"
    /// </summary>
    public static string UserCoupons(Guid userId) => $"coupons:user:{userId}";

    #endregion

    #region Working Hours

    /// <summary>
    /// Merchant working hours: "hours:merchant:123e4567"
    /// </summary>
    public static string MerchantWorkingHours(Guid merchantId) => $"hours:merchant:{merchantId}";

    #endregion

    #region Statistics & Analytics

    /// <summary>
    /// Merchant dashboard stats: "stats:merchant:123e4567:date:2025-01-15"
    /// </summary>
    public static string MerchantStats(Guid merchantId, DateTime date)
        => $"stats:merchant:{merchantId}:date:{date:yyyy-MM-dd}";

    /// <summary>
    /// Platform-wide statistics: "stats:platform:date:2025-01-15"
    /// </summary>
    public static string PlatformStats(DateTime date)
        => $"stats:platform:date:{date:yyyy-MM-dd}";

    #endregion

    #region User Preferences

    /// <summary>
    /// User language preference: "pref:user:123e4567:language"
    /// </summary>
    public static string UserLanguagePreference(Guid userId) => $"pref:user:{userId}:language";

    /// <summary>
    /// User notification preferences: "pref:user:123e4567:notifications"
    /// </summary>
    public static string UserNotificationPreferences(Guid userId) => $"pref:user:{userId}:notifications";

    #endregion

    #region System Configuration

    /// <summary>
    /// System settings: "config:system:setting-name"
    /// </summary>
    public static string SystemConfig(string settingName) => $"config:system:{settingName}";

    /// <summary>
    /// Feature flags: "config:feature:{featureName}"
    /// </summary>
    public static string FeatureFlag(string featureName) => $"config:feature:{featureName}";

    #endregion

    #region Service Categories

    /// <summary>
    /// All service categories: "service-categories:all:page:{page}:size:{size}"
    /// </summary>
    public static string AllServiceCategories(int page, int pageSize)
        => $"service-categories:all:page:{page}:size:{pageSize}";

    /// <summary>
    /// Single service category: "service-category:{categoryId}"
    /// </summary>
    public static string ServiceCategory(Guid categoryId) => $"service-category:{categoryId}";

    /// <summary>
    /// Service categories by type: "service-categories:type:{type}:page:{page}"
    /// </summary>
    public static string ServiceCategoriesByType(string type, int page, int pageSize)
        => $"service-categories:type:{type}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Active service categories by type: "service-categories:active:type:{type}"
    /// </summary>
    public static string ActiveServiceCategoriesByType(string type)
        => $"service-categories:active:type:{type}";

    /// <summary>
    /// All service categories pattern: "service-categor*"
    /// </summary>
    public static string AllServiceCategoriesPattern() => "service-categor*";

    #endregion

    #region Special Holidays

    /// <summary>
    /// All special holidays: "holidays:all"
    /// </summary>
    public static string AllSpecialHolidays() => "holidays:all";

    /// <summary>
    /// Special holidays by merchant: "holidays:merchant:{merchantId}"
    /// </summary>
    public static string SpecialHolidaysByMerchant(Guid merchantId) => $"holidays:merchant:{merchantId}";

    /// <summary>
    /// Special holidays by date range: "holidays:merchant:{merchantId}:{start}:{end}"
    /// </summary>
    public static string SpecialHolidaysByDateRange(Guid merchantId, DateTime startDate, DateTime endDate)
        => $"holidays:merchant:{merchantId}:{startDate:yyyy-MM-dd}:{endDate:yyyy-MM-dd}";

    /// <summary>
    /// Single special holiday: "holiday:{holidayId}"
    /// </summary>
    public static string SpecialHoliday(Guid holidayId) => $"holiday:{holidayId}";

    /// <summary>
    /// Upcoming holidays: "holidays:upcoming:merchant:{merchantId}"
    /// </summary>
    public static string UpcomingHolidays(Guid merchantId) => $"holidays:upcoming:merchant:{merchantId}";

    /// <summary>
    /// All holidays pattern: "holiday*"
    /// </summary>
    public static string AllHolidaysPattern() => "holiday*";

    #endregion

    #region Reviews & Ratings

    /// <summary>
    /// Reviews by entity: "reviews:{entityType}:{entityId}:page:{page}"
    /// </summary>
    public static string ReviewsByEntity(string entityType, Guid entityId, int page, int pageSize)
        => $"reviews:{entityType}:{entityId}:page:{page}:size:{pageSize}";

    /// <summary>
    /// Rating stats: "rating:stats:{entityType}:{entityId}"
    /// </summary>
    public static string RatingStats(string entityType, Guid entityId)
        => $"rating:stats:{entityType}:{entityId}";

    /// <summary>
    /// All reviews pattern for entity: "reviews:{entityType}:{entityId}:*"
    /// </summary>
    public static string AllReviewsByEntity(string entityType, Guid entityId)
        => $"reviews:{entityType}:{entityId}:*";

    #endregion

    #region Languages & Translations

    /// <summary>
    /// All translations for language: "translations:{languageCode}:all"
    /// </summary>
    public static string AllTranslations(string languageCode) => $"translations:{languageCode}:all";

    /// <summary>
    /// Single translation: "translation:{languageCode}:{key}"
    /// </summary>
    public static string Translation(string languageCode, string key)
        => $"translation:{languageCode}:{key.Replace(":", "-")}";

    /// <summary>
    /// Supported languages: "languages:supported"
    /// </summary>
    public static string SupportedLanguages() => "languages:supported";

    /// <summary>
    /// All translations pattern: "translation*"
    /// </summary>
    public static string AllTranslationsPattern() => "translation*";

    #endregion

    #region Campaigns

    /// <summary>
    /// Active campaigns: "campaigns:active:page:{page}"
    /// </summary>
    public static string ActiveCampaigns(int page, int pageSize)
        => $"campaigns:active:page:{page}:size:{pageSize}";

    /// <summary>
    /// Campaign by id: "campaign:{campaignId}"
    /// </summary>
    public static string Campaign(Guid campaignId) => $"campaign:{campaignId}";

    /// <summary>
    /// All campaigns pattern: "campaign*"
    /// </summary>
    public static string AllCampaignsPattern() => "campaign*";

    #endregion

    #region Working Hours

    /// <summary>
    /// Working hours by merchant: "working-hours:merchant:{merchantId}"
    /// </summary>
    public static string WorkingHoursByMerchant(Guid merchantId) => $"working-hours:merchant:{merchantId}";

    /// <summary>
    /// All working hours pattern: "working-hours:*"
    /// </summary>
    public static string AllWorkingHoursPattern() => "working-hours:*";

    #endregion
}

