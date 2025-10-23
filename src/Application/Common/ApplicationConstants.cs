namespace Getir.Application.Common;

/// <summary>
/// Application-wide constants to eliminate magic numbers and improve maintainability
/// </summary>
public static class ApplicationConstants
{
    #region Request Limits
    /// <summary>
    /// Maximum istek boyutu (10MB)
    /// </summary>
    public const int MaxRequestSizeBytes = 10 * 1024 * 1024;
    
    /// <summary>
    /// Maximum sipariş item sayısı
    /// </summary>
    public const int MaxOrderItems = 50;
    
    /// <summary>
    /// Maximum ürün sayısı
    /// </summary>
    public const int MaxProductsPerRequest = 100;
    
    /// <summary>
    /// Maximum yorum sayısı
    /// </summary>
    public const int MaxReviewsPerPage = 100;
    #endregion

    #region Pagination
    /// <summary>
    /// Default sayfa boyutu
    /// </summary>
    public const int DefaultPageSize = 20;
    
    /// <summary>
    /// Maximum sayfa boyutu
    /// </summary>
    public const int MaxPageSize = 100;
    
    /// <summary>
    /// Minimum sayfa boyutu
    /// </summary>
    public const int MinPageSize = 1;
    #endregion

    #region Cache
    /// <summary>
    /// Default cache süresi (dakika)
    /// </summary>
    public const int DefaultCacheMinutes = 30;
    
    /// <summary>
    /// Long-term cache süresi (dakika)
    /// </summary>
    public const int LongCacheMinutes = 60;
    
    /// <summary>
    /// Short-term cache süresi (dakika)
    /// </summary>
    public const int ShortCacheMinutes = 10;
    
    /// <summary>
    /// Background task queue kapasitesi
    /// </summary>
    public const int BackgroundTaskQueueCapacity = 1000;
    #endregion

    #region Validation Limits
    /// <summary>
    /// Maximum ad uzunluğu (ad, soyad, vb.)
    /// </summary>
    public const int MaxNameLength = 100;
    
    /// <summary>
    /// Maximum email adresi uzunluğu
    /// </summary>
    public const int MaxEmailLength = 256;
    
    /// <summary>
    /// Maximum açıklama uzunluğu
    /// </summary>
    public const int MaxDescriptionLength = 500;
    
    /// <summary>
    /// Maximum yorum uzunluğu
    /// </summary>
    public const int MaxCommentLength = 1000;
    
    /// <summary>
    /// Maximum telefon numarası uzunluğu
    /// </summary>
    public const int MaxPhoneNumberLength = 20;
    
    /// <summary>
    /// Maximum adres uzunluğu
    /// </summary>
    public const int MaxAddressLength = 500;
    
    /// <summary>
    /// Maximum ürün adı uzunluğu
    /// </summary>
    public const int MaxProductNameLength = 200;
    
    /// <summary>
    /// Maximum merchant adı uzunluğu
    /// </summary>
    public const int MaxMerchantNameLength = 200;
    
    /// <summary>
    /// Maximum kategori adı uzunluğu
    /// </summary>
    public const int MaxCategoryNameLength = 200;
    
    /// <summary>
    /// Maximum URL uzunluğu
    /// </summary>
    public const int MaxUrlLength = 500;
    
    /// <summary>
    /// Maximum ürün miktarı
    /// </summary>
    public const int MaxQuantity = 1000;
    
    /// <summary>
    /// Maximum puan değeri
    /// </summary>
    public const int MaxRating = 5;
    
    /// <summary>
    /// Minimum puan değeri
    /// </summary>
    public const int MinRating = 1;
    #endregion

    #region Business Rules
    /// <summary>
    /// Minimum sipariş miktarı (para birimi)
    /// </summary>
    public const decimal MinOrderAmount = 10.00m;
    
    /// <summary>
    /// Maximum indirim yüzdesi
    /// </summary>
    public const decimal MaxDiscountPercentage = 50.0m;
    
    /// <summary>
    /// Maximum retry deneme sayısı
    /// </summary>
    public const int MaxRetryAttempts = 3;
    
    /// <summary>
    /// Default average delivery süresi (dakika)
    /// </summary>
    public const int DefaultDeliveryTimeMinutes = 30;
    
    /// <summary>
    /// Maximum delivery süresi (dakika)
    /// </summary>
    public const int MaxDeliveryTimeMinutes = 120;
    
    /// <summary>
    /// Minimum delivery süresi (dakika)
    /// </summary>
    public const int MinDeliveryTimeMinutes = 5;
    #endregion

    #region Performance
    /// <summary>
    /// Database komut zaman aşımı (saniye)
    /// </summary>
    public const int DatabaseCommandTimeoutSeconds = 120;
    
    /// <summary>
    /// SignalR client zaman aşımı (saniye)
    /// </summary>
    public const int SignalRClientTimeoutSeconds = 30;
    
    /// <summary>
    /// Performans uyarı eşiği (milisaniye)
    /// </summary>
    public const int PerformanceWarningThresholdMs = 1000;
    
    /// <summary>
    /// Maximum yakın zamanda gösterilecek item sayısı
    /// </summary>
    public const int MaxRecentItems = 10;
    
    /// <summary>
    /// Maximum en üstte gösterilecek item sayısı
    /// </summary>
    public const int MaxTopItems = 10;
    #endregion

    #region Time Periods
    /// <summary>
    /// Yakın zamanda analiz için gün sayısı
    /// </summary>
    public const int RecentDataDays = 30;
    
    /// <summary>
    /// Aylık analiz için gün sayısı
    /// </summary>
    public const int MonthlyAnalysisDays = 30;
    
    /// <summary>
    /// Haftalık analiz için gün sayısı
    /// </summary>
    public const int WeeklyAnalysisDays = 7;
    
    /// <summary>
    /// Günlük analiz için gün sayısı
    /// </summary>
    public const int DailyAnalysisDays = 1;
    #endregion

    #region Geographic
    /// <summary>
    /// Default enlem hassasiyeti
    /// </summary>
    public const int LatitudePrecision = 10;
    
    /// <summary>
    /// Default boylam hassasiyeti
    /// </summary>
    public const int LongitudePrecision = 8;
    
    /// <summary>
    /// Default kurye ortalama hızı (km/h)
    /// </summary>
    public const int DefaultCourierSpeedKmh = 30;
    #endregion

    #region Percentages
    /// <summary>
    /// Minimum disk alanı yüzdesi uyarısı
    /// </summary>
    public const int MinDiskSpacePercentage = 10;
    
    /// <summary>
    /// Maximum tamamlama oranı yüzdesi
    /// </summary>
    public const int MaxCompletionRatePercentage = 100;
    
    /// <summary>
    /// Default pozitif yorum oranı yüzdesi
    /// </summary>
    public const int DefaultPositiveReviewRatePercentage = 80;
    #endregion

    #region Background Tasks
    /// <summary>
    /// Background task gecikme süresi (milisaniye)
    /// </summary>
    public const int BackgroundTaskDelayMs = 100;
    
    /// <summary>
    /// Maximum background task çalışma süresi (dakika)
    /// </summary>
    public const int MaxBackgroundTaskExecutionMinutes = 30;
    #endregion

    #region File Upload
    /// <summary>
    /// Maximum dosya boyutu (5MB)
    /// </summary>
    public const int MaxFileSizeBytes = 5 * 1024 * 1024;
    
    /// <summary>
    /// İzin verilen resim dosya uzantıları
    /// </summary>
    public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    
    /// <summary>
    /// İzin verilen belge dosya uzantıları
    /// </summary>
    public static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".txt" };
    #endregion

    #region Security
    /// <summary>
    /// Minimum şifre uzunluğu
    /// </summary>
    public const int MinPasswordLength = 6;
    
    /// <summary>
    /// Maximum şifre uzunluğu
    /// </summary>
    public const int MaxPasswordLength = 100;
    
    /// <summary>
    /// JWT token süresi (dakika)
    /// </summary>
    public const int JwtTokenExpirationMinutes = 60;
    
    /// <summary>
    /// Refresh token süresi (7 gün)
    /// </summary>
    public const int RefreshTokenExpirationMinutes = 10080;
    #endregion

    #region Database
    /// <summary>
    /// Maximum retry deneme sayısı
    /// </summary>
    public const int MaxDatabaseRetryCount = 5;
    
    /// <summary>
    /// Maximum retry gecikme süresi (saniye)
    /// </summary>
    public const int MaxDatabaseRetryDelaySeconds = 30;
    
    /// <summary>
    /// Default database bağlantı zaman aşımı (saniye)
    /// </summary>
    public const int DefaultDatabaseConnectionTimeoutSeconds = 30;
    #endregion

    #region API
    /// <summary>
    /// Default API sürümü
    /// </summary>
    public const string DefaultApiVersion = "v1";
    
    /// <summary>
    /// Maximum API response boyutu (bytes)
    /// </summary>
    public const int MaxApiResponseSizeBytes = 1024 * 1024; // 1MB
    #endregion
}
