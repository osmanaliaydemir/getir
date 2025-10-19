using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.Services.DeliveryOptimization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

/// <summary>
/// Minimal tests - DeliveryCapacity is complex planning service
/// </summary>
public class DeliveryCapacityServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<DeliveryCapacityService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly DeliveryCapacityService _service;

    public DeliveryCapacityServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<DeliveryCapacityService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();

        _service = new DeliveryCapacityService(
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

