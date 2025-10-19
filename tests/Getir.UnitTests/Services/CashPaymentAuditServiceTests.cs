using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Payments;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Getir.UnitTests.Services;

public class CashPaymentAuditServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CashPaymentAuditService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly CashPaymentAuditService _service;

    public CashPaymentAuditServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CashPaymentAuditService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();

        _service = new CashPaymentAuditService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object);
    }

    #region CreateAuditLogAsync Tests

    [Fact]
    public async Task CreateAuditLogAsync_ValidRequest_ShouldCreateSuccessfully()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateAuditLogRequest(
            PaymentId: paymentId,
            CourierId: null,
            CustomerId: null,
            AdminId: userId,
            EventType: AuditEventType.CashPaymentCompleted,
            SeverityLevel: AuditSeverityLevel.Information,
            Title: "Payment Completed",
            Description: "Cash payment completed successfully",
            IpAddress: "192.168.1.1",
            UserAgent: "Test Agent");

        SetupAuditLogRepository();

        // Act
        var result = await _service.CreateAuditLogAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private void SetupAuditLogRepository()
    {
        var auditLogRepo = new Mock<IGenericRepository<CashPaymentAuditLog>>();
        auditLogRepo
            .Setup(r => r.AddAsync(It.IsAny<CashPaymentAuditLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CashPaymentAuditLog entity, CancellationToken ct) => entity);
        _unitOfWorkMock.Setup(u => u.Repository<CashPaymentAuditLog>()).Returns(auditLogRepo.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    #endregion
}

