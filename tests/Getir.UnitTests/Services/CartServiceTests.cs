using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Cart;
using Getir.Domain.Entities;
using Getir.UnitTests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class CartServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CartService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly CartService _cartService;

    public CartServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CartService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        
        _cartService = new CartService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object);
    }

    [Fact]
    public async Task AddItemAsync_ToEmptyCart_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var merchant = TestDataGenerator.CreateMerchant();
        var product = TestDataGenerator.CreateProduct(merchant.Id, stockQuantity: 100);
        
        var request = new AddToCartRequest(merchant.Id, product.Id, 2, "Test notes");

        var readCartRepoMock = new Mock<IReadOnlyRepository<CartItem>>();
        readCartRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>(),
            null, true, null, null,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CartItem>());

        var readProductRepoMock = new Mock<IReadOnlyRepository<Product>>();
        readProductRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var cartRepoMock = new Mock<IGenericRepository<CartItem>>();
        cartRepoMock.Setup(r => r.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);

        _unitOfWorkMock.Setup(u => u.ReadRepository<CartItem>()).Returns(readCartRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(readProductRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<CartItem>()).Returns(cartRepoMock.Object);

        // Act
        var result = await _cartService.AddItemAsync(userId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Quantity.Should().Be(2);
        
        cartRepoMock.Verify(r => r.AddAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_DifferentMerchant_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var merchant1 = TestDataGenerator.CreateMerchant();
        var merchant2 = TestDataGenerator.CreateMerchant();
        var product = TestDataGenerator.CreateProduct(merchant2.Id);
        
        var existingCartItem = TestDataGenerator.CreateCartItem(userId, merchant1.Id, Guid.NewGuid());
        
        var request = new AddToCartRequest(merchant2.Id, product.Id, 1, null);

        var readCartRepoMock = new Mock<IReadOnlyRepository<CartItem>>();
        readCartRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>(),
            null, true, null, null,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CartItem> { existingCartItem });

        _unitOfWorkMock.Setup(u => u.ReadRepository<CartItem>()).Returns(readCartRepoMock.Object);

        // Act
        var result = await _cartService.AddItemAsync(userId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("CART_DIFFERENT_MERCHANT");
        result.Error.Should().Contain("one merchant at a time");
    }

    [Fact]
    public async Task AddItemAsync_InsufficientStock_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var merchant = TestDataGenerator.CreateMerchant();
        var product = TestDataGenerator.CreateProduct(merchant.Id, stockQuantity: 5);
        
        var request = new AddToCartRequest(merchant.Id, product.Id, 10, null);

        var readCartRepoMock = new Mock<IReadOnlyRepository<CartItem>>();
        readCartRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>(),
            null, true, null, null,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CartItem>());

        var readProductRepoMock = new Mock<IReadOnlyRepository<Product>>();
        readProductRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock.Setup(u => u.ReadRepository<CartItem>()).Returns(readCartRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(readProductRepoMock.Object);

        // Act
        var result = await _cartService.AddItemAsync(userId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INSUFFICIENT_STOCK");
    }

    [Fact]
    public async Task ClearCartAsync_WithItems_ShouldRemoveAll()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cartItems = new List<CartItem>
        {
            TestDataGenerator.CreateCartItem(userId, Guid.NewGuid(), Guid.NewGuid()),
            TestDataGenerator.CreateCartItem(userId, Guid.NewGuid(), Guid.NewGuid())
        };

        var cartRepoMock = new Mock<IGenericRepository<CartItem>>();
        cartRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>(),
            null, true, 1, 20, null,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItems);

        _unitOfWorkMock.Setup(u => u.Repository<CartItem>()).Returns(cartRepoMock.Object);

        // Act
        var result = await _cartService.ClearCartAsync(userId);

        // Assert
        result.Success.Should().BeTrue();
        cartRepoMock.Verify(r => r.RemoveRange(It.IsAny<IEnumerable<CartItem>>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
