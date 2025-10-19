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

public class CashPaymentSecurityServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CashPaymentSecurityService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ICashPaymentAuditService> _auditServiceMock;
    private readonly CashPaymentSecurityService _service;

    public CashPaymentSecurityServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CashPaymentSecurityService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _auditServiceMock = new Mock<ICashPaymentAuditService>();

        _service = new CashPaymentSecurityService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _fileStorageServiceMock.Object,
            _auditServiceMock.Object);
    }

    #region CalculateChangeAsync Tests

    [Fact]
    public async Task CalculateChangeAsync_ValidRequest_ShouldCalculateChange()
    {
        // Arrange
        var request = new CalculateChangeRequest(
            OrderAmount: 100m,
            GivenAmount: 150m);

        // Act
        var result = await _service.CalculateChangeAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ChangeAmount.Should().Be(50m);
        result.Value.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CalculateChangeAsync_InsufficientAmount_ShouldReturnInvalid()
    {
        // Arrange
        var request = new CalculateChangeRequest(
            OrderAmount: 100m,
            GivenAmount: 80m); // Less than total

        // Act
        var result = await _service.CalculateChangeAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsValid.Should().BeFalse();
    }

    #endregion

    #region Helper Methods (minimal)
    // Minimal helpers for simple tests
    #endregion
}

