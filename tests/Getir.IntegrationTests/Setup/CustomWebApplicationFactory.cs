using Getir.Domain.Entities;
using Getir.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Getir.IntegrationTests.Setup;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove SQL Server DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add InMemory DbContext for testing
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("GetirTestDb");
            });

            // Build service provider
            var sp = services.BuildServiceProvider();

            // Create scope and seed test data
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();

            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    private void SeedTestData(AppDbContext context)
    {
        // Seed test categories
        var category = new Category
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Test Category",
            Description = "Test Description",
            IsActive = true,
            DisplayOrder = 1,
            CreatedAt = DateTime.UtcNow
        };

        context.Categories.Add(category);
        context.SaveChanges();
    }
}
