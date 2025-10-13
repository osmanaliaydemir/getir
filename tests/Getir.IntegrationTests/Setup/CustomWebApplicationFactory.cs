using Getir.Domain.Entities;
using Getir.Infrastructure.Persistence;
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

            // Override authorization policies for testing - allow all authenticated users
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                    
                // Override role-based policies to just require authentication
                options.AddPolicy("Admin", policy => policy.RequireAuthenticatedUser());
                options.AddPolicy("MerchantOwner", policy => policy.RequireAuthenticatedUser());
                options.AddPolicy("Courier", policy => policy.RequireAuthenticatedUser());
            });

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
