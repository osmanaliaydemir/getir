using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Localization;
using FluentValidation;
using Serilog;
using WebApp.Data;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/webapp-.txt", rollingInterval: Serilog.RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add Authentication & Authorization
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var apiSettings = builder.Configuration.GetSection("ApiSettings");
        options.Authority = apiSettings["Authority"] ?? (builder.Environment.IsDevelopment() ? "https://localhost:7001" : "https://ajilgo.runasp.net");
        options.Audience = apiSettings["Audience"] ?? "getir-api";
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

// Add HttpClient for API communication with resilience
builder.Services.AddHttpClient<ApiClient>(client =>
{
    var apiSettings = builder.Configuration.GetSection("ApiSettings");
    var baseUrl = apiSettings["BaseUrl"] ?? (builder.Environment.IsDevelopment() ? "https://localhost:7001" : "https://ajilgo.runasp.net");
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(int.Parse(apiSettings["Timeout"] ?? "30"));
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    
    // Development ortamında SSL sertifika doğrulamasını atla
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    }
    // Production'da SSL doğrulaması aktif
    else
    {
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => 
        {
            // Production'da sertifika doğrulaması yapılır
            return errors == System.Net.Security.SslPolicyErrors.None;
        };
    }
    
    return handler;
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 3;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.MinimumThroughput = 5;
});

// Add Caching
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

// Add Redis Cache if configured
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
    });
    
    // Add Redis ConnectionMultiplexer
    builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(provider =>
    {
        return StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString);
    });
}
else
{
    // Add mock ConnectionMultiplexer for development
    builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(provider =>
    {
        return null!; // Will be handled gracefully in AdvancedCacheService
    });
}

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ApiPolicy", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });
    
    options.AddSlidingWindowLimiter("StrictPolicy", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 2;
    });
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<ApiHealthCheck>("api")
    .AddCheck<DatabaseHealthCheck>("database");

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// Add TelemetryClient
builder.Services.AddSingleton<Microsoft.ApplicationInsights.TelemetryClient>();

// Add Performance Monitoring
builder.Services.AddApplicationInsightsTelemetryProcessor<CustomTelemetryProcessor>();

// Add SignalR
builder.Services.AddSignalR();

// Add Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "tr", "en", "ar" };
    options.SetDefaultCulture("tr")
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<WebApp.Models.Validators.LoginRequestValidator>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IHelpService, HelpService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<ISignalRService, SignalRService>();
builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<GlobalErrorHandler>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddSingleton<IAdvancedSeoService, AdvancedSeoService>();
builder.Services.AddScoped<IAdvancedSecurityService, AdvancedSecurityService>();
builder.Services.AddSingleton<IAdvancedMonitoringService, AdvancedMonitoringService>();
builder.Services.AddScoped<IAdvancedPwaService, AdvancedPwaService>();

// Register services with dependencies
builder.Services.AddSingleton<IPerformanceMonitoringService, PerformanceMonitoringService>();
builder.Services.AddSingleton<IAdvancedCacheService, AdvancedCacheService>();

// Add WeatherForecastService (keep for compatibility)
builder.Services.AddSingleton<WeatherForecastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Add Security Headers
app.UseSecurityHeaders(policies =>
    policies.AddDefaultSecurityHeaders()
        .AddContentTypeOptionsNoSniff()
        .AddFrameOptionsDeny()
        .AddXssProtectionBlock()
        .AddReferrerPolicyStrictOriginWhenCrossOrigin());

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add Localization
app.UseRequestLocalization();

// Add Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Add Rate Limiting
app.UseRateLimiter();

// Add Health Checks
app.MapHealthChecks("/health");

// Add SignalR Hubs
app.MapHub<NotificationHub>("/hubs/notification");

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Ensure Serilog flushes and closes down properly
app.Run();