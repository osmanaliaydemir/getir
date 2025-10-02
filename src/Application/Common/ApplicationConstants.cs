namespace Getir.Application.Common;

/// <summary>
/// Application-wide constants to eliminate magic numbers and improve maintainability
/// </summary>
public static class ApplicationConstants
{
    #region Request Limits
    /// <summary>
    /// Maximum request body size in bytes (10MB)
    /// </summary>
    public const int MaxRequestSizeBytes = 10 * 1024 * 1024;
    
    /// <summary>
    /// Maximum number of items in a single order
    /// </summary>
    public const int MaxOrderItems = 50;
    
    /// <summary>
    /// Maximum number of products in a single request
    /// </summary>
    public const int MaxProductsPerRequest = 100;
    
    /// <summary>
    /// Maximum number of reviews per page
    /// </summary>
    public const int MaxReviewsPerPage = 100;
    #endregion

    #region Pagination
    /// <summary>
    /// Default page size for paginated results
    /// </summary>
    public const int DefaultPageSize = 20;
    
    /// <summary>
    /// Maximum page size for paginated results
    /// </summary>
    public const int MaxPageSize = 100;
    
    /// <summary>
    /// Minimum page size for paginated results
    /// </summary>
    public const int MinPageSize = 1;
    #endregion

    #region Cache
    /// <summary>
    /// Default cache duration in minutes
    /// </summary>
    public const int DefaultCacheMinutes = 30;
    
    /// <summary>
    /// Long-term cache duration in minutes
    /// </summary>
    public const int LongCacheMinutes = 60;
    
    /// <summary>
    /// Short-term cache duration in minutes
    /// </summary>
    public const int ShortCacheMinutes = 10;
    
    /// <summary>
    /// Background task queue capacity
    /// </summary>
    public const int BackgroundTaskQueueCapacity = 1000;
    #endregion

    #region Validation Limits
    /// <summary>
    /// Maximum length for names (first name, last name, etc.)
    /// </summary>
    public const int MaxNameLength = 100;
    
    /// <summary>
    /// Maximum length for email addresses
    /// </summary>
    public const int MaxEmailLength = 256;
    
    /// <summary>
    /// Maximum length for descriptions
    /// </summary>
    public const int MaxDescriptionLength = 500;
    
    /// <summary>
    /// Maximum length for comments
    /// </summary>
    public const int MaxCommentLength = 1000;
    
    /// <summary>
    /// Maximum length for phone numbers
    /// </summary>
    public const int MaxPhoneNumberLength = 20;
    
    /// <summary>
    /// Maximum length for addresses
    /// </summary>
    public const int MaxAddressLength = 500;
    
    /// <summary>
    /// Maximum length for product names
    /// </summary>
    public const int MaxProductNameLength = 200;
    
    /// <summary>
    /// Maximum length for merchant names
    /// </summary>
    public const int MaxMerchantNameLength = 200;
    
    /// <summary>
    /// Maximum length for category names
    /// </summary>
    public const int MaxCategoryNameLength = 200;
    
    /// <summary>
    /// Maximum length for URLs
    /// </summary>
    public const int MaxUrlLength = 500;
    
    /// <summary>
    /// Maximum quantity for products
    /// </summary>
    public const int MaxQuantity = 1000;
    
    /// <summary>
    /// Maximum rating value
    /// </summary>
    public const int MaxRating = 5;
    
    /// <summary>
    /// Minimum rating value
    /// </summary>
    public const int MinRating = 1;
    #endregion

    #region Business Rules
    /// <summary>
    /// Minimum order amount in currency
    /// </summary>
    public const decimal MinOrderAmount = 10.00m;
    
    /// <summary>
    /// Maximum discount percentage
    /// </summary>
    public const decimal MaxDiscountPercentage = 50.0m;
    
    /// <summary>
    /// Maximum retry attempts for operations
    /// </summary>
    public const int MaxRetryAttempts = 3;
    
    /// <summary>
    /// Default average delivery time in minutes
    /// </summary>
    public const int DefaultDeliveryTimeMinutes = 30;
    
    /// <summary>
    /// Maximum delivery time in minutes
    /// </summary>
    public const int MaxDeliveryTimeMinutes = 120;
    
    /// <summary>
    /// Minimum delivery time in minutes
    /// </summary>
    public const int MinDeliveryTimeMinutes = 5;
    #endregion

    #region Performance
    /// <summary>
    /// Database command timeout in seconds
    /// </summary>
    public const int DatabaseCommandTimeoutSeconds = 30;
    
    /// <summary>
    /// SignalR client timeout in seconds
    /// </summary>
    public const int SignalRClientTimeoutSeconds = 30;
    
    /// <summary>
    /// Performance warning threshold in milliseconds
    /// </summary>
    public const int PerformanceWarningThresholdMs = 1000;
    
    /// <summary>
    /// Maximum number of recent items to display
    /// </summary>
    public const int MaxRecentItems = 10;
    
    /// <summary>
    /// Maximum number of top items to display
    /// </summary>
    public const int MaxTopItems = 10;
    #endregion

    #region Time Periods
    /// <summary>
    /// Number of days for recent data analysis
    /// </summary>
    public const int RecentDataDays = 30;
    
    /// <summary>
    /// Number of days for monthly analysis
    /// </summary>
    public const int MonthlyAnalysisDays = 30;
    
    /// <summary>
    /// Number of days for weekly analysis
    /// </summary>
    public const int WeeklyAnalysisDays = 7;
    
    /// <summary>
    /// Number of days for daily analysis
    /// </summary>
    public const int DailyAnalysisDays = 1;
    #endregion

    #region Geographic
    /// <summary>
    /// Default latitude precision
    /// </summary>
    public const int LatitudePrecision = 10;
    
    /// <summary>
    /// Default longitude precision
    /// </summary>
    public const int LongitudePrecision = 8;
    
    /// <summary>
    /// Default average courier speed in km/h
    /// </summary>
    public const int DefaultCourierSpeedKmh = 30;
    #endregion

    #region Percentages
    /// <summary>
    /// Minimum disk space percentage before warning
    /// </summary>
    public const int MinDiskSpacePercentage = 10;
    
    /// <summary>
    /// Maximum completion rate percentage
    /// </summary>
    public const int MaxCompletionRatePercentage = 100;
    
    /// <summary>
    /// Default positive review rate percentage
    /// </summary>
    public const int DefaultPositiveReviewRatePercentage = 80;
    #endregion

    #region Background Tasks
    /// <summary>
    /// Background task delay in milliseconds
    /// </summary>
    public const int BackgroundTaskDelayMs = 100;
    
    /// <summary>
    /// Maximum background task execution time in minutes
    /// </summary>
    public const int MaxBackgroundTaskExecutionMinutes = 30;
    #endregion

    #region File Upload
    /// <summary>
    /// Maximum file size in bytes (5MB)
    /// </summary>
    public const int MaxFileSizeBytes = 5 * 1024 * 1024;
    
    /// <summary>
    /// Allowed image file extensions
    /// </summary>
    public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    
    /// <summary>
    /// Allowed document file extensions
    /// </summary>
    public static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".txt" };
    #endregion

    #region Security
    /// <summary>
    /// Minimum password length
    /// </summary>
    public const int MinPasswordLength = 6;
    
    /// <summary>
    /// Maximum password length
    /// </summary>
    public const int MaxPasswordLength = 100;
    
    /// <summary>
    /// JWT token expiration in minutes
    /// </summary>
    public const int JwtTokenExpirationMinutes = 60;
    
    /// <summary>
    /// Refresh token expiration in minutes (7 days)
    /// </summary>
    public const int RefreshTokenExpirationMinutes = 10080;
    #endregion

    #region Database
    /// <summary>
    /// Maximum retry count for database operations
    /// </summary>
    public const int MaxDatabaseRetryCount = 5;
    
    /// <summary>
    /// Maximum retry delay in seconds
    /// </summary>
    public const int MaxDatabaseRetryDelaySeconds = 30;
    
    /// <summary>
    /// Default database connection timeout in seconds
    /// </summary>
    public const int DefaultDatabaseConnectionTimeoutSeconds = 30;
    #endregion

    #region API
    /// <summary>
    /// Default API version
    /// </summary>
    public const string DefaultApiVersion = "v1";
    
    /// <summary>
    /// Maximum API response size in bytes
    /// </summary>
    public const int MaxApiResponseSizeBytes = 1024 * 1024; // 1MB
    #endregion
}
