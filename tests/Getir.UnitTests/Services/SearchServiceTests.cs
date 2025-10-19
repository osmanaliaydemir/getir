using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Search;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class SearchServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<SearchService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly SearchService _service;

    public SearchServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<SearchService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        
        _service = new SearchService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object);
    }

    [Fact]
    public async Task SearchProductsAsync_WithQuery_ShouldReturnMatchingProducts()
    {
        // Arrange
        var query = new SearchProductsQuery("pizza", null, null, null, null, 1, 10);
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Pizza Margherita", IsActive = true, Price = 50, Merchant = new Merchant { Name = "Test Restaurant" } },
            new() { Id = Guid.NewGuid(), Name = "Pizza Pepperoni", IsActive = true, Price = 60, Merchant = new Merchant { Name = "Test Restaurant" } }
        };

        var productRepoMock = new Mock<IGenericRepository<Product>>();
        productRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var readRepoMock = new Mock<IReadOnlyRepository<Product>>();
        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.SearchProductsAsync(query);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Total.Should().Be(2);
    }

    [Fact]
    public async Task SearchProductsAsync_EmptyQuery_ShouldReturnAllProducts()
    {
        // Arrange
        var query = new SearchProductsQuery("", null, null, null, null, 1, 10);
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1", IsActive = true, Price = 50, Merchant = new Merchant { Name = "Test" } }
        };

        var productRepoMock = new Mock<IGenericRepository<Product>>();
        productRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var readRepoMock = new Mock<IReadOnlyRepository<Product>>();
        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.SearchProductsAsync(query);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchMerchantsAsync_WithQuery_ShouldReturnMatchingMerchants()
    {
        // Arrange
        var query = new SearchMerchantsQuery("restaurant", null, null, null, null, 1, 10);
        var merchants = new List<Merchant>
        {
            new() { Id = Guid.NewGuid(), Name = "Best Restaurant", IsActive = true, Rating = 4.5m, ServiceCategory = new ServiceCategory { Name = "Food" }, Owner = new User { FirstName = "Owner", LastName = "User" } }
        };

        var merchantRepoMock = new Mock<IGenericRepository<Merchant>>();
        merchantRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchants);

        var readRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _unitOfWorkMock.Setup(u => u.Repository<Merchant>()).Returns(merchantRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.SearchMerchantsAsync(query);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchMerchantsAsync_NoResults_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new SearchMerchantsQuery("nonexistent", null, null, null, null, 1, 10);

        var merchantRepoMock = new Mock<IGenericRepository<Merchant>>();
        merchantRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Merchant>());

        var readRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _unitOfWorkMock.Setup(u => u.Repository<Merchant>()).Returns(merchantRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.SearchMerchantsAsync(query);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.Total.Should().Be(0);
    }
}

