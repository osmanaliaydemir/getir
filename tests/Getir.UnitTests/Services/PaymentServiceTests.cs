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

public class PaymentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<PaymentService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<PaymentService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        
        _paymentService = new PaymentService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object);
    }

    // CreatePaymentAsync test'leri - Payment validation karmaşık, şimdilik skip

    [Fact]
    public async Task GetPaymentByIdAsync_ValidPayment_ShouldReturnPayment()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = TestDataGenerator.CreatePayment(paymentId, PaymentStatus.Completed);

        SetupPaymentRepositories();
        SetupPaymentMock(payment);

        // Act
        var result = await _paymentService.GetPaymentByIdAsync(paymentId, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(paymentId);
        result.Value.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task GetPaymentByIdAsync_InvalidPaymentId_ShouldReturnNotFound()
    {
        // Arrange
        var paymentId = Guid.NewGuid();

        SetupPaymentRepositories();
        SetupPaymentMock(null);

        // Act
        var result = await _paymentService.GetPaymentByIdAsync(paymentId, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND_PAYMENT");
    }

    private void SetupPaymentRepositories()
    {
        var paymentRepoMock = new Mock<IGenericRepository<Payment>>();
        var orderRepoMock = new Mock<IGenericRepository<Order>>();

        _unitOfWorkMock.Setup(u => u.Repository<Payment>()).Returns(paymentRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Order>()).Returns(orderRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupPaymentMock(Payment? payment)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Payment>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Payment>()).Returns(readRepoMock.Object);
    }
}

