using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Orders;
using Getir.Domain.Entities;
using Getir.UnitTests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly Mock<ISignalRService> _signalRServiceMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<OrderService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        _signalRServiceMock = new Mock<ISignalRService>();
        
        _orderService = new OrderService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object,
            paymentService: null, // Mock payment service
            _signalRServiceMock.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_ValidOrder_ShouldSucceedWithTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var merchant = TestDataGenerator.CreateMerchant();
        var product = TestDataGenerator.CreateProduct(merchant.Id, stockQuantity: 100);
        product.Price = 50;

        var request = new CreateOrderRequest(
            merchant.Id,
            new List<OrderLineRequest>
            {
                new(product.Id, 2, "Test notes")
            },
            "Test Address",
            40.9897m,
            29.0257m,
            "CreditCard",
            null);

        SetupMerchantMock(merchant);
        SetupProductMock(product);
        SetupOrderRepositories();

        // Act
        var result = await _orderService.CreateOrderAsync(userId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.SubTotal.Should().Be(100); // 2 * 50
        result.Value.Total.Should().Be(115); // 100 + 15 (delivery)
        result.Value.Status.Should().Be("Pending");
        
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_BelowMinimumAmount_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var merchant = TestDataGenerator.CreateMerchant();
        merchant.MinimumOrderAmount = 100; // Minimum 100 TL
        
        var product = TestDataGenerator.CreateProduct(merchant.Id);
        product.Price = 30;

        var request = new CreateOrderRequest(
            merchant.Id,
            new List<OrderLineRequest> { new(product.Id, 1, null) }, // Only 30 TL
            "Test Address",
            40.9897m,
            29.0257m,
            "CreditCard",
            null);

        SetupMerchantMock(merchant);
        SetupProductMock(product);

        // Act
        var result = await _orderService.CreateOrderAsync(userId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("BELOW_MINIMUM_ORDER");
        result.Error.Should().Contain("Minimum order amount");
        
        _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_InsufficientStock_ShouldRollback()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var merchant = TestDataGenerator.CreateMerchant();
        var product = TestDataGenerator.CreateProduct(merchant.Id, stockQuantity: 5);

        var request = new CreateOrderRequest(
            merchant.Id,
            new List<OrderLineRequest> { new(product.Id, 10, null) }, // Request 10, but only 5 available
            "Test Address",
            40.9897m,
            29.0257m,
            "CreditCard",
            null);

        SetupMerchantMock(merchant);
        SetupProductMock(product);

        // Act
        var result = await _orderService.CreateOrderAsync(userId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INSUFFICIENT_STOCK");
        
        _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_MultipleProducts_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var merchant = TestDataGenerator.CreateMerchant();
        merchant.DeliveryFee = 20;
        
        var product1 = TestDataGenerator.CreateProduct(merchant.Id, stockQuantity: 100);
        product1.Price = 50;
        product1.DiscountedPrice = 40; // İndirimli
        
        var product2 = TestDataGenerator.CreateProduct(merchant.Id, stockQuantity: 100);
        product2.Price = 30;
        product2.DiscountedPrice = null; // İndirimsiz

        var request = new CreateOrderRequest(
            merchant.Id,
            new List<OrderLineRequest>
            {
                new(product1.Id, 2, null), // 2 * 40 = 80
                new(product2.Id, 3, null)  // 3 * 30 = 90
            },
            "Test Address",
            40.9897m,
            29.0257m,
            "CreditCard",
            null);

        SetupMerchantMock(merchant);
        SetupMultipleProductsMock(new[] { product1, product2 });
        SetupOrderRepositories();

        // Act
        var result = await _orderService.CreateOrderAsync(userId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.SubTotal.Should().Be(170); // 80 + 90
        result.Value.DeliveryFee.Should().Be(20);
        result.Value.Total.Should().Be(190); // 170 + 20
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateOrderAsync_InactiveMerchant_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var merchant = TestDataGenerator.CreateMerchant();
        merchant.IsActive = false;

        var request = new CreateOrderRequest(
            merchant.Id,
            new List<OrderLineRequest> { new(Guid.NewGuid(), 1, null) },
            "Test Address",
            40.9897m,
            29.0257m,
            "CreditCard",
            null);

        var readMerchantRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        readMerchantRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Merchant?)null); // IsActive kontrolü nedeniyle null döner

        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(readMerchantRepoMock.Object);

        // Act
        var result = await _orderService.CreateOrderAsync(userId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("MERCHANT_NOT_FOUND");
    }

    // Helper methods
    private void SetupMerchantMock(Merchant merchant)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchant);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(readRepoMock.Object);
    }

    private void SetupProductMock(Product product)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Product>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.Is<System.Linq.Expressions.Expression<Func<Product, bool>>>(expr => true),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var productRepoMock = new Mock<IGenericRepository<Product>>();
        productRepoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(readRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);
    }

    private void SetupMultipleProductsMock(Product[] products)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Product>>();
        foreach (var product in products)
        {
            readRepoMock.Setup(r => r.FirstOrDefaultAsync(
                It.Is<System.Linq.Expressions.Expression<Func<Product, bool>>>(expr => 
                    expr.Compile()(product)),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
        }

        var productRepoMock = new Mock<IGenericRepository<Product>>();
        foreach (var product in products)
        {
            productRepoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
        }

        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(readRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);
    }

    private void SetupOrderRepositories()
    {
        var orderRepoMock = new Mock<IGenericRepository<Order>>();
        var orderLineRepoMock = new Mock<IGenericRepository<OrderLine>>();

        _unitOfWorkMock.Setup(u => u.Repository<Order>()).Returns(orderRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<OrderLine>()).Returns(orderLineRepoMock.Object);
    }
}
