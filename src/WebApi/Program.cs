// System namespaces
// Third-party namespaces
using AspNetCoreRateLimit;
using FluentValidation;
// Application namespaces
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Addresses;
using Getir.Application.Services.AuditLogging;
using Getir.Application.Services.Auth;
using Getir.Application.Services.Campaigns;
using Getir.Application.Services.Cart;
using Getir.Application.Services.Coupons;
using Getir.Application.Services.Couriers;
using Getir.Application.Services.DeliveryOptimization;
using Getir.Application.Services.DeliveryZones;
using Getir.Application.Services.Internationalization;
using Getir.Application.Services.Merchants;
using Getir.Application.Services.Notifications;
using Getir.Application.Services.Orders;
using Getir.Application.Services.Payments;
using Getir.Application.Services.ProductCategories;
using Getir.Application.Services.ProductOptions;
using Getir.Application.Services.Products;
using Getir.Application.Services.RateLimiting;
using Getir.Application.Services.RealtimeTracking;
using Getir.Application.Services.Reviews;
using Getir.Application.Services.Search;
using Getir.Application.Services.ServiceCategories;
using Getir.Application.Services.SpecialHolidays;
using Getir.Application.Services.Stock;
using Getir.Application.Services.UserPreferences;
using Getir.Application.Services.WorkingHours;
// Infrastructure namespaces
using Getir.Infrastructure.Persistence;
using Getir.Infrastructure.Persistence.Repositories;
using Getir.Infrastructure.Security;
using Getir.Infrastructure.Services.Notifications;

// WebApi namespaces
using Getir.WebApi.Configuration;
using Getir.WebApi.Middleware;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ============= SERILOG =============
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// ============= DATABASE =============
// Skip DbContext registration in Testing environment (handled by test setup)
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3, // Retry sayısını azalttık
                    maxRetryDelay: TimeSpan.FromSeconds(10), // Retry delay'i azalttık
                    errorNumbersToAdd: null);
                
                // Connection pooling optimization
                sqlOptions.CommandTimeout(ApplicationConstants.DatabaseCommandTimeoutSeconds);
        });

    // Query optimization
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.EnableServiceProviderCaching();
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
    });
}

// ============= DEPENDENCY INJECTION =============
// Infrastructure
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IReadOnlyRepository<>), typeof(ReadOnlyRepository<>));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();

// Framework Adapters (Clean Architecture - keeps Application layer framework-agnostic)
builder.Services.AddScoped<IFileUploadAdapter, Getir.Infrastructure.Adapters.AspNetCoreFileUploadAdapter>();

// Common Services
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddScoped<ICacheService, MemoryCacheService>();
builder.Services.AddScoped<IBackgroundTaskService, BackgroundTaskService>();
builder.Services.AddScoped<IEmailService, Getir.Infrastructure.Services.Notifications.EmailService>();

// Memory Cache
builder.Services.AddMemoryCache();

// Application Insights (temporarily disabled)
// builder.Services.AddApplicationInsightsConfiguration(builder.Configuration);

// CSRF Protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
});

// SignalR Services
builder.Services.AddScoped<ISignalRService, Getir.Infrastructure.SignalR.SignalRService>();
builder.Services.AddScoped<ISignalRNotificationSender>(sp => 
    new Getir.Infrastructure.SignalR.SignalRNotificationSender(
        sp.GetRequiredService<IHubContext<Getir.WebApi.Hubs.NotificationHub>>()));
builder.Services.AddScoped<ISignalROrderSender>(sp => 
    new Getir.Infrastructure.SignalR.SignalROrderSender(
        sp.GetRequiredService<IHubContext<Getir.WebApi.Hubs.OrderHub>>()));
builder.Services.AddScoped<ISignalRCourierSender>(sp => 
    new Getir.Infrastructure.SignalR.SignalRCourierSender(
        sp.GetRequiredService<IHubContext<Getir.WebApi.Hubs.CourierHub>>()));

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserAddressService, UserAddressService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationPreferencesService, NotificationPreferencesService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<ICourierService, CourierService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IWorkingHoursService, WorkingHoursService>();
builder.Services.AddScoped<ISpecialHolidayService, SpecialHolidayService>();
builder.Services.AddScoped<IDeliveryZoneService, DeliveryZoneService>();
builder.Services.AddScoped<IMerchantDashboardService, MerchantDashboardService>();
builder.Services.AddScoped<IMerchantOnboardingService, MerchantOnboardingService>();
builder.Services.AddScoped<IProductOptionGroupService, ProductOptionGroupService>();
builder.Services.AddScoped<IProductOptionService, ProductOptionService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// Delivery Optimization Services
builder.Services.AddScoped<IDeliveryCapacityService, DeliveryCapacityService>();
builder.Services.AddScoped<IRouteOptimizationService, RouteOptimizationService>();

// Audit Logging Services
builder.Services.AddScoped<IUserActivityLogService, UserActivityLogService>();
builder.Services.AddScoped<ISystemChangeLogService, SystemChangeLogService>();
builder.Services.AddScoped<ISecurityEventLogService, SecurityEventLogService>();
builder.Services.AddScoped<ILogAnalysisService, LogAnalysisService>();

// Internationalization Services
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<ITranslationService, TranslationService>();
builder.Services.AddScoped<IUserLanguageService, UserLanguageService>();

// Rate Limiting Services
builder.Services.AddScoped<IRateLimitService, RateLimitService>();
builder.Services.AddScoped<IRateLimitConfigurationService, RateLimitConfigurationService>();
builder.Services.AddScoped<IRateLimitMonitoringService, RateLimitMonitoringService>();

// Realtime Tracking Services
builder.Services.AddScoped<IOrderTrackingService, OrderTrackingService>();
builder.Services.AddScoped<IETAEstimationService, ETAEstimationService>();
builder.Services.AddScoped<ITrackingNotificationService, TrackingNotificationService>();
builder.Services.AddScoped<ITrackingSettingsService, TrackingSettingsService>();
builder.Services.AddScoped<IRealtimeTrackingService, RealtimeTrackingService>();
builder.Services.AddScoped<IStockManagementService, StockManagementService>();

builder.Services.AddScoped<Getir.Application.Services.Admin.IAdminService, Getir.Application.Services.Admin.AdminService>();
builder.Services.AddScoped<Getir.Application.Services.Payments.IPaymentService, Getir.Application.Services.Payments.PaymentService>();
builder.Services.AddScoped<Getir.Application.Services.GeoLocation.IGeoLocationService, Getir.Application.Services.GeoLocation.GeoLocationService>();

// Cash Payment Security Services
builder.Services.AddScoped<ICashPaymentSecurityService, CashPaymentSecurityService>();
builder.Services.AddScoped<ICashPaymentAuditService, CashPaymentAuditService>();

// File Upload Services
builder.Services.AddScoped<IFileStorageService, Getir.Infrastructure.Services.FileStorage.SimpleFileStorageService>();
builder.Services.AddScoped<Getir.Application.Services.FileUpload.IFileUploadIntegrationService, Getir.Application.Services.FileUpload.FileUploadIntegrationService>();
builder.Services.AddScoped<ICdnService, Getir.Infrastructure.Services.Cdn.SimpleCdnService>();

// File Upload Settings
builder.Services.Configure<FileUploadSettings>(builder.Configuration.GetSection("FileUpload"));

// ============= CONFIGURATION =============
// Add Controllers
builder.Services.AddControllers();

builder.Services.AddAuthConfiguration(builder.Configuration);
builder.Services.AddApiKeyConfiguration(builder.Configuration);
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

// Health check services
builder.Services.AddHttpClient<Getir.WebApi.HealthChecks.ExternalApiHealthCheck>();
builder.Services.AddVersioningConfiguration();
builder.Services.AddValidationConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddRateLimitConfiguration(builder.Configuration);
builder.Services.AddMetricsConfiguration();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssemblyContaining<Getir.Application.Validators.CreateSpecialHolidayRequestValidator>();

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(ApplicationConstants.SignalRClientTimeoutSeconds);
});

// CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Allow all origins for SignalR WebSocket
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============= BUILD APP =============
var app = builder.Build();

// ============= MIDDLEWARE PIPELINE =============
// Order is important! Exception handler MUST be first to catch all errors
app.UseMiddleware<GlobalExceptionMiddleware>();  // 1. Global exception handler (ApiResponse format)
app.UseMiddleware<RequestIdMiddleware>();        // 2. Request ID tracking

app.UseSerilogRequestLogging();

app.UseMiddleware<ValidationMiddleware>();       // 3. Validation (after logging)

// Skip SecurityAuditMiddleware in Testing environment (requires Session)
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseMiddleware<SecurityAuditMiddleware>();
}

app.UseMiddleware<RateLimitMiddleware>();

// Rate limiting
app.UseIpRateLimiting();
app.UseClientRateLimiting();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwaggerConfiguration();

// ============= SECURITY =============
app.UseHttpsRedirection();

// Request size limiting
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
    context.Request.Body.Position = 0;
    
    if (body.Length > ApplicationConstants.MaxRequestSizeBytes) // Request size limit
    {
        context.Response.StatusCode = 413; // Payload Too Large
        await context.Response.WriteAsync("Request size exceeds 10MB limit");
        return;
    }
    
    await next();
});

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:;";
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
    context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
    context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
    context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
    
    await next();
});

// CORS for SignalR
app.UseCors("SignalRCorsPolicy");


app.UseAuthentication();
app.UseAuthorization();

// ============= CONTROLLERS =============
app.MapControllers();

// ============= SIGNALR HUBS =============
app.MapHub<Getir.WebApi.Hubs.NotificationHub>("/hubs/notifications");
app.MapHub<Getir.WebApi.Hubs.OrderHub>("/hubs/orders");
app.MapHub<Getir.WebApi.Hubs.CourierHub>("/hubs/courier");
app.MapHub<Getir.WebApi.Hubs.RealtimeTrackingHub>("/hubs/realtime-tracking");

app.UseHealthChecksConfiguration();
app.UseMetricsConfiguration();

// ============= RUN =============
try
{
    Log.Information("Starting Getir API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration tests
public partial class Program { }
