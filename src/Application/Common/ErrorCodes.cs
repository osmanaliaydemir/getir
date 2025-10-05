namespace Getir.Application.Common;

/// <summary>
/// Uygulama genelinde kullanılan error code'ları
/// </summary>
public static class ErrorCodes
{
    // General errors
    public const string INTERNAL_ERROR = "INTERNAL_ERROR";
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string NOT_FOUND = "NOT_FOUND";
    public const string UNAUTHORIZED = "UNAUTHORIZED";
    public const string FORBIDDEN = "FORBIDDEN";
    public const string CONFLICT = "CONFLICT";
    public const string BAD_REQUEST = "BAD_REQUEST";
    
    // Transaction errors
    public const string TRANSACTION_ERROR = "TRANSACTION_ERROR";
    public const string CONCURRENCY_ERROR = "CONCURRENCY_ERROR";
    
    // Authentication errors
    public const string AUTH_EMAIL_EXISTS = "AUTH_EMAIL_EXISTS";
    public const string AUTH_INVALID_CREDENTIALS = "AUTH_INVALID_CREDENTIALS";
    public const string AUTH_ACCOUNT_DEACTIVATED = "AUTH_ACCOUNT_DEACTIVATED";
    public const string AUTH_INVALID_REFRESH_TOKEN = "AUTH_INVALID_REFRESH_TOKEN";
    public const string AUTH_REFRESH_TOKEN_EXPIRED = "AUTH_REFRESH_TOKEN_EXPIRED";
    
    // Business logic errors
    public const string INSUFFICIENT_STOCK = "INSUFFICIENT_STOCK";
    public const string BELOW_MINIMUM_ORDER = "BELOW_MINIMUM_ORDER";
    public const string ORDER_NOT_FOUND = "ORDER_NOT_FOUND";
    public const string MERCHANT_NOT_FOUND = "MERCHANT_NOT_FOUND";
    public const string PRODUCT_NOT_FOUND = "PRODUCT_NOT_FOUND";
    public const string PRODUCT_VARIANT_NOT_FOUND = "PRODUCT_VARIANT_NOT_FOUND";
    public const string COUPON_NOT_FOUND = "COUPON_NOT_FOUND";
    public const string COUPON_EXPIRED = "COUPON_EXPIRED";
    public const string COUPON_USAGE_LIMIT_REACHED = "COUPON_USAGE_LIMIT_REACHED";
    public const string CART_DIFFERENT_MERCHANT = "CART_DIFFERENT_MERCHANT";
    public const string ALREADY_REVIEWED = "ALREADY_REVIEWED";
    public const string CANNOT_REVIEW = "CANNOT_REVIEW";
    
    // System errors
    public const string EXTERNAL_SERVICE_ERROR = "EXTERNAL_SERVICE_ERROR";
    public const string RATE_LIMIT_EXCEEDED = "RATE_LIMIT_EXCEEDED";
    public const string SERVICE_UNAVAILABLE = "SERVICE_UNAVAILABLE";
}
