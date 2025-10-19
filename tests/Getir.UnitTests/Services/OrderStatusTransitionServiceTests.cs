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

public class OrderStatusTransitionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<OrderStatusTransitionService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IOrderStatusValidatorService> _validatorServiceMock;
    private readonly OrderStatusTransitionService _service;

    public OrderStatusTransitionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<OrderStatusTransitionService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _validatorServiceMock = new Mock<IOrderStatusValidatorService>();

        _service = new OrderStatusTransitionService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _validatorServiceMock.Object);
    }

    #region GetOrderStatusHistoryAsync Tests

    [Fact]
    public async Task GetOrderStatusHistoryAsync_ValidOrder_ShouldReturnHistory()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var logs = new List<OrderStatusTransitionLog>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                FromStatus = OrderStatus.Pending,
                ToStatus = OrderStatus.Confirmed,
                ChangedBy = Guid.NewGuid(),
                ChangedAt = DateTime.UtcNow,
                Reason = "Order confirmed"
            }
        };

        SetupTransitionLogsList(logs);

        // Act
        var result = await _service.GetOrderStatusHistoryAsync(orderId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value.First().FromStatus.Should().Be(OrderStatus.Pending);
        result.Value.First().ToStatus.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task GetOrderStatusHistoryAsync_NoHistory_ShouldReturnEmptyList()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        SetupTransitionLogsList(new List<OrderStatusTransitionLog>());

        // Act
        var result = await _service.GetOrderStatusHistoryAsync(orderId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private void SetupTransitionLogsList(List<OrderStatusTransitionLog> logs)
    {
        var logsRepo = new Mock<IReadOnlyRepository<OrderStatusTransitionLog>>();
        logsRepo
            .Setup(r => r.ListAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<OrderStatusTransitionLog, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<OrderStatusTransitionLog, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);
        _unitOfWorkMock.Setup(u => u.ReadRepository<OrderStatusTransitionLog>()).Returns(logsRepo.Object);
    }

    #endregion
}

