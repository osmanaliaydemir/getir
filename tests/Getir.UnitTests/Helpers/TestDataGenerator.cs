using Bogus;
using Getir.Domain.Entities;

namespace Getir.UnitTests.Helpers;

public static class TestDataGenerator
{
    public static User CreateUser(string? email = null, string? password = null)
    {
        var faker = new Faker();
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email ?? faker.Internet.Email(),
            PasswordHash = password ?? "hashed_password_123",
            FirstName = faker.Name.FirstName(),
            LastName = faker.Name.LastName(),
            PhoneNumber = faker.Phone.PhoneNumber(),
            IsEmailVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Category CreateCategory(string? name = null)
    {
        var faker = new Faker();
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name ?? faker.Commerce.Categories(1)[0],
            Description = faker.Lorem.Sentence(),
            IsActive = true,
            DisplayOrder = faker.Random.Int(1, 10),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Merchant CreateMerchant(Guid? categoryId = null)
    {
        var faker = new Faker();
        return new Merchant
        {
            Id = Guid.NewGuid(),
            Name = faker.Company.CompanyName(),
            CategoryId = categoryId ?? Guid.NewGuid(),
            Address = faker.Address.FullAddress(),
            Latitude = (decimal)faker.Address.Latitude(),
            Longitude = (decimal)faker.Address.Longitude(),
            PhoneNumber = faker.Phone.PhoneNumber(),
            MinimumOrderAmount = 50,
            DeliveryFee = 15,
            AverageDeliveryTime = 30,
            IsActive = true,
            IsOpen = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Product CreateProduct(Guid? merchantId = null, int stockQuantity = 100)
    {
        var faker = new Faker();
        return new Product
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId ?? Guid.NewGuid(),
            Name = faker.Commerce.ProductName(),
            Description = faker.Commerce.ProductDescription(),
            Price = faker.Random.Decimal(10, 200),
            StockQuantity = stockQuantity,
            IsAvailable = true,
            IsActive = true,
            DisplayOrder = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Coupon CreateCoupon(string? code = null, string discountType = "Percentage")
    {
        var faker = new Faker();
        return new Coupon
        {
            Id = Guid.NewGuid(),
            Code = code ?? faker.Random.AlphaNumeric(10).ToUpper(),
            Title = faker.Lorem.Sentence(),
            DiscountType = discountType,
            DiscountValue = discountType == "Percentage" ? 20 : 50,
            MinimumOrderAmount = 50,
            MaximumDiscountAmount = discountType == "Percentage" ? 100 : null,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30),
            UsageCount = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CartItem CreateCartItem(Guid userId, Guid merchantId, Guid productId, int quantity = 1)
    {
        return new CartItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MerchantId = merchantId,
            ProductId = productId,
            Quantity = quantity,
            CreatedAt = DateTime.UtcNow
        };
    }
}
