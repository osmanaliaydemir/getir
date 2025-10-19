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

    #region GetMerchantByIdAsync Tests

    [Fact]
    public async Task GetMerchantByIdAsync_ValidId_ShouldReturnMerchant()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var owner = TestDataGenerator.CreateUser();
        var category = new ServiceCategory { Id = Guid.NewGuid(), Name = "Restaurant" };
        
        var merchant = TestDataGenerator.CreateMerchant();
        merchant.Id = merchantId;
        merchant.Name = "Test Merchant";
        merchant.IsActive = true;
        merchant.OwnerId = owner.Id;
        merchant.Owner = owner; // Navigation property
        merchant.ServiceCategoryId = category.Id;
        merchant.ServiceCategory = category; // Navigation property

        SetupMerchantWithNavigationMock(merchant);

        // Act
        var result = await _merchantService.GetMerchantByIdAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(merchantId);
        result.Value.Name.Should().Be("Test Merchant");
    }

    [Fact]
    public async Task GetMerchantByIdAsync_InvalidId_ShouldFail()
    {
        // Arrange
        SetupMerchantMock(null);

        // Act
        var result = await _merchantService.GetMerchantByIdAsync(Guid.NewGuid());

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().BeOneOf("MERCHANT_NOT_FOUND", "NOT_FOUND_MERCHANT", "INTERNAL_ERROR");
    }

    #endregion

    #region GetMerchantByOwnerIdAsync Tests

    [Fact]
    public async Task GetMerchantByOwnerIdAsync_ValidOwnerId_ShouldReturnMerchant()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var owner = TestDataGenerator.CreateUser();
        owner.Id = ownerId;
        var category = new ServiceCategory { Id = Guid.NewGuid(), Name = "Restaurant" };
        
        var merchant = TestDataGenerator.CreateMerchant();
        merchant.OwnerId = ownerId;
        merchant.Owner = owner; // Navigation property
        merchant.ServiceCategoryId = category.Id;
        merchant.ServiceCategory = category; // Navigation property
        merchant.IsActive = true;

        SetupMerchantWithNavigationMock(merchant);

        // Act
        var result = await _merchantService.GetMerchantByOwnerIdAsync(ownerId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.OwnerId.Should().Be(ownerId);
    }

    [Fact]
    public async Task GetMerchantByOwnerIdAsync_NoMerchant_ShouldFail()
    {
        // Arrange
        SetupMerchantMock(null);

        // Act
        var result = await _merchantService.GetMerchantByOwnerIdAsync(Guid.NewGuid());

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().BeOneOf("MERCHANT_NOT_FOUND", "NOT_FOUND_MERCHANT", "INTERNAL_ERROR");
    }

    #endregion

    #region Helper Methods

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

    private void SetupMerchantWithNavigationMock(Merchant merchant)
    {
        // GetMerchantByIdAsync and similar methods use GetAsync with include
        var repoMock = new Mock<IGenericRepository<Merchant>>();
        repoMock.Setup(r => r.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchant);

        _unitOfWorkMock.Setup(u => u.Repository<Merchant>()).Returns(repoMock.Object);
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

    private void SetupMerchantByIdMock(Merchant? merchant)
    {
        var repoMock = new Mock<IGenericRepository<Merchant>>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchant);

        _unitOfWorkMock.Setup(u => u.Repository<Merchant>()).Returns(repoMock.Object);
    }

    private void SetupPagedMerchants(List<Merchant> merchants, int totalCount)
    {
        var repoMock = new Mock<IGenericRepository<Merchant>>();
        var readRepoMock = new Mock<IReadOnlyRepository<Merchant>>();

        repoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchants);

        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        _unitOfWorkMock.Setup(u => u.Repository<Merchant>()).Returns(repoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(readRepoMock.Object);
    }

    private void SetupSearchMerchants(List<Merchant> merchants, int totalCount)
    {
        var repoMock = new Mock<IGenericRepository<Merchant>>();
        var readRepoMock = new Mock<IReadOnlyRepository<Merchant>>();

        repoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchants);

        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        _unitOfWorkMock.Setup(u => u.Repository<Merchant>()).Returns(repoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(readRepoMock.Object);
    }

    #endregion
}

