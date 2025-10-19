using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.WorkingHours;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class WorkingHoursServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<WorkingHoursService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly WorkingHoursService _service;

    public WorkingHoursServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<WorkingHoursService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        
        _service = new WorkingHoursService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object);
    }

    [Fact]
    public async Task GetWorkingHoursByMerchantAsync_ValidMerchant_ShouldReturnWorkingHours()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var workingHours = new List<WorkingHours>
        {
            new() { Id = Guid.NewGuid(), MerchantId = merchantId, DayOfWeek = DayOfWeek.Monday, OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(18, 0, 0), IsClosed = false, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), MerchantId = merchantId, DayOfWeek = DayOfWeek.Tuesday, OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(18, 0, 0), IsClosed = false, CreatedAt = DateTime.UtcNow }
        };

        var readRepoMock = new Mock<IReadOnlyRepository<WorkingHours>>();
        readRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<WorkingHours, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<WorkingHours, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(workingHours);

        _unitOfWorkMock.Setup(u => u.ReadRepository<WorkingHours>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetWorkingHoursByMerchantAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task IsMerchantOpenAsync_DuringBusinessHours_ShouldReturnTrue()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var checkTime = new DateTime(2025, 10, 20, 10, 0, 0); // Monday 10:00

        var workingHours = new WorkingHours
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeSpan(9, 0, 0),
            CloseTime = new TimeSpan(18, 0, 0),
            IsClosed = false
        };

        var readRepoMock = new Mock<IReadOnlyRepository<WorkingHours>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<WorkingHours, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(workingHours);

        _unitOfWorkMock.Setup(u => u.ReadRepository<WorkingHours>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.IsMerchantOpenAsync(merchantId, checkTime);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task IsMerchantOpenAsync_OutsideBusinessHours_ShouldReturnFalse()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var checkTime = new DateTime(2025, 10, 20, 20, 0, 0); // Monday 20:00 (after closing)

        var workingHours = new WorkingHours
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeSpan(9, 0, 0),
            CloseTime = new TimeSpan(18, 0, 0),
            IsClosed = false
        };

        var readRepoMock = new Mock<IReadOnlyRepository<WorkingHours>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<WorkingHours, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(workingHours);

        _unitOfWorkMock.Setup(u => u.ReadRepository<WorkingHours>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.IsMerchantOpenAsync(merchantId, checkTime);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsMerchantOpenAsync_DayClosed_ShouldReturnFalse()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var checkTime = new DateTime(2025, 10, 19, 10, 0, 0); // Sunday

        var workingHours = new WorkingHours
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            DayOfWeek = DayOfWeek.Sunday,
            OpenTime = null,
            CloseTime = null,
            IsClosed = true
        };

        var readRepoMock = new Mock<IReadOnlyRepository<WorkingHours>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<WorkingHours, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(workingHours);

        _unitOfWorkMock.Setup(u => u.ReadRepository<WorkingHours>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.IsMerchantOpenAsync(merchantId, checkTime);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeFalse();
    }
}

