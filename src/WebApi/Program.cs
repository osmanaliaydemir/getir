using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Getir.Application.Abstractions;
using Getir.Application.Services.Addresses;
using Getir.Application.Services.Auth;
using Getir.Application.Services.Campaigns;
using Getir.Application.Services.Cart;
using Getir.Application.Services.ServiceCategories;
using Getir.Application.Services.ProductCategories;
using Getir.Application.Services.Coupons;
using Getir.Application.Services.Couriers;
using Getir.Application.Services.Merchants;
using Getir.Application.Services.Notifications;
using Getir.Application.Services.Orders;
using Getir.Application.Services.Products;
using Getir.Application.Services.Search;
using Getir.Infrastructure.Persistence;
using Getir.Infrastructure.Persistence.Repositories;
using Getir.Infrastructure.Security;
using Getir.WebApi.Configuration;
using Getir.WebApi.Endpoints;
using Getir.WebApi.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Getir.Application.Services.WorkingHours;
using Getir.Application.Services.DeliveryZones;

var builder = WebApplication.CreateBuilder(args);

// ============= SERILOG =============
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// ============= DATABASE =============
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ============= DEPENDENCY INJECTION =============
// Infrastructure
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IReadOnlyRepository<>), typeof(ReadOnlyRepository<>));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();

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
builder.Services.AddScoped<ICourierService, CourierService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IWorkingHoursService, WorkingHoursService>();
builder.Services.AddScoped<IDeliveryZoneService, DeliveryZoneService>();
builder.Services.AddScoped<IMerchantDashboardService, MerchantDashboardService>();
builder.Services.AddScoped<IMerchantOnboardingService, MerchantOnboardingService>();

// ============= CONFIGURATION =============
builder.Services.AddAuthConfiguration(builder.Configuration);
builder.Services.AddHealthChecksConfiguration(builder.Configuration);
builder.Services.AddVersioningConfiguration();
builder.Services.AddValidationConfiguration();
builder.Services.AddSwaggerConfiguration();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000") // Frontend URL
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============= BUILD APP =============
var app = builder.Build();

// ============= MIDDLEWARE PIPELINE =============
// Order is important!
app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwaggerConfiguration();

app.UseHttpsRedirection();

// CORS for SignalR
app.UseCors("SignalRCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// ============= ENDPOINTS =============
app.MapAuthEndpoints();
app.MapServiceCategoryEndpoints();
app.MapProductCategoryEndpoints();
app.MapMerchantEndpoints();
app.MapProductEndpoints();
app.MapOrderEndpoints();
app.MapUserEndpoints();
app.MapCartEndpoints();
app.MapCouponEndpoints();
app.MapCampaignEndpoints();
app.MapNotificationEndpoints();
app.MapCourierEndpoints();
app.MapSearchEndpoints();
app.MapWorkingHoursEndpoints();
app.MapDeliveryZoneEndpoints();
app.MapMerchantDashboardEndpoints();
app.MapMerchantOnboardingEndpoints();
app.MapMerchantProductEndpoints();
app.MapMerchantOrderEndpoints();

// ============= SIGNALR HUBS =============
app.MapHub<Getir.WebApi.Hubs.NotificationHub>("/hubs/notifications");
app.MapHub<Getir.WebApi.Hubs.OrderHub>("/hubs/orders");
app.MapHub<Getir.WebApi.Hubs.CourierHub>("/hubs/courier");

app.UseHealthChecksConfiguration();

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
