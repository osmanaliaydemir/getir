using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Merchants;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class MerchantDashboardServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<MerchantDashboardService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly MerchantDashboardService _service;

    public MerchantDashboardServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<MerchantDashboardService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        
        _service = new MerchantDashboardService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object);
    }

    [Fact]
    public async Task GetDashboardAsync_ValidMerchant_ShouldReturnDashboard()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var merchant = new Merchant { Id = merchantId, OwnerId = userId, Name = "Test", IsOpen = true, Rating = 4.5m };

        var merchantReadRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        merchantReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchant);

        var merchantRepoMock = new Mock<IGenericRepository<Merchant>>();
        merchantRepoMock.Setup(r => r.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchant);

        var orderReadRepoMock = new Mock<IReadOnlyRepository<Order>>();
        orderReadRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        orderReadRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>
            {
                new() { Id = Guid.NewGuid(), Total = 100m, Status = OrderStatus.Delivered }
            });

        orderReadRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        var productReadRepoMock = new Mock<IReadOnlyRepository<Product>>();
        productReadRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        productReadRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(merchantReadRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Merchant>()).Returns(merchantRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Order>()).Returns(orderReadRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(productReadRepoMock.Object);

        // Act
        var result = await _service.GetDashboardAsync(merchantId, userId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDashboardAsync_UnauthorizedUser_ShouldFail()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var merchantReadRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        merchantReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Merchant?)null);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(merchantReadRepoMock.Object);

        // Act
        var result = await _service.GetDashboardAsync(merchantId, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND_MERCHANT");
    }
}

