using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Stock;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DomainEnums = Getir.Domain.Enums;

namespace Getir.UnitTests.Services;

public class StockAlertServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<StockAlertService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ISignalRService> _signalRServiceMock;
    private readonly StockAlertService _service;

    public StockAlertServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<StockAlertService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _signalRServiceMock = new Mock<ISignalRService>();
        
        _service = new StockAlertService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _signalRServiceMock.Object);
    }

    [Fact]
    public async Task CreateLowStockAlertsAsync_WithLowStockProducts_ShouldCreateAlerts()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        
        var merchantRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        merchantRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Merchant { Id = merchantId });

        var settingsRepoMock = new Mock<IReadOnlyRepository<StockSettings>>();
        settingsRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StockSettings, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockSettings { MerchantId = merchantId, LowStockAlerts = true, DefaultMinimumStock = 10, IsActive = true });

        var productRepoMock = new Mock<IReadOnlyRepository<Product>>();
        productRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>
            {
                new() { Id = Guid.NewGuid(), Name = "Product 1", StockQuantity = 5, IsActive = true }
            });

        var alertReadRepoMock = new Mock<IReadOnlyRepository<StockAlert>>();
        alertReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StockAlert, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockAlert?)null);

        var alertRepoMock = new Mock<IGenericRepository<StockAlert>>();
        alertRepoMock.Setup(r => r.AddAsync(It.IsAny<StockAlert>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockAlert());

        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(merchantRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<StockSettings>()).Returns(settingsRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(productRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<StockAlert>()).Returns(alertReadRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<StockAlert>()).Returns(alertRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateLowStockAlertsAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateLowStockAlertsAsync_MerchantNotFound_ShouldFail()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        
        var merchantRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        merchantRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Merchant?)null);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(merchantRepoMock.Object);

        // Act
        var result = await _service.CreateLowStockAlertsAsync(merchantId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("MERCHANT_NOT_FOUND");
    }

    [Fact]
    public async Task ResolveStockAlertAsync_ValidAlert_ShouldResolve()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var userRepoMock = new Mock<IReadOnlyRepository<User>>();
        userRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        var alertReadRepoMock = new Mock<IReadOnlyRepository<StockAlert>>();
        alertReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StockAlert, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockAlert { Id = alertId, IsResolved = false });

        var alertRepoMock = new Mock<IGenericRepository<StockAlert>>();
        alertRepoMock.Setup(r => r.Update(It.IsAny<StockAlert>()));

        _unitOfWorkMock.Setup(u => u.ReadRepository<User>()).Returns(userRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<StockAlert>()).Returns(alertReadRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<StockAlert>()).Returns(alertRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.ResolveStockAlertAsync(alertId, userId, "Resolved");

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ResolveStockAlertAsync_AlertNotFound_ShouldFail()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var userRepoMock = new Mock<IReadOnlyRepository<User>>();
        userRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        var alertReadRepoMock = new Mock<IReadOnlyRepository<StockAlert>>();
        alertReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StockAlert, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockAlert?)null);

        _unitOfWorkMock.Setup(u => u.ReadRepository<User>()).Returns(userRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<StockAlert>()).Returns(alertReadRepoMock.Object);

        // Act
        var result = await _service.ResolveStockAlertAsync(alertId, userId, "Resolved");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ALERT_NOT_FOUND");
    }

    [Fact]
    public async Task GetStockAlertStatisticsAsync_ValidMerchant_ShouldReturnStats()
    {
        // Arrange
        var merchantId = Guid.NewGuid();

        var merchantRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        merchantRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Merchant { Id = merchantId });

        var alertRepoMock = new Mock<IReadOnlyRepository<StockAlert>>();
        alertRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StockAlert, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<StockAlert, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StockAlert>
            {
                new() { Id = Guid.NewGuid(), MerchantId = merchantId, AlertType = DomainEnums.StockAlertType.LowStock, IsResolved = false, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), MerchantId = merchantId, AlertType = DomainEnums.StockAlertType.OutOfStock, IsResolved = true, CreatedAt = DateTime.UtcNow }
            });

        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(merchantRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<StockAlert>()).Returns(alertRepoMock.Object);

        // Act
        var result = await _service.GetStockAlertStatisticsAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.TotalAlerts.Should().Be(2);
        result.Value.PendingAlerts.Should().Be(1);
        result.Value.ResolvedAlerts.Should().Be(1);
    }
}

