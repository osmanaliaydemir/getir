using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Getir.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Getir.IntegrationTests.Setup;
using Xunit;

namespace Getir.IntegrationTests.Api;

public class PaymentEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public PaymentEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    #region Helper Methods

    private async Task<Guid> CreateTestUserAsync(UserRole role = UserRole.Customer)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"test_{role}@example.com",
            PasswordHash = "test_hash",
            FirstName = "Test",
            LastName = "User",
            Role = role,
            IsEmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }

    private async Task<Guid> CreateTestMerchantAsync(Guid ownerId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Name = "Test Merchant",
            Description = "Test Merchant Description",
            ServiceCategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // Test service category
            Address = "Test Address",
            Latitude = 41.0082m,
            Longitude = 28.9784m,
            PhoneNumber = "5551234567",
            Email = "merchant@test.com",
            MinimumOrderAmount = 50m,
            DeliveryFee = 10m,
            AverageDeliveryTime = 30,
            IsActive = true,
            IsBusy = false,
            IsOpen = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Merchants.Add(merchant);
        await context.SaveChangesAsync();
        return merchant.Id;
    }

    private async Task<Guid> CreateTestCourierAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "courier@test.com",
            PasswordHash = "test_hash",
            FirstName = "Test",
            LastName = "Courier",
            Role = UserRole.Courier,
            IsEmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var courier = new Courier
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VehicleType = "Bicycle",
            LicensePlate = "TEST123",
            IsActive = true,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        context.Couriers.Add(courier);
        await context.SaveChangesAsync();
        return courier.Id;
    }

    private async Task<Guid> CreateTestOrderAsync(Guid userId, Guid merchantId, Guid? courierId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "TEST-001",
            UserId = userId,
            MerchantId = merchantId,
            CourierId = courierId,
            Status = OrderStatus.Confirmed,
            SubTotal = 75.00m,
            DeliveryFee = 10.00m,
            Discount = 0m,
            Total = 85.00m,
            PaymentMethod = "Cash",
            PaymentStatus = "Pending",
            DeliveryAddress = "Test Delivery Address",
            DeliveryLatitude = 41.0082m,
            DeliveryLongitude = 28.9784m,
            CreatedAt = DateTime.UtcNow
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return order.Id;
    }

    private async Task<string> GetAuthTokenAsync(Guid userId, UserRole role)
    {
        // Bu basit bir test için mock token döndürüyor
        // Gerçek implementation'da JWT token oluşturulmalı
        return $"Bearer test_token_{userId}_{role}";
    }

    #endregion

    #region Create Payment Tests

    [Fact]
    public async Task CreatePayment_WithValidRequest_ShouldReturnPaymentResponse()
    {
        // Arrange
        var userId = await CreateTestUserAsync(UserRole.Customer);
        var merchantId = await CreateTestMerchantAsync(userId);
        var orderId = await CreateTestOrderAsync(userId, merchantId);
        var authToken = await GetAuthTokenAsync(userId, UserRole.Customer);

        var request = new CreatePaymentRequest(
            OrderId: orderId,
            PaymentMethod: PaymentMethod.Cash,
            Amount: 85.50m,
            ChangeAmount: 4.50m,
            Notes: "Para üstü: 4.50 TL"
        );

        _client.DefaultRequestHeaders.Add("Authorization", authToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/payments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(content, _jsonOptions);

        paymentResponse.Should().NotBeNull();
        paymentResponse!.PaymentMethod.Should().Be(PaymentMethod.Cash);
        paymentResponse.Status.Should().Be(PaymentStatus.Pending);
        paymentResponse.Amount.Should().Be(85.50m);
        paymentResponse.ChangeAmount.Should().Be(4.50m);
        paymentResponse.Notes.Should().Be("Para üstü: 4.50 TL");
    }

    [Fact]
    public async Task CreatePayment_WithInvalidOrderId_ShouldReturnNotFound()
    {
        // Arrange
        var userId = await CreateTestUserAsync(UserRole.Customer);
        var authToken = await GetAuthTokenAsync(userId, UserRole.Customer);

        var request = new CreatePaymentRequest(
            OrderId: Guid.NewGuid(), // Non-existent order
            PaymentMethod: PaymentMethod.Cash,
            Amount: 85.50m
        );

        _client.DefaultRequestHeaders.Add("Authorization", authToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/payments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreatePayment_WithUnsupportedPaymentMethod_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = await CreateTestUserAsync(UserRole.Customer);
        var merchantId = await CreateTestMerchantAsync(userId);
        var orderId = await CreateTestOrderAsync(userId, merchantId);
        var authToken = await GetAuthTokenAsync(userId, UserRole.Customer);

        var request = new CreatePaymentRequest(
            OrderId: orderId,
            PaymentMethod: PaymentMethod.CreditCard, // Not supported yet
            Amount: 85.50m
        );

        _client.DefaultRequestHeaders.Add("Authorization", authToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/payments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Get Payment Tests

    [Fact]
    public async Task GetPaymentById_WithValidId_ShouldReturnPaymentResponse()
    {
        // Arrange
        var userId = await CreateTestUserAsync(UserRole.Customer);
        var merchantId = await CreateTestMerchantAsync(userId);
        var orderId = await CreateTestOrderAsync(userId, merchantId);
        var authToken = await GetAuthTokenAsync(userId, UserRole.Customer);

        // Create payment first
        var createRequest = new CreatePaymentRequest(
            OrderId: orderId,
            PaymentMethod: PaymentMethod.Cash,
            Amount: 85.50m
        );

        _client.DefaultRequestHeaders.Add("Authorization", authToken);
        var createResponse = await _client.PostAsJsonAsync("/api/v1/payments", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var payment = JsonSerializer.Deserialize<PaymentResponse>(createContent, _jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/v1/payments/{payment!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(content, _jsonOptions);

        paymentResponse.Should().NotBeNull();
        paymentResponse!.Id.Should().Be(payment.Id);
        paymentResponse.PaymentMethod.Should().Be(PaymentMethod.Cash);
        paymentResponse.Amount.Should().Be(85.50m);
    }

    [Fact]
    public async Task GetPaymentById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var userId = await CreateTestUserAsync(UserRole.Customer);
        var authToken = await GetAuthTokenAsync(userId, UserRole.Customer);

        _client.DefaultRequestHeaders.Add("Authorization", authToken);

        // Act
        var response = await _client.GetAsync($"/api/v1/payments/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Courier Cash Collection Tests

    [Fact]
    public async Task GetPendingCashPayments_AsCourier_ShouldReturnPendingPayments()
    {
        // Arrange
        var courierId = await CreateTestCourierAsync();
        var authToken = await GetAuthTokenAsync(courierId, UserRole.Courier);

        _client.DefaultRequestHeaders.Add("Authorization", authToken);

        // Act
        var response = await _client.GetAsync("/api/v1/payments/courier/pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<PaymentResponse>>(content, _jsonOptions);

        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().NotBeNull();
        pagedResult.Page.Should().Be(1);
        pagedResult.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task CollectCashPayment_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var courierId = await CreateTestCourierAsync();
        var userId = await CreateTestUserAsync(UserRole.Customer);
        var merchantId = await CreateTestMerchantAsync(userId);
        var orderId = await CreateTestOrderAsync(userId, merchantId, courierId);
        var authToken = await GetAuthTokenAsync(courierId, UserRole.Courier);

        // Create payment first
        var createRequest = new CreatePaymentRequest(
            OrderId: orderId,
            PaymentMethod: PaymentMethod.Cash,
            Amount: 85.50m
        );

        _client.DefaultRequestHeaders.Add("Authorization", authToken);
        var createResponse = await _client.PostAsJsonAsync("/api/v1/payments", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var payment = JsonSerializer.Deserialize<PaymentResponse>(createContent, _jsonOptions);

        // Collect payment
        var collectRequest = new CollectCashPaymentRequest(
            CollectedAmount: 85.50m,
            Notes: "Para üstü: 4.50 TL verildi"
        );

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/payments/courier/{payment!.Id}/collect", collectRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task FailCashPayment_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var courierId = await CreateTestCourierAsync();
        var userId = await CreateTestUserAsync(UserRole.Customer);
        var merchantId = await CreateTestMerchantAsync(userId);
        var orderId = await CreateTestOrderAsync(userId, merchantId, courierId);
        var authToken = await GetAuthTokenAsync(courierId, UserRole.Courier);

        // Create payment first
        var createRequest = new CreatePaymentRequest(
            OrderId: orderId,
            PaymentMethod: PaymentMethod.Cash,
            Amount: 85.50m
        );

        _client.DefaultRequestHeaders.Add("Authorization", authToken);
        var createResponse = await _client.PostAsJsonAsync("/api/v1/payments", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var payment = JsonSerializer.Deserialize<PaymentResponse>(createContent, _jsonOptions);

        // Fail payment
        var failRequest = new { reason = "Customer not available" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/payments/courier/{payment!.Id}/fail", failRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCourierCashSummary_WithValidCourier_ShouldReturnSummary()
    {
        // Arrange
        var courierId = await CreateTestCourierAsync();
        var authToken = await GetAuthTokenAsync(courierId, UserRole.Courier);

        _client.DefaultRequestHeaders.Add("Authorization", authToken);

        // Act
        var response = await _client.GetAsync("/api/v1/payments/courier/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var summary = JsonSerializer.Deserialize<CourierCashSummaryResponse>(content, _jsonOptions);

        summary.Should().NotBeNull();
        summary!.CourierId.Should().Be(courierId);
        summary.TotalCollected.Should().Be(0m); // No collections yet
        summary.TotalOrders.Should().Be(0);
    }

    #endregion

    #region Merchant Tests

    [Fact]
    public async Task GetMerchantCashSummary_AsMerchantOwner_ShouldReturnSummary()
    {
        // Arrange
        var userId = await CreateTestUserAsync(UserRole.MerchantOwner);
        var merchantId = await CreateTestMerchantAsync(userId);
        var authToken = await GetAuthTokenAsync(userId, UserRole.MerchantOwner);

        _client.DefaultRequestHeaders.Add("Authorization", authToken);

        // Act
        var response = await _client.GetAsync("/api/v1/payments/merchant/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var summary = JsonSerializer.Deserialize<MerchantCashSummaryResponse>(content, _jsonOptions);

        summary.Should().NotBeNull();
        summary!.MerchantId.Should().Be(merchantId);
        summary.TotalAmount.Should().Be(0m); // No payments yet
    }

    [Fact]
    public async Task GetMerchantSettlements_AsMerchantOwner_ShouldReturnSettlements()
    {
        // Arrange
        var userId = await CreateTestUserAsync(UserRole.MerchantOwner);
        var merchantId = await CreateTestMerchantAsync(userId);
        var authToken = await GetAuthTokenAsync(userId, UserRole.MerchantOwner);

        _client.DefaultRequestHeaders.Add("Authorization", authToken);

        // Act
        var response = await _client.GetAsync("/api/v1/payments/merchant/settlements");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<SettlementResponse>>(content, _jsonOptions);

        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().NotBeNull();
        pagedResult.Page.Should().Be(1);
        pagedResult.PageSize.Should().Be(20);
    }

    #endregion

    #region Admin Tests

    [Fact]
    public async Task GetAllCashPayments_AsAdmin_ShouldReturnAllPayments()
    {
        // Arrange
        var userId = await CreateTestUserAsync(UserRole.Admin);
        var authToken = await GetAuthTokenAsync(userId, UserRole.Admin);

        _client.DefaultRequestHeaders.Add("Authorization", authToken);

        // Act
        var response = await _client.GetAsync("/api/v1/payments/admin/cash-collections");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<PaymentResponse>>(content, _jsonOptions);

        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().NotBeNull();
        pagedResult.Page.Should().Be(1);
        pagedResult.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task ProcessSettlement_AsAdmin_ShouldReturnOk()
    {
        // Arrange
        var userId = await CreateTestUserAsync(UserRole.Admin);
        var merchantId = await CreateTestMerchantAsync(userId);
        var authToken = await GetAuthTokenAsync(userId, UserRole.Admin);

        var request = new ProcessSettlementRequest(
            CommissionRate: 15m,
            Notes: "Test settlement",
            BankTransferReference: "TEST123456"
        );

        _client.DefaultRequestHeaders.Add("Authorization", authToken);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/payments/admin/settlements/{merchantId}/process", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task CreatePayment_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CreatePaymentRequest(
            OrderId: Guid.NewGuid(),
            PaymentMethod: PaymentMethod.Cash,
            Amount: 85.50m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/payments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPendingCashPayments_AsCustomer_ShouldReturnForbidden()
    {
        // Arrange
        var userId = await CreateTestUserAsync(UserRole.Customer);
        var authToken = await GetAuthTokenAsync(userId, UserRole.Customer);

        _client.DefaultRequestHeaders.Add("Authorization", authToken);

        // Act
        var response = await _client.GetAsync("/api/v1/payments/courier/pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllCashPayments_AsCustomer_ShouldReturnForbidden()
    {
        // Arrange
        var userId = await CreateTestUserAsync(UserRole.Customer);
        var authToken = await GetAuthTokenAsync(userId, UserRole.Customer);

        _client.DefaultRequestHeaders.Add("Authorization", authToken);

        // Act
        var response = await _client.GetAsync("/api/v1/payments/admin/cash-collections");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}
