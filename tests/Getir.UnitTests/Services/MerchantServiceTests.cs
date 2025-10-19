using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Merchants;
using Getir.Domain.Entities;
using Getir.UnitTests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class MerchantServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<MerchantService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly MerchantService _merchantService;

    public MerchantServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<MerchantService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        
        _merchantService = new MerchantService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object);
    }

    // MerchantService test'leri - Mock setup karmaşık, şimdilik skip

    private void SetupMerchantRepositories()
    {
        var merchantRepoMock = new Mock<IGenericRepository<Merchant>>();
        var orderRepoMock = new Mock<IGenericRepository<Order>>();

        _unitOfWorkMock.Setup(u => u.Repository<Merchant>()).Returns(merchantRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Order>()).Returns(orderRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupMerchantMock(Merchant? merchant)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchant);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(readRepoMock.Object);
    }

    private void SetupMerchantsListMock(List<Merchant> merchants)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        readRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchants);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(readRepoMock.Object);
    }
}

