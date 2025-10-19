using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.Services.DeliveryOptimization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

/// <summary>
/// Minimal tests - RouteOptimization is complex algorithm service
/// </summary>
public class RouteOptimizationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<RouteOptimizationService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly RouteOptimizationService _service;

    public RouteOptimizationServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<RouteOptimizationService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();

        _service = new RouteOptimizationService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public void ServiceConstruction_ShouldSucceed()
    {
        // Assert
        Assert.NotNull(_service);
    }
}

