using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.DeliveryZones;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class DeliveryZoneServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<DeliveryZoneService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly DeliveryZoneService _service;

    public DeliveryZoneServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<DeliveryZoneService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        
        _service = new DeliveryZoneService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object);
    }

    [Fact]
    public async Task GetDeliveryZonesByMerchantAsync_ValidMerchant_ShouldReturnZones()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var zones = new List<DeliveryZone>
        {
            new() { Id = Guid.NewGuid(), MerchantId = merchantId, Name = "Zone 1", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        var readRepoMock = new Mock<IReadOnlyRepository<DeliveryZone>>();
        readRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<DeliveryZone, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<DeliveryZone, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(zones);

        _unitOfWorkMock.Setup(u => u.ReadRepository<DeliveryZone>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetDeliveryZonesByMerchantAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateDeliveryZoneAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var points = new List<DeliveryZonePointRequest>
        {
            new(41.0m, 29.0m, 1),
            new(41.1m, 29.1m, 2),
            new(41.2m, 29.2m, 3)
        };
        var request = new CreateDeliveryZoneRequest(merchantId, "Zone 1", "Description", 10m, 30, points);

        var merchantRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        merchantRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Merchant { Id = merchantId, OwnerId = userId });

        var zoneRepoMock = new Mock<IGenericRepository<DeliveryZone>>();
        zoneRepoMock.Setup(r => r.AddAsync(It.IsAny<DeliveryZone>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryZone());

        var pointRepoMock = new Mock<IGenericRepository<DeliveryZonePoint>>();
        pointRepoMock.Setup(r => r.AddAsync(It.IsAny<DeliveryZonePoint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryZonePoint());

        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(merchantRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<DeliveryZone>()).Returns(zoneRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<DeliveryZonePoint>()).Returns(pointRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateDeliveryZoneAsync(request, userId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateDeliveryZoneAsync_LessThan3Points_ShouldFail()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var points = new List<DeliveryZonePointRequest>
        {
            new(41.0m, 29.0m, 1),
            new(41.1m, 29.1m, 2)
        };
        var request = new CreateDeliveryZoneRequest(merchantId, "Zone 1", "Description", 10m, 30, points);

        var merchantRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        merchantRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Merchant { Id = merchantId, OwnerId = userId });

        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(merchantRepoMock.Object);

        // Act
        var result = await _service.CreateDeliveryZoneAsync(request, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_DELIVERY_ZONE");
    }
}

