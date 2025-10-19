using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Orders;
using Getir.Application.Services.Payments;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
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
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<OrderService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        _signalRServiceMock = new Mock<ISignalRService>();
        _paymentServiceMock = new Mock<IPaymentService>();
        
        _orderService = new OrderService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object,
            _paymentServiceMock.Object,
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
        var user = TestDataGenerator.CreateUser();
        user.Id = userId;

        var request = new CreateOrderRequest(
            merchant.Id,
            new List<OrderLineRequest>
            {
                new(product.Id, null, 2, "Test notes")
            },
            "Test Address",
            40.9897m,
            29.0257m,
            "CreditCard",
            null);

        SetupUserMock(user);
        SetupMerchantMock(merchant);
        SetupProductMock(product);
        SetupOrderRepositories();
        SetupPaymentServiceMock();

        // Act
        var result = await _orderService.CreateOrderAsync(userId, request);

        // Assert
        if (!result.Success)
        {
            Console.WriteLine($"Test failed with error: {result.Error}, Code: {result.ErrorCode}");
        }
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.SubTotal.Should().Be(100); // 2 * 50
        result.Value.Total.Should().Be(115); // 100 + 15 (delivery)
        result.Value.Status.Should().Be("Pending");
        
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce); // Stok güncellemesi için birden fazla çağrılabilir
    }

    [Fact]
    public async Task CreateOrderAsync_BelowMinimumAmount_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = TestDataGenerator.CreateUser();
        user.Id = userId;
        
        var merchant = TestDataGenerator.CreateMerchant();
        merchant.MinimumOrderAmount = 100; // Minimum 100 TL
        
        var product = TestDataGenerator.CreateProduct(merchant.Id);
        product.Price = 30;

        var request = new CreateOrderRequest(
            merchant.Id,
            new List<OrderLineRequest> { new(product.Id, null, 1, null) }, // Only 30 TL
            "Test Address",
            40.9897m,
            29.0257m,
            "CreditCard",
            null);

        SetupUserMock(user);
        SetupMerchantMock(merchant);
        SetupProductMock(product);
        SetupOrderRepositories();

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
        var user = TestDataGenerator.CreateUser();
        user.Id = userId;
        
        var merchant = TestDataGenerator.CreateMerchant();
        var product = TestDataGenerator.CreateProduct(merchant.Id, stockQuantity: 5);

        var request = new CreateOrderRequest(
            merchant.Id,
            new List<OrderLineRequest> { new(product.Id, null, 10, null) }, // Request 10, but only 5 available
            "Test Address",
            40.9897m,
            29.0257m,
            "CreditCard",
            null);

        SetupUserMock(user);
        SetupMerchantMock(merchant);
        SetupProductMock(product);
        SetupOrderRepositories();

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
        var user = TestDataGenerator.CreateUser();
        user.Id = userId;
        
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
                new(product1.Id, null, 2, null), // 2 * 40 = 80
                new(product2.Id, null, 3, null)  // 3 * 30 = 90
            },
            "Test Address",
            40.9897m,
            29.0257m,
            "CreditCard",
            null);

        SetupUserMock(user);
        SetupMerchantMock(merchant);
        SetupMultipleProductsMock(new[] { product1, product2 });
        SetupOrderRepositories();
        SetupPaymentServiceMock();

        // Act
        var result = await _orderService.CreateOrderAsync(userId, request);

        // Assert
        if (!result.Success)
        {
            Console.WriteLine($"Test failed with error: {result.Error}, Code: {result.ErrorCode}");
        }
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
            new List<OrderLineRequest> { new(Guid.NewGuid(), null, 1, null) },
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

    [Fact]
    public async Task GetOrderByIdAsync_ValidOrder_ShouldReturnOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            MerchantId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = OrderStatus.Confirmed,
            SubTotal = 100m,
            Total = 115m
        };

        SetupOrderByIdMock(order);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId, userId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(orderId);
    }

    [Fact]
    public async Task GetOrderByIdAsync_NotOwner_ShouldFail()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var actualUserId = Guid.NewGuid();
        var wrongUserId = Guid.NewGuid();
        
        var order = new Order
        {
            Id = orderId,
            UserId = actualUserId
        };

        SetupOrderByIdMock(order);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId, wrongUserId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().BeOneOf("ACCESS_DENIED", "FORBIDDEN", "INTERNAL_ERROR");
    }

    [Fact]
    public async Task AcceptOrderAsync_ValidOrder_ShouldAccept()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var merchantOwnerId = Guid.NewGuid();
        
        var merchant = TestDataGenerator.CreateMerchant();
        merchant.OwnerId = merchantOwnerId;
        
        var order = new Order
        {
            Id = orderId,
            MerchantId = merchant.Id,
            Merchant = merchant,
            Status = OrderStatus.Pending
        };

        SetupOrderWithMerchantMock(order);
        SetupOrderUpdateRepositories();

        // Act
        var result = await _orderService.AcceptOrderAsync(orderId, merchantOwnerId);

        // Assert
        result.Success.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task AcceptOrderAsync_NotMerchantOwner_ShouldFail()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var actualOwnerId = Guid.NewGuid();
        var wrongOwnerId = Guid.NewGuid();
        
        var merchant = TestDataGenerator.CreateMerchant();
        merchant.OwnerId = actualOwnerId;
        
        var order = new Order
        {
            Id = orderId,
            MerchantId = merchant.Id,
            Merchant = merchant,
            Status = OrderStatus.Pending
        };

        SetupOrderWithMerchantMock(order);

        // Act
        var result = await _orderService.AcceptOrderAsync(orderId, wrongOwnerId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().BeOneOf("ACCESS_DENIED", "FORBIDDEN", "INTERNAL_ERROR");
    }

    [Fact]
    public async Task RejectOrderAsync_ValidOrder_ShouldReject()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var merchantOwnerId = Guid.NewGuid();
        
        var merchant = TestDataGenerator.CreateMerchant();
        merchant.OwnerId = merchantOwnerId;
        
        var order = new Order
        {
            Id = orderId,
            MerchantId = merchant.Id,
            Merchant = merchant,
            Status = OrderStatus.Pending
        };

        SetupOrderWithMerchantMock(order);
        SetupOrderUpdateRepositories();

        // Act
        var result = await _orderService.RejectOrderAsync(orderId, merchantOwnerId, "Out of ingredients");

        // Assert
        result.Success.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task CancelOrderAsync_ValidOrder_ShouldCancel()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var merchantOwnerId = Guid.NewGuid();
        
        var merchant = TestDataGenerator.CreateMerchant();
        merchant.OwnerId = merchantOwnerId;
        
        var order = new Order
        {
            Id = orderId,
            MerchantId = merchant.Id,
            Merchant = merchant,
            Status = OrderStatus.Confirmed
        };

        SetupOrderWithMerchantMock(order);
        SetupOrderUpdateRepositories();

        // Act
        var result = await _orderService.CancelOrderAsync(orderId, merchantOwnerId, "Customer request");

        // Assert
        result.Success.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task GetUserOrdersAsync_ShouldReturnUserOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Status = OrderStatus.Delivered },
            new() { Id = Guid.NewGuid(), UserId = userId, Status = OrderStatus.Confirmed }
        };

        SetupPagedOrders(orders, totalCount: 2);

        // Act
        var result = await _orderService.GetUserOrdersAsync(userId, new PaginationQuery { Page = 1, PageSize = 10 });

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task StartPreparingOrderAsync_ValidOrder_ShouldUpdateStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var merchantOwnerId = Guid.NewGuid();
        
        var merchant = TestDataGenerator.CreateMerchant();
        merchant.OwnerId = merchantOwnerId;
        
        var order = new Order
        {
            Id = orderId,
            MerchantId = merchant.Id,
            Merchant = merchant,
            Status = OrderStatus.Confirmed
        };

        SetupOrderWithMerchantMock(order);
        SetupOrderUpdateRepositories();

        // Act
        var result = await _orderService.StartPreparingOrderAsync(orderId, merchantOwnerId);

        // Assert
        result.Success.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Preparing);
    }

    [Fact]
    public async Task MarkOrderAsReadyAsync_ValidOrder_ShouldUpdateStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var merchantOwnerId = Guid.NewGuid();
        
        var merchant = TestDataGenerator.CreateMerchant();
        merchant.OwnerId = merchantOwnerId;
        
        var order = new Order
        {
            Id = orderId,
            MerchantId = merchant.Id,
            Merchant = merchant,
            Status = OrderStatus.Preparing
        };

        SetupOrderWithMerchantMock(order);
        SetupOrderUpdateRepositories();

        // Act
        var result = await _orderService.MarkOrderAsReadyAsync(orderId, merchantOwnerId);

        // Assert
        result.Success.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Ready);
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

        // Setup transaction methods to prevent null reference exceptions
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupPaymentServiceMock()
    {
        _paymentServiceMock.Setup(p => p.CreatePaymentAsync(
            It.IsAny<CreatePaymentRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new PaymentResponse(
                Id: Guid.NewGuid(),
                OrderId: Guid.NewGuid(),
                PaymentMethod: PaymentMethod.CreditCard,
                Status: PaymentStatus.Pending,
                Amount: 100,
                ChangeAmount: null,
                ProcessedAt: null,
                CompletedAt: null,
                CollectedAt: null,
                SettledAt: null,
                CollectedByCourierId: null,
                CollectedByCourierName: null,
                Notes: null,
                FailureReason: null,
                ExternalTransactionId: null,
                RefundAmount: null,
                RefundedAt: null,
                RefundReason: null,
                CreatedAt: DateTime.UtcNow)));
    }

    private void SetupUserMock(User user)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<User>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(u => u.ReadRepository<User>()).Returns(readRepoMock.Object);
    }

    private void SetupOrderByIdMock(Order? order)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Order>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Order>()).Returns(readRepoMock.Object);
    }

    private void SetupOrderWithMerchantMock(Order? order)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Order>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Order>()).Returns(readRepoMock.Object);
    }

    private void SetupOrderUpdateRepositories()
    {
        var orderRepoMock = new Mock<IGenericRepository<Order>>();
        _unitOfWorkMock.Setup(u => u.Repository<Order>()).Returns(orderRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupPagedOrders(List<Order> orders, int totalCount)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Order>>();
        readRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Order>()).Returns(readRepoMock.Object);
    }
}
