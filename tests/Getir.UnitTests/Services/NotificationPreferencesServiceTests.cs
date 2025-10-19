using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Notifications;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Getir.UnitTests.Services;

public class NotificationPreferencesServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<NotificationPreferencesService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly NotificationPreferencesService _service;

    public NotificationPreferencesServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<NotificationPreferencesService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();

        _service = new NotificationPreferencesService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object);
    }

    #region GetUserPreferencesAsync Tests

    [Fact]
    public async Task GetUserPreferencesAsync_ExistingPreferences_ShouldReturnPreferences()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = CreateSamplePreferences(userId);

        SetupPreferencesExists(preferences);

        // Act
        var result = await _service.GetUserPreferencesAsync(userId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().Be(userId);
        result.Value.EmailEnabled.Should().BeTrue();
        result.Value.SmsEnabled.Should().BeTrue();
        result.Value.PushEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserPreferencesAsync_NoPreferences_ShouldCreateDefaults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = CreateSamplePreferences(userId);

        SetupPreferencesNotExistsThenExists(preferences);
        SetupPreferencesRepository();

        // Act
        var result = await _service.GetUserPreferencesAsync(userId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().Be(userId);
    }

    #endregion

    #region UpdateUserPreferencesAsync Tests

    [Fact]
    public async Task UpdateUserPreferencesAsync_ValidUpdate_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = CreateSamplePreferences(userId);
        var request = new UpdateNotificationPreferencesRequest(
            EmailEnabled: false,
            EmailPromotions: false,
            PushEnabled: true);

        SetupPreferencesExists(preferences);
        SetupPreferencesUpdateRepository();

        // Act
        var result = await _service.UpdateUserPreferencesAsync(userId, request);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserPreferencesAsync_PreferencesNotFound_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateNotificationPreferencesRequest(EmailEnabled: false);

        SetupPreferencesNotExists();

        // Act
        var result = await _service.UpdateUserPreferencesAsync(userId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("PREFERENCES_NOT_FOUND");
    }

    #endregion

    #region ResetToDefaultsAsync Tests

    [Fact]
    public async Task ResetToDefaultsAsync_ValidUser_ShouldResetToDefaults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = CreateSamplePreferences(userId);
        preferences.EmailPromotions = false; // Changed from default
        preferences.SmsPromotions = true;    // Changed from default

        SetupPreferencesExists(preferences);
        SetupPreferencesUpdateRepository();

        // Act
        var result = await _service.ResetToDefaultsAsync(userId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ResetToDefaultsAsync_PreferencesNotFound_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupPreferencesNotExists();

        // Act
        var result = await _service.ResetToDefaultsAsync(userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("PREFERENCES_NOT_FOUND");
    }

    #endregion

    #region CreateDefaultPreferencesAsync Tests

    [Fact]
    public async Task CreateDefaultPreferencesAsync_NewUser_ShouldCreateDefaults()
    {
        // Arrange
        var userId = Guid.NewGuid();

        SetupPreferencesNotExists();
        SetupPreferencesRepository();

        // Act
        var result = await _service.CreateDefaultPreferencesAsync(userId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateDefaultPreferencesAsync_ExistingPreferences_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = CreateSamplePreferences(userId);

        SetupPreferencesExists(preferences);

        // Act
        var result = await _service.CreateDefaultPreferencesAsync(userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("PREFERENCES_ALREADY_EXIST");
    }

    #endregion

    #region GetPreferencesSummaryAsync Tests

    [Fact]
    public async Task GetPreferencesSummaryAsync_ValidUser_ShouldReturnSummary()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = CreateSamplePreferences(userId);

        SetupPreferencesExists(preferences);

        // Act
        var result = await _service.GetPreferencesSummaryAsync(userId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.EmailEnabled.Should().BeTrue();
        result.Value.SmsEnabled.Should().BeTrue();
        result.Value.PushEnabled.Should().BeTrue();
        result.Value.Language.Should().Be("tr-TR");
    }

    [Fact]
    public async Task GetPreferencesSummaryAsync_PreferencesNotFound_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupPreferencesNotExists();

        // Act
        var result = await _service.GetPreferencesSummaryAsync(userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("PREFERENCES_NOT_FOUND");
    }

    #endregion

    #region Helper Methods

    private UserNotificationPreferences CreateSamplePreferences(Guid userId)
    {
        return new UserNotificationPreferences
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EmailEnabled = true,
            EmailOrderUpdates = true,
            EmailPromotions = true,
            EmailNewsletter = true,
            EmailSecurityAlerts = true,
            SmsEnabled = true,
            SmsOrderUpdates = true,
            SmsPromotions = false,
            SmsSecurityAlerts = true,
            PushEnabled = true,
            PushOrderUpdates = true,
            PushPromotions = true,
            PushMerchantUpdates = true,
            PushSecurityAlerts = true,
            QuietStartTime = new TimeSpan(22, 0, 0),
            QuietEndTime = new TimeSpan(8, 0, 0),
            RespectQuietHours = true,
            Language = "tr-TR",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private void SetupPreferencesExists(UserNotificationPreferences preferences)
    {
        var preferencesRepo = new Mock<IReadOnlyRepository<UserNotificationPreferences>>();
        preferencesRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<UserNotificationPreferences, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(preferences);
        _unitOfWorkMock.Setup(u => u.ReadRepository<UserNotificationPreferences>()).Returns(preferencesRepo.Object);
    }

    private void SetupPreferencesNotExists()
    {
        var preferencesRepo = new Mock<IReadOnlyRepository<UserNotificationPreferences>>();
        preferencesRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<UserNotificationPreferences, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserNotificationPreferences?)null);
        _unitOfWorkMock.Setup(u => u.ReadRepository<UserNotificationPreferences>()).Returns(preferencesRepo.Object);
    }

    private void SetupPreferencesNotExistsThenExists(UserNotificationPreferences preferences)
    {
        var preferencesRepo = new Mock<IReadOnlyRepository<UserNotificationPreferences>>();
        var callCount = 0;
        preferencesRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<UserNotificationPreferences, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1 ? null : preferences;
            });
        _unitOfWorkMock.Setup(u => u.ReadRepository<UserNotificationPreferences>()).Returns(preferencesRepo.Object);
    }

    private void SetupPreferencesRepository()
    {
        var genericRepo = new Mock<IGenericRepository<UserNotificationPreferences>>();
        genericRepo
            .Setup(r => r.AddAsync(It.IsAny<UserNotificationPreferences>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserNotificationPreferences entity, CancellationToken ct) => entity);
        _unitOfWorkMock.Setup(u => u.Repository<UserNotificationPreferences>()).Returns(genericRepo.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private void SetupPreferencesUpdateRepository()
    {
        var genericRepo = new Mock<IGenericRepository<UserNotificationPreferences>>();
        genericRepo.Setup(r => r.Update(It.IsAny<UserNotificationPreferences>()));
        _unitOfWorkMock.Setup(u => u.Repository<UserNotificationPreferences>()).Returns(genericRepo.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    #endregion
}

