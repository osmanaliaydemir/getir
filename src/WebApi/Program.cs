using FluentValidation;
using Getir.Application.Abstractions;
using Getir.Application.Services.Addresses;
using Getir.Application.Services.Auth;
using Getir.Application.Services.Campaigns;
using Getir.Application.Services.Cart;
using Getir.Application.Services.Categories;
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

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserAddressService, UserAddressService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICourierService, CourierService>();
builder.Services.AddScoped<ISearchService, SearchService>();

// ============= CONFIGURATION =============
builder.Services.AddAuthConfiguration(builder.Configuration);
builder.Services.AddHealthChecksConfiguration(builder.Configuration);
builder.Services.AddVersioningConfiguration();
builder.Services.AddValidationConfiguration();
builder.Services.AddSwaggerConfiguration();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

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

app.UseAuthentication();
app.UseAuthorization();

// ============= ENDPOINTS =============
app.MapAuthEndpoints();
app.MapCategoryEndpoints();
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
