using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Couriers;
using Getir.Domain.Entities;
using Getir.UnitTests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class CourierServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CourierService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly Mock<ISignalRService> _signalRServiceMock;
    private readonly CourierService _courierService;

    public CourierServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CourierService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        _signalRServiceMock = new Mock<ISignalRService>();
        
        _courierService = new CourierService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object,
            _signalRServiceMock.Object);
    }

    [Fact]
    public async Task GetCourierByIdAsync_ValidCourier_ShouldReturnCourier()
    {
        // Arrange
        var courierId = Guid.NewGuid();
        var courier = TestDataGenerator.CreateCourier();
        courier.Id = courierId;
        courier.VehicleType = "Motorcycle";
        courier.IsAvailable = true;

        SetupCourierRepositories();
        SetupCourierMock(courier);

        // Act
        var result = await _courierService.GetCourierByIdAsync(courierId, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(courierId);
        result.Value.VehicleType.Should().Be("Motorcycle");
    }

    [Fact]
    public async Task GetCourierByIdAsync_InvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var courierId = Guid.NewGuid();

        SetupCourierRepositories();
        SetupCourierMock(null);

        // Act
        var result = await _courierService.GetCourierByIdAsync(courierId, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND_COURIER");
    }

    [Fact]
    public async Task GetCouriersByAvailabilityAsync_OnlyAvailable_ShouldReturnAvailableCouriers()
    {
        // Arrange
        var couriers = new List<Courier>
        {
            TestDataGenerator.CreateCourier(isAvailable: true),
            TestDataGenerator.CreateCourier(isAvailable: true)
        };

        SetupCourierRepositories();
        SetupCouriersListMock(couriers);

        // Act
        var result = await _courierService.GetCouriersByAvailabilityAsync(
            true, 
            new PaginationQuery { Page = 1, PageSize = 10 }, 
            CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.Should().AllSatisfy(c => c.IsAvailable.Should().BeTrue());
    }

    [Fact]
    public async Task AssignCourierToOrderAsync_ValidAssignment_ShouldSucceed()
    {
        // Arrange
        var courierId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var courier = TestDataGenerator.CreateCourier(isAvailable: true);
        courier.Id = courierId;
        var order = TestDataGenerator.CreateOrder();
        order.Id = orderId;

        SetupCourierRepositories();
        SetupCourierMock(courier);
        SetupOrderMock(order);

        // Act
        var result = await _courierService.AssignCourierToOrderAsync(
            orderId, 
            courierId, 
            CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task AssignCourierToOrderAsync_UnavailableCourier_ShouldFail()
    {
        // Arrange
        var courierId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var courier = TestDataGenerator.CreateCourier(isAvailable: false);
        courier.Id = courierId;

        SetupCourierRepositories();
        SetupCourierMock(courier);

        // Act
        var result = await _courierService.AssignCourierToOrderAsync(
            orderId, 
            courierId, 
            CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("COURIER_NOT_AVAILABLE");
    }

    private void SetupCourierRepositories()
    {
        var courierRepoMock = new Mock<IGenericRepository<Courier>>();
        var orderRepoMock = new Mock<IGenericRepository<Order>>();
        var courierLocationRepoMock = new Mock<IGenericRepository<CourierLocation>>();

        _unitOfWorkMock.Setup(u => u.Repository<Courier>()).Returns(courierRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Order>()).Returns(orderRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<CourierLocation>()).Returns(courierLocationRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupCourierMock(Courier? courier)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Courier>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Courier, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(courier);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Courier>()).Returns(readRepoMock.Object);
    }

    private void SetupCouriersListMock(List<Courier> couriers)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Courier>>();
        readRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Courier, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Courier, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(couriers);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Courier>()).Returns(readRepoMock.Object);
    }

    private void SetupOrderMock(Order order)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Order>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Order>()).Returns(readRepoMock.Object);
    }
}

