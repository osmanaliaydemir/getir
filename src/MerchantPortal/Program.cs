using Getir.MerchantPortal.Middleware;
using Getir.MerchantPortal.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/merchantportal-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Configuration
var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>()!;

// Data Protection (Cookie encryption için)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Keys")))
    .SetApplicationName("GetirMerchantPortal");

// Add services to the container
builder.Services.AddControllersWithViews();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(12);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // ✅ HTTP için None olmalı
    options.Cookie.SameSite = SameSiteMode.Lax; // ✅ Cross-origin için Lax
});

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
        options.Cookie.Name = "GetirMerchantAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // ✅ HTTP için None olmalı
        options.Cookie.SameSite = SameSiteMode.Lax; // ✅ Cross-origin için Lax
    });

// HttpContext
builder.Services.AddHttpContextAccessor();

// Register AuthTokenHandler
builder.Services.AddTransient<AuthTokenHandler>();

// HttpClient for API calls
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiSettings.BaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<AuthTokenHandler>() // Auto-inject JWT token from session
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    
    // Development ortamında SSL certificate validation'ı bypass et
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = 
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    
    return handler;
});

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IWorkingHoursService, WorkingHoursService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddSingleton<ISignalRService, SignalRService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();

// Settings
builder.Services.AddSingleton(apiSettings);

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // app.UseHsts(); // ✅ HTTP için HSTS kapalı
}

// app.UseHttpsRedirection(); // ✅ HTTP için HTTPS redirect kapalı
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseSessionValidation(); // Validate session/cookie consistency
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

try
{
    Log.Information("Starting MerchantPortal application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MerchantPortal application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
