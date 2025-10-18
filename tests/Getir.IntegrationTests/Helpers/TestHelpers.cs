using System.Net.Http.Json;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Getir.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Getir.IntegrationTests.Helpers;

public static class TestHelpers
{
    public static async Task<(string Token, Guid UserId)> GetAuthTokenAsync(HttpClient client, bool asAdmin = true)
    {
        var email = $"testuser{Guid.NewGuid()}@example.com";
        var role = asAdmin ? Domain.Enums.UserRole.Admin : Domain.Enums.UserRole.Customer;
        
        var registerRequest = new RegisterRequest(
            email,
            "Test123!",
            "Test",
            "User",
            "+905551234567",
            role);

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to get auth token. Status: {response.StatusCode}, Error: {errorContent}");
        }
        
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        
        return (authResponse!.AccessToken, authResponse.UserId);
    }

    /// <summary>
    /// Creates test merchant directly in database (bypassing REST API)
    /// This is faster and more reliable for test setup
    /// </summary>
    public static async Task<Guid> CreateTestMerchantAsync(IServiceProvider services, Guid? ownerId = null)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var merchantId = Guid.NewGuid();
        var ownerUserId = ownerId ?? Guid.NewGuid();

        // Create owner user if not provided
        if (ownerId == null)
        {
            var owner = new User
            {
                Id = ownerUserId,
                Email = $"merchant-owner-{merchantId}@test.com",
                PasswordHash = "$2a$11$test",
                FirstName = "Test",
                LastName = "Merchant Owner",
                Role = UserRole.MerchantOwner,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(owner);
        }

        var merchant = new Merchant
        {
            Id = merchantId,
            OwnerId = ownerUserId,
            Name = $"Test Merchant {Guid.NewGuid().ToString().Substring(0, 8)}",
            Description = "Test merchant description",
            ServiceCategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Address = "123 Test Street",
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            PhoneNumber = "+905551234567",
            Email = $"merchant{merchantId.ToString().Substring(0, 8)}@test.com",
            MinimumOrderAmount = 50.0m,
            DeliveryFee = 10.0m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Merchants.Add(merchant);
        await dbContext.SaveChangesAsync();

        return merchantId;
    }

    /// <summary>
    /// Overload for HttpClient compatibility (gets DbContext from DI)
    /// </summary>
    public static async Task<Guid> CreateTestMerchantAsync(HttpClient client)
    {
        // This is a workaround - we need to get DbContext from somewhere
        // For now, throw a helpful exception
        throw new NotSupportedException(
            "Use CreateTestMerchantAsync(IServiceProvider services) instead. " +
            "Call from test class with _factory.Services");
    }

    /// <summary>
    /// Creates test product directly in database
    /// </summary>
    public static async Task<Guid> CreateTestProductAsync(
        IServiceProvider services,
        Guid merchantId,
        string name = "Test Product",
        decimal price = 100.0m)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var productId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            MerchantId = merchantId,
            ProductCategoryId = null,
            Name = name,
            Description = "Test product description",
            Price = price,
            ImageUrl = null,
            StockQuantity = 100,
            Unit = "piece",
            IsAvailable = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        return productId;
    }

    /// <summary>
    /// Overload for backward compatibility
    /// </summary>
    public static async Task<Guid> CreateTestProductAsync(HttpClient client, Guid merchantId, string name = "Test Product", decimal price = 100.0m)
    {
        throw new NotSupportedException(
            "Use CreateTestProductAsync(IServiceProvider services, Guid merchantId) instead. " +
            "Call from test class with _factory.Services");
    }

    public static async Task<Guid> CreateTestAddressAsync(HttpClient client)
    {
        var request = new CreateAddressRequest(
            $"Test Address {Guid.NewGuid().ToString().Substring(0, 8)}",
            "123 Test St",
            "Test City",
            "Test District",
            40.7128m,
            -74.0060m);

        var response = await client.PostAsJsonAsync("/api/v1/user/addresses", request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create address. Status: {response.StatusCode}, Error: {errorContent}");
        }
        
        var address = await response.Content.ReadFromJsonAsync<AddressResponse>();
        return address!.Id;
    }

    /// <summary>
    /// Creates test order directly in database
    /// </summary>
    public static async Task<Guid> CreateTestOrderAsync(
        IServiceProvider services,
        Guid? userId = null,
        Guid? merchantId = null,
        Guid? productId = null)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create test user if not provided
        var customerId = userId ?? Guid.NewGuid();
        if (userId == null)
        {
            var customer = new User
            {
                Id = customerId,
                Email = $"customer-{customerId}@test.com",
                PasswordHash = "$2a$11$test",
                FirstName = "Test",
                LastName = "Customer",
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(customer);
        }

        // Create merchant if not provided
        var testMerchantId = merchantId ?? await CreateTestMerchantAsync(services);

        // Create product if not provided
        var testProductId = productId ?? await CreateTestProductAsync(services, testMerchantId);

        // Create order
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = $"TEST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            UserId = customerId,
            MerchantId = testMerchantId,
            Status = OrderStatus.Pending,
            SubTotal = 200.0m,
            DeliveryFee = 10.0m,
            Discount = 0m,
            Total = 210.0m,
            PaymentMethod = "CREDIT_CARD",
            PaymentStatus = "Pending",
            DeliveryAddress = "123 Test Street, Test City",
            DeliveryLatitude = 40.7128m,
            DeliveryLongitude = -74.0060m,
            EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(30),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Orders.Add(order);

        // Create order line
        var orderLine = new OrderLine
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = testProductId,
            ProductName = "Test Product",
            Quantity = 2,
            UnitPrice = 100.0m,
            TotalPrice = 200.0m
        };

        dbContext.OrderLines.Add(orderLine);
        await dbContext.SaveChangesAsync();

        return orderId;
    }

    /// <summary>
    /// Overload for backward compatibility
    /// </summary>
    public static async Task<Guid> CreateTestOrderAsync(HttpClient client)
    {
        throw new NotSupportedException(
            "Use CreateTestOrderAsync(IServiceProvider services) instead. " +
            "Call from test class with _factory.Services");
    }

    /// <summary>
    /// Creates test address directly in database
    /// </summary>
    public static async Task<Guid> CreateTestAddressAsync(
        IServiceProvider services,
        Guid userId,
        string address = "Test Address")
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var addressId = Guid.NewGuid();
        var userAddress = new UserAddress
        {
            Id = addressId,
            UserId = userId,
            Title = address,
            FullAddress = "123 Test St",
            City = "Test City",
            District = "Test District",
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            IsDefault = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.UserAddresses.Add(userAddress);
        await dbContext.SaveChangesAsync();

        return addressId;
    }
}


