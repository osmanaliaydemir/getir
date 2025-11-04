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

// Force URLs early so Kestrel binds to expected addresses even when launchSettings is bypassed
var configuredUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (string.IsNullOrWhiteSpace(configuredUrls))
{
    // Fallback default ports if none provided externally
    configuredUrls = builder.Configuration["Kestrel:Endpoints:Urls"]
        ?? "https://localhost:7170;http://localhost:5017";
}
builder.WebHost.UseUrls(configuredUrls);

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

// Add HttpClient for API communication with resilience (break circular DI with AuthService)
builder.Services.AddHttpClient("ApiClient", client =>
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
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    }
    else
    {
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => 
        {
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

// Typed ApiClient without injecting IAuthService to avoid circular dependency
builder.Services.AddTransient<ApiClient>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = factory.CreateClient("ApiClient");
    var logger = sp.GetRequiredService<ILogger<ApiClient>>();
    return new ApiClient(httpClient, logger);
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
    
    // Add Redis ConnectionMultiplexer (resilient)
    builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(provider =>
    {
        try
        {
            var options = StackExchange.Redis.ConfigurationOptions.Parse(redisConnectionString);
            options.ConnectRetry = 1;
            options.ConnectTimeout = 1000;
            options.SyncTimeout = 1000;
            options.AbortOnConnectFail = false;
            return StackExchange.Redis.ConnectionMultiplexer.Connect(options);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Redis connection failed. Continuing without Redis.");
            return null!; // AdvancedCacheService should handle null gracefully
        }
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
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IHelpService, HelpService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<ISignalRService, SignalRService>();
builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<GlobalErrorHandler>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<LocalizationService>();
builder.Services.AddSingleton<IAdvancedSeoService, AdvancedSeoService>();
builder.Services.AddScoped<IAdvancedSecurityService, AdvancedSecurityService>();
builder.Services.AddSingleton<IAdvancedMonitoringService, AdvancedMonitoringService>();
builder.Services.AddScoped<IAdvancedPwaService, AdvancedPwaService>();

// Register services with dependencies
builder.Services.AddSingleton<IPerformanceMonitoringService, PerformanceMonitoringService>();
builder.Services.AddSingleton<IAdvancedCacheService, AdvancedCacheService>();

// Add WeatherForecastService (keep for compatibility)
builder.Services.AddSingleton<WeatherForecastService>();

WebApplication? app = null;
try
{
    app = builder.Build();
    
    Log.Information("Application building completed successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to build");
    throw;
}

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

// TEMP: Disable extra security headers during diagnosis to avoid CSP/frame issues
// app.UseSecurityHeaders(policies =>
//     policies.AddDefaultSecurityHeaders()
//         .AddContentTypeOptionsNoSniff()
//         .AddFrameOptionsDeny()
//         .AddXssProtectionBlock()
//         .AddReferrerPolicyStrictOriginWhenCrossOrigin());

// Log each HTTP request EARLY in pipeline to catch all requests
app.UseSerilogRequestLogging();

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

// Diagnostics: lightweight liveness endpoint - MUST BE BEFORE MapFallback
app.MapGet("/ping", () => Results.Ok("pong"));
app.MapGet("/test", () => Results.Json(new { status = "ok", timestamp = DateTime.UtcNow }));

// Add Health Checks
app.MapHealthChecks("/health");

// Map Razor Pages so _Host and other pages can be served
app.MapRazorPages();

// Add SignalR Hubs
app.MapHub<NotificationHub>("/hubs/notification");

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Diagnostics root probe removed so that fallback to _Host handles '/'

// Ensure Serilog flushes and closes down properly
try
{
    Log.Information("Environment: {Env}", app.Environment.EnvironmentName);
    Log.Information("Application starting with requested URLs: {Urls}", configuredUrls);
    // After Kestrel starts, app.Urls will contain the actual bound addresses
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        Log.Information("Application started. Listening on: {Urls}", string.Join(", ", app.Urls));
    });
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}