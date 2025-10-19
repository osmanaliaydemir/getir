using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.Services.Orders;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Getir.UnitTests.Services;

public class OrderStatusValidatorServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<OrderStatusValidatorService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly OrderStatusValidatorService _service;

    public OrderStatusValidatorServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<OrderStatusValidatorService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();

        _service = new OrderStatusValidatorService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object);
    }

    #region GetValidNextStatusesAsync Tests

    [Fact]
    public async Task GetValidNextStatusesAsync_ValidOrder_ShouldReturnStatuses()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = CreateSampleOrder(orderId, OrderStatus.Pending);

        SetupOrderExists(order);

        // Act
        var result = await _service.GetValidNextStatusesAsync(orderId, userId, "Admin");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        // Pending can transition to Confirmed or Cancelled
        result.Value.Should().Contain(OrderStatus.Confirmed);
        result.Value.Should().Contain(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task GetValidNextStatusesAsync_OrderNotFound_ShouldFail()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupOrderNotExists();

        // Act
        var result = await _service.GetValidNextStatusesAsync(orderId, userId, "Admin");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ORDER_NOT_FOUND");
    }

    #endregion

    #region GetRequiredTransitionDataAsync Tests

    [Fact]
    public async Task GetRequiredTransitionDataAsync_PendingToConfirmed_ShouldRequireEstimatedTime()
    {
        // Arrange
        var fromStatus = OrderStatus.Pending;
        var toStatus = OrderStatus.Confirmed;

        // Act
        var result = await _service.GetRequiredTransitionDataAsync(fromStatus, toStatus);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().Contain("EstimatedPreparationTime");
    }

    [Fact]
    public async Task GetRequiredTransitionDataAsync_AnyCancelled_ShouldRequireCancellationReason()
    {
        // Arrange
        var fromStatus = OrderStatus.Pending;
        var toStatus = OrderStatus.Cancelled;

        // Act
        var result = await _service.GetRequiredTransitionDataAsync(fromStatus, toStatus);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().Contain("CancellationReason");
    }

    #endregion

    #region Helper Methods

    private Order CreateSampleOrder(Guid orderId, OrderStatus status)
    {
        var merchantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        return new Order
        {
            Id = orderId,
            OrderNumber = "ORD-12345",
            MerchantId = merchantId,
            UserId = userId,
            Status = status,
            Total = 100m,
            DeliveryFee = 10m,
            EstimatedDeliveryTime = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow,
            Merchant = new Merchant
            {
                Id = merchantId,
                Name = "Test Merchant",
                OwnerId = ownerId,
                IsActive = true,
                ServiceCategory = new ServiceCategory
                {
                    Id = Guid.NewGuid(),
                    Name = "Food"
                }
            },
            User = new User
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe"
            }
        };
    }

    private void SetupOrderExists(Order order)
    {
        var orderRepo = new Mock<IReadOnlyRepository<Order>>();
        orderRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Order>()).Returns(orderRepo.Object);
    }

    private void SetupOrderNotExists()
    {
        var orderRepo = new Mock<IReadOnlyRepository<Order>>();
        orderRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Order>()).Returns(orderRepo.Object);
    }

    #endregion
}

