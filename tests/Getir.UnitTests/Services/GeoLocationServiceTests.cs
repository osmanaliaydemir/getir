using Getir.Application.Abstractions;
using Getir.Application.Services.GeoLocation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Getir.UnitTests.Services;

public class GeoLocationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<GeoLocationService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GeoLocationService _service;

    public GeoLocationServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<GeoLocationService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();

        _service = new GeoLocationService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object);
    }

    #region CalculateDistance Tests

    [Fact]
    public void CalculateDistance_SameLocation_ShouldReturnZero()
    {
        // Arrange
        double lat = 41.0082;
        double lon = 28.9784;

        // Act
        var distance = _service.CalculateDistance(lat, lon, lat, lon);

        // Assert
        distance.Should().Be(0);
    }

    [Fact]
    public void CalculateDistance_IstanbulToAnkara_ShouldReturnApproximateDistance()
    {
        // Arrange - Istanbul (Taksim) to Ankara (Kızılay)
        double istanbulLat = 41.0370;
        double istanbulLon = 28.9858;
        double ankaraLat = 39.9208;
        double ankaraLon = 32.8541;

        // Act
        var distance = _service.CalculateDistance(istanbulLat, istanbulLon, ankaraLat, ankaraLon);

        // Assert - Should be approximately 350-400 km
        distance.Should().BeGreaterThan(300);
        distance.Should().BeLessThan(500);
    }

    [Fact]
    public void CalculateDistance_ShortDistance_ShouldReturnAccurateResult()
    {
        // Arrange - Two points 1km apart (approximately)
        double lat1 = 41.0082;
        double lon1 = 28.9784;
        double lat2 = 41.0172;  // ~1km north
        double lon2 = 28.9784;

        // Act
        var distance = _service.CalculateDistance(lat1, lon1, lat2, lon2);

        // Assert - Should be approximately 1 km
        distance.Should().BeGreaterThan(0.5);
        distance.Should().BeLessThan(1.5);
    }

    #endregion
}

