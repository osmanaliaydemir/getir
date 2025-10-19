using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.Services.Stock;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Getir.UnitTests.Services;

public class StockSyncServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<StockSyncService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly StockSyncService _service;

    public StockSyncServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<StockSyncService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();

        _service = new StockSyncService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object);
    }

    #region GetSynchronizationStatusAsync Tests

    [Fact]
    public async Task GetSynchronizationStatusAsync_ValidMerchant_ShouldReturnStatus()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var settings = new StockSettings
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            EnableStockSync = true,
            ExternalSystemId = "TEST_SYSTEM",
            IsActive = true,
            SyncIntervalMinutes = 60
        };

        SetupStockSettingsExists(settings);
        SetupSyncSessionExists(null);

        // Act
        var result = await _service.GetSynchronizationStatusAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsEnabled.Should().BeTrue();
        result.Value.ExternalSystemId.Should().Be("TEST_SYSTEM");
    }

    [Fact]
    public async Task GetSynchronizationStatusAsync_NotConfigured_ShouldFail()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        SetupStockSettingsNotExists();

        // Act
        var result = await _service.GetSynchronizationStatusAsync(merchantId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("SYNC_NOT_CONFIGURED");
    }

    #endregion

    #region GetSynchronizationHistoryAsync Tests

    [Fact]
    public async Task GetSynchronizationHistoryAsync_ValidMerchant_ShouldReturnHistory()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var sessions = new List<StockSyncSession>
        {
            new()
            {
                Id = Guid.NewGuid(),
                MerchantId = merchantId,
                ExternalSystemId = "TEST",
                SyncType = StockSyncType.Manual,
                Status = StockSyncStatus.Success,
                StartedAt = DateTime.UtcNow.AddHours(-1),
                CompletedAt = DateTime.UtcNow,
                SyncedItemsCount = 10,
                FailedItemsCount = 0
            }
        };

        SetupSyncSessionsList(sessions);

        // Act
        var result = await _service.GetSynchronizationHistoryAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value.First().Status.Should().Be(StockSyncStatus.Success);
    }

    #endregion

    #region Helper Methods

    private void SetupStockSettingsExists(StockSettings settings)
    {
        var settingsRepo = new Mock<IReadOnlyRepository<StockSettings>>();
        settingsRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<StockSettings, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);
        _unitOfWorkMock.Setup(u => u.ReadRepository<StockSettings>()).Returns(settingsRepo.Object);
    }

    private void SetupStockSettingsNotExists()
    {
        var settingsRepo = new Mock<IReadOnlyRepository<StockSettings>>();
        settingsRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<StockSettings, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockSettings?)null);
        _unitOfWorkMock.Setup(u => u.ReadRepository<StockSettings>()).Returns(settingsRepo.Object);
    }

    private void SetupSyncSessionExists(StockSyncSession? session)
    {
        var sessionRepo = new Mock<IReadOnlyRepository<StockSyncSession>>();
        sessionRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<StockSyncSession, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _unitOfWorkMock.Setup(u => u.ReadRepository<StockSyncSession>()).Returns(sessionRepo.Object);
    }

    private void SetupSyncSessionsList(List<StockSyncSession> sessions)
    {
        var sessionRepo = new Mock<IReadOnlyRepository<StockSyncSession>>();
        sessionRepo
            .Setup(r => r.ListAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<StockSyncSession, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<StockSyncSession, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);
        _unitOfWorkMock.Setup(u => u.ReadRepository<StockSyncSession>()).Returns(sessionRepo.Object);
    }

    #endregion
}

