using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Payments;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Getir.UnitTests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

/// <summary>
/// Comprehensive unit tests for PaymentService
/// Tests payment creation, status updates, retrieval, and settlement operations
/// </summary>
public class PaymentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<PaymentService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ISignalRService> _signalRServiceMock;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<PaymentService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _signalRServiceMock = new Mock<ISignalRService>();
        
        _paymentService = new PaymentService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _signalRServiceMock.Object);
    }

    #region CreatePaymentAsync Tests

    [Fact]
    public async Task CreatePaymentAsync_ValidCashPayment_ShouldSucceed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Total = 100m,
            Status = OrderStatus.Confirmed
        };

        var request = new CreatePaymentRequest(
            orderId,
            PaymentMethod.Cash,
            100m,
            10m,
            "Change needed");

        SetupOrderMock(order);
        SetupPaymentRepositories();

        // Act
        var result = await _paymentService.CreatePaymentAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.OrderId.Should().Be(orderId);
        result.Value.PaymentMethod.Should().Be(PaymentMethod.Cash);
        result.Value.Status.Should().Be(PaymentStatus.Pending);
        result.Value.Amount.Should().Be(100m);
        result.Value.ChangeAmount.Should().Be(10m);

        _unitOfWorkMock.Verify(u => u.Repository<Payment>()
            .AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentAsync_OrderNotFound_ShouldFail()
    {
        // Arrange
        var request = new CreatePaymentRequest(
            Guid.NewGuid(),
            PaymentMethod.Cash,
            100m,
            0m,
            null);

        SetupOrderMock(null);

        // Act
        var result = await _paymentService.CreatePaymentAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ORDER_NOT_FOUND");
    }

    [Fact]
    public async Task CreatePaymentAsync_UnsupportedPaymentMethod_ShouldFail()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Total = 100m
        };

        var request = new CreatePaymentRequest(
            orderId,
            PaymentMethod.CreditCard, // Not yet supported
            100m,
            0m,
            null);

        SetupOrderMock(order);

        // Act
        var result = await _paymentService.CreatePaymentAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("PAYMENT_METHOD_NOT_SUPPORTED");
    }

    #endregion

    #region UpdatePaymentStatusAsync Tests

    [Fact]
    public async Task UpdatePaymentStatusAsync_ValidUpdate_ShouldSucceed()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        
        var payment = new Payment
        {
            Id = paymentId,
            OrderId = orderId,
            Status = PaymentStatus.Pending,
            Amount = 100m,
            PaymentMethod = PaymentMethod.Cash
        };

        var order = new Order
        {
            Id = orderId,
            UserId = Guid.NewGuid()
        };

        var request = new PaymentStatusUpdateRequest(
            PaymentStatus.Completed,
            "Payment completed");

        SetupPaymentWithOrderMock(payment, order); // This already sets up SaveChangesAsync

        // Act
        var result = await _paymentService.UpdatePaymentStatusAsync(paymentId, request);

        // Assert
        result.Success.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdatePaymentStatusAsync_PaymentNotFound_ShouldFail()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var request = new PaymentStatusUpdateRequest(
            PaymentStatus.Completed,
            "Test");

        SetupPaymentWithOrderMock(null, null);

        // Act
        var result = await _paymentService.UpdatePaymentStatusAsync(paymentId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND_PAYMENT");
    }

    [Fact]
    public async Task UpdatePaymentStatusAsync_ToFailed_ShouldSetFailureReason()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            OrderId = Guid.NewGuid(),
            Status = PaymentStatus.Pending,
            PaymentMethod = PaymentMethod.Cash
        };

        var order = new Order
        {
            Id = payment.OrderId,
            UserId = Guid.NewGuid()
        };

        var request = new PaymentStatusUpdateRequest(
            PaymentStatus.Failed,
            null,
            "Insufficient funds");

        SetupPaymentWithOrderMock(payment, order); // This already sets up SaveChangesAsync

        // Act
        var result = await _paymentService.UpdatePaymentStatusAsync(paymentId, request);

        // Assert
        result.Success.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be("Insufficient funds");
    }

    #endregion

    #region GetPaymentByIdAsync Tests

    [Fact]
    public async Task GetPaymentByIdAsync_ValidId_ShouldReturnPayment()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            OrderId = Guid.NewGuid(),
            Status = PaymentStatus.Completed,
            Amount = 150m,
            PaymentMethod = PaymentMethod.Cash,
            CreatedAt = DateTime.UtcNow
        };

        SetupPaymentByIdMock(payment);

        // Act
        var result = await _paymentService.GetPaymentByIdAsync(paymentId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(paymentId);
        result.Value.Amount.Should().Be(150m);
        result.Value.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task GetPaymentByIdAsync_InvalidId_ShouldFail()
    {
        // Arrange
        SetupPaymentByIdMock(null);

        // Act
        var result = await _paymentService.GetPaymentByIdAsync(Guid.NewGuid());

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND_PAYMENT");
    }

    #endregion

    #region GetOrderPaymentsAsync Tests

    [Fact]
    public async Task GetOrderPaymentsAsync_ValidOrder_ShouldReturnPayments()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var payments = new List<Payment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Amount = 100m,
                Status = PaymentStatus.Completed,
                PaymentMethod = PaymentMethod.Cash,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Amount = 100m,
                Status = PaymentStatus.Failed,
                PaymentMethod = PaymentMethod.Cash,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        SetupOrderPaymentsPagedMock(payments, totalCount: 2);

        // Act
        var result = await _paymentService.GetOrderPaymentsAsync(orderId, new PaginationQuery { Page = 1, PageSize = 10 });

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetOrderPaymentsAsync_NoPayments_ShouldReturnEmptyList()
    {
        // Arrange
        SetupOrderPaymentsPagedMock(new List<Payment>(), totalCount: 0);

        // Act
        var result = await _paymentService.GetOrderPaymentsAsync(Guid.NewGuid(), new PaginationQuery { Page = 1, PageSize = 10 });

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
        result.Value.Total.Should().Be(0);
    }

    #endregion

    #region Helper Methods

    private void SetupOrderMock(Order? order)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Order>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Order>()).Returns(readRepoMock.Object);
    }

    private void SetupPaymentRepositories()
    {
        var paymentRepoMock = new Mock<IGenericRepository<Payment>>();
        var orderRepoMock = new Mock<IGenericRepository<Order>>();
        var userRepoMock = new Mock<IGenericRepository<User>>();

        _unitOfWorkMock.Setup(u => u.Repository<Payment>()).Returns(paymentRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Order>()).Returns(orderRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<User>()).Returns(userRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupPaymentWithOrderMock(Payment? payment, Order? order)
    {
        var paymentRepoMock = new Mock<IGenericRepository<Payment>>();
        paymentRepoMock.Setup(r => r.GetByIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _unitOfWorkMock.Setup(u => u.Repository<Payment>()).Returns(paymentRepoMock.Object);

        if (order != null)
        {
            var orderRepoMock = new Mock<IGenericRepository<Order>>();
            orderRepoMock.Setup(r => r.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _unitOfWorkMock.Setup(u => u.Repository<Order>()).Returns(orderRepoMock.Object);
        }
        
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupPaymentByIdMock(Payment? payment)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Payment>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Payment>()).Returns(readRepoMock.Object);
    }

    private void SetupOrderPaymentsPagedMock(List<Payment> payments, int totalCount)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Payment>>();
        
        readRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Payment, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Payment>()).Returns(readRepoMock.Object);
    }

    #endregion
}
