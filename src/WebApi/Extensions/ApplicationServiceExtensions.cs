using Getir.Application.Abstractions;
using Getir.Application.Services.Addresses;
using Getir.Application.Services.Admin;
using Getir.Application.Services.AuditLogging;
using Getir.Application.Services.Auth;
using Getir.Application.Services.Campaigns;
using Getir.Application.Services.Cart;
using Getir.Application.Services.Coupons;
using Getir.Application.Services.Couriers;
using Getir.Application.Services.DeliveryOptimization;
using Getir.Application.Services.DeliveryZones;
using Getir.Application.Services.Favorites;
using Getir.Application.Services.FileUpload;
using Getir.Application.Services.GeoLocation;
using Getir.Application.Services.Internationalization;
using Getir.Application.Services.Merchants;
using Getir.Application.Services.Notifications;
using Getir.Application.Services.Orders;
using Getir.Application.Services.Payments;
using Getir.Application.Services.ProductCategories;
using Getir.Application.Services.ProductOptions;
using Getir.Application.Services.Products;
using Getir.Application.Services.ProductReviews;
using Getir.Application.Services.RateLimiting;
using Getir.Application.Services.RealtimeTracking;
using Getir.Application.Services.Reviews;
using Getir.Application.Services.Search;
using Getir.Application.Services.ServiceCategories;
using Getir.Application.Services.SpecialHolidays;
using Getir.Application.Services.Stock;
using Getir.Application.Services.UserPreferences;
using Getir.Application.Services.WorkingHours;
using Getir.Infrastructure.Services.Notifications;

namespace Getir.WebApi.Extensions;

/// <summary>
/// Extension methods for registering Application layer services
/// This keeps Program.cs clean and maintainable
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Registers all Application layer services with dependency injection
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Core Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminService, AdminService>();
        
        // Category Services
        services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        
        // Merchant Services
        services.AddScoped<IMerchantService, MerchantService>();
        services.AddScoped<IMerchantDashboardService, MerchantDashboardService>();
        services.AddScoped<IMerchantOnboardingService, MerchantOnboardingService>();
        
        // Product Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductOptionGroupService, ProductOptionGroupService>();
        services.AddScoped<IProductOptionService, ProductOptionService>();
        
        // Order Services
        services.AddScoped<IOrderService, OrderService>();
        
        // Favorites Services
        services.AddScoped<IFavoritesService, FavoritesService>();
        
        // Address Services
        services.AddScoped<IUserAddressService, UserAddressService>();
        
        // Shopping Cart Services
        services.AddScoped<ICartService, CartService>();
        
        // Coupon & Campaign Services
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<ICampaignService, CampaignService>();
        
        // Notification Services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationPreferencesService, NotificationPreferencesService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IPushNotificationService, PushNotificationService>();
        
        // User Services
        services.AddScoped<IUserPreferencesService, UserPreferencesService>();
        
        // Courier Services
        services.AddScoped<ICourierService, CourierService>();
        
        // Search Services
        services.AddScoped<ISearchService, SearchService>();
        
        // Working Hours & Holidays
        services.AddScoped<IWorkingHoursService, WorkingHoursService>();
        services.AddScoped<ISpecialHolidayService, SpecialHolidayService>();
        
        // Delivery Services
        services.AddScoped<IDeliveryZoneService, DeliveryZoneService>();
        services.AddScoped<IDeliveryCapacityService, DeliveryCapacityService>();
        services.AddScoped<IRouteOptimizationService, RouteOptimizationService>();
        
        // Audit Logging Services
        services.AddScoped<IUserActivityLogService, UserActivityLogService>();
        services.AddScoped<ISystemChangeLogService, SystemChangeLogService>();
        services.AddScoped<ISecurityEventLogService, SecurityEventLogService>();
        services.AddScoped<ILogAnalysisService, LogAnalysisService>();
        
        // Internationalization Services
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<ITranslationService, TranslationService>();
        services.AddScoped<IUserLanguageService, UserLanguageService>();
        
        // Rate Limiting Services
        services.AddScoped<IRateLimitService, RateLimitService>();
        services.AddScoped<IRateLimitConfigurationService, RateLimitConfigurationService>();
        services.AddScoped<IRateLimitMonitoringService, RateLimitMonitoringService>();
        
        // Realtime Tracking Services
        services.AddScoped<IOrderTrackingService, OrderTrackingService>();
        services.AddScoped<IETAEstimationService, ETAEstimationService>();
        services.AddScoped<ITrackingNotificationService, TrackingNotificationService>();
        services.AddScoped<ITrackingSettingsService, TrackingSettingsService>();
        services.AddScoped<IRealtimeTrackingService, RealtimeTrackingService>();
        
        // Stock Management Services
        services.AddScoped<IStockManagementService, StockManagementService>();
        services.AddScoped<IStockAlertService, StockAlertService>();
        services.AddScoped<IStockSyncService, StockSyncService>();
        services.AddScoped<IInventoryService, InventoryService>();
        
        // Order Support Services
        services.AddScoped<IOrderStatusValidatorService, OrderStatusValidatorService>();
        services.AddScoped<IOrderStatusTransitionService, OrderStatusTransitionService>();
        
        // Merchant Support Services
        services.AddScoped<IMerchantDocumentService, MerchantDocumentService>();
        
        // Product Support Services
        services.AddScoped<IMarketProductVariantService, MarketProductVariantService>();
        
        // Notification Support Services
        services.AddScoped<INotificationHistoryService, NotificationHistoryService>();
        
        // Payment Services
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ICashPaymentSecurityService, CashPaymentSecurityService>();
        services.AddScoped<ICashPaymentAuditService, CashPaymentAuditService>();
        
        // GeoLocation Services
        services.AddScoped<IGeoLocationService, GeoLocationService>();
        
        // File Upload Services
        services.AddScoped<IFileUploadIntegrationService, FileUploadIntegrationService>();
        
        // Review Services
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IProductReviewService, ProductReviewService>();
        
        return services;
    }
}

