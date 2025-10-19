using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.SpecialHolidays;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class SpecialHolidayServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<SpecialHolidayService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly SpecialHolidayService _service;

    public SpecialHolidayServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<SpecialHolidayService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        
        _service = new SpecialHolidayService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task GetAllSpecialHolidaysAsync_ShouldReturnHolidays()
    {
        // Arrange
        var holidays = new List<SpecialHoliday>
        {
            new() { Id = Guid.NewGuid(), MerchantId = Guid.NewGuid(), Title = "New Year", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1), IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        var readRepoMock = new Mock<IReadOnlyRepository<SpecialHoliday>>();
        readRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<SpecialHoliday, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<SpecialHoliday, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        _unitOfWorkMock.Setup(u => u.ReadRepository<SpecialHoliday>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetAllSpecialHolidaysAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSpecialHolidaysByMerchantAsync_ValidMerchant_ShouldReturnHolidays()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var holidays = new List<SpecialHoliday>
        {
            new() { Id = Guid.NewGuid(), MerchantId = merchantId, Title = "Holiday", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1), IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        var readRepoMock = new Mock<IReadOnlyRepository<SpecialHoliday>>();
        readRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<SpecialHoliday, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<SpecialHoliday, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(holidays);

        _unitOfWorkMock.Setup(u => u.ReadRepository<SpecialHoliday>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetSpecialHolidaysByMerchantAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateSpecialHolidayAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateSpecialHolidayRequest(
            merchantId,
            "New Year",
            "Celebration",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            true,
            null,
            null,
            false);

        var merchantRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        merchantRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Merchant { Id = merchantId, OwnerId = userId });

        var holidayRepoMock = new Mock<IGenericRepository<SpecialHoliday>>();
        holidayRepoMock.Setup(r => r.AddAsync(It.IsAny<SpecialHoliday>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecialHoliday());

        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(merchantRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<SpecialHoliday>()).Returns(holidayRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateSpecialHolidayAsync(request, userId);

        // Assert
        result.Success.Should().BeTrue();
    }
}

