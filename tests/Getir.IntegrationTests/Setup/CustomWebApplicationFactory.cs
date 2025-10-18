using Getir.Domain.Entities;
using Getir.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Getir.IntegrationTests.Setup;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove all possible DbContext registrations
            var toRemove = services
                .Where(d => d.ServiceType.FullName != null &&
                           (d.ServiceType.FullName.Contains("DbContext") ||
                            d.ServiceType.FullName.Contains("DbContextOptions")))
                .ToList();

            foreach (var service in toRemove)
            {
                services.Remove(service);
            }

            // Add InMemory database with RowVersion interceptor
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                options.UseInMemoryDatabase("GetirTestDb")
                       .EnableSensitiveDataLogging()
                       .EnableDetailedErrors()
                       .AddInterceptors(new RowVersionInterceptor());
            });

            // Configure JWT Authentication for testing
            // IMPORTANT: Keep authentication working so Context.User is populated with claims
            // This allows Hub methods to read UserId from ClaimTypes.NameIdentifier
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // For SignalR: Check for access_token in query string (SignalR requirement)
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        
                        return Task.CompletedTask;
                    }
                };
            });
            
            // Override authorization for testing - completely bypass for simplicity
            // This allows SignalR /negotiate and hub endpoints to work without auth complexity
            services.AddAuthorization(options =>
            {
                // Make default policy always succeed (bypass authorization checks)
                // BUT still keep JWT authentication active so Context.User is populated
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new AlwaysSucceedRequirement())
                    .Build();
                    
                // NO fallback policy
                options.FallbackPolicy = null;
                    
                // Override all role-based policies to always succeed
                options.AddPolicy("Admin", policy => policy.AddRequirements(new AlwaysSucceedRequirement()));
                options.AddPolicy("MerchantOwner", policy => policy.AddRequirements(new AlwaysSucceedRequirement()));
                options.AddPolicy("Courier", policy => policy.AddRequirements(new AlwaysSucceedRequirement()));
                options.AddPolicy("Merchant", policy => policy.AddRequirements(new AlwaysSucceedRequirement()));
            });
            
            // Register the custom authorization handler
            services.AddSingleton<IAuthorizationHandler, AlwaysSucceedAuthorizationHandler>();

            // Add test email configuration
            services.Configure<Application.DTO.EmailConfiguration>(options =>
            {
                // This will be bound from empty configuration, providing defaults
            });

            // Add mock email configuration via Options pattern
            //services.AddSingleton<Microsoft.Extensions.Options.IOptions<Application.DTO.EmailConfiguration>>(
            //    new Microsoft.Extensions.Options.OptionsWrapper<Application.DTO.EmailConfiguration>(
            //        new Application.DTO.EmailConfiguration(
            //            "smtp.test.com",
            //            587,
            //            true,
            //            "test@test.com",
            //            "testpassword",
            //            "test@test.com",
            //            "Test System"
            //        )
            //    )
            //);

            // Build and seed
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                    SeedTestData(db);
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();
                    logger.LogError(ex, "An error occurred seeding the database.");
                }
            }
        });
    }

    private void SeedTestData(AppDbContext context)
    {
        // Clear existing data
        context.ServiceCategories.RemoveRange(context.ServiceCategories);
        context.SaveChanges();

        // Seed test service categories
        var serviceCategory = new ServiceCategory
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Test Service Category",
            Description = "Test Service Description",
            IsActive = true,
            DisplayOrder = 1,
            CreatedAt = DateTime.UtcNow,
            Type = Domain.Enums.ServiceCategoryType.Restaurant
        };

        context.ServiceCategories.Add(serviceCategory);
        
        // Seed a test admin user for merchant/product operations
        var adminUser = new User
        {
            Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            Email = "testadmin@test.com",
            PasswordHash = "$2a$11$test", // Dummy hash
            FirstName = "Test",
            LastName = "Admin",
            Role = Domain.Enums.UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Users.Add(adminUser);
        context.SaveChanges();
    }
}

/// <summary>
/// Authorization requirement that always succeeds (for testing)
/// </summary>
public class AlwaysSucceedRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization handler that always succeeds (bypasses all authorization for tests)
/// </summary>
public class AlwaysSucceedAuthorizationHandler : AuthorizationHandler<AlwaysSucceedRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AlwaysSucceedRequirement requirement)
    {
        // Always succeed - this bypasses authorization checks in tests
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
