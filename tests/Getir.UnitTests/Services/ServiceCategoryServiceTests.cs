using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.ServiceCategories;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class ServiceCategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ServiceCategoryService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly ServiceCategoryService _service;

    public ServiceCategoryServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ServiceCategoryService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        
        _service = new ServiceCategoryService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object);
    }

    [Fact]
    public async Task GetServiceCategoriesAsync_ShouldReturnCategories()
    {
        // Arrange
        var query = new PaginationQuery { Page = 1, PageSize = 10 };
        var categories = new List<ServiceCategory>
        {
            new() { Id = Guid.NewGuid(), Name = "Restaurant", IsActive = true, Type = ServiceCategoryType.Restaurant }
        };

        var repoMock = new Mock<IGenericRepository<ServiceCategory>>();
        repoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ServiceCategory, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<ServiceCategory, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var readRepoMock = new Mock<IReadOnlyRepository<ServiceCategory>>();
        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ServiceCategory, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _unitOfWorkMock.Setup(u => u.Repository<ServiceCategory>()).Returns(repoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<ServiceCategory>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetServiceCategoriesAsync(query);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateServiceCategoryAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var request = new CreateServiceCategoryRequest("Restaurant", "Restaurant services", ServiceCategoryType.Restaurant, null, null, 1);

        var repoMock = new Mock<IGenericRepository<ServiceCategory>>();
        repoMock.Setup(r => r.AddAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServiceCategory());

        _unitOfWorkMock.Setup(u => u.Repository<ServiceCategory>()).Returns(repoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateServiceCategoryAsync(request);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateServiceCategoryAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new UpdateServiceCategoryRequest("Updated", "Updated desc", ServiceCategoryType.Restaurant, null, null, 1, true);

        var category = new ServiceCategory { Id = categoryId, Name = "Old", IsActive = true };

        var repoMock = new Mock<IGenericRepository<ServiceCategory>>();
        repoMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        repoMock.Setup(r => r.Update(It.IsAny<ServiceCategory>()));

        var readRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _unitOfWorkMock.Setup(u => u.Repository<ServiceCategory>()).Returns(repoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(readRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.UpdateServiceCategoryAsync(categoryId, request);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteServiceCategoryAsync_ValidCategory_ShouldSoftDelete()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new ServiceCategory { Id = categoryId, Name = "Category", IsActive = true };

        var repoMock = new Mock<IGenericRepository<ServiceCategory>>();
        repoMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        repoMock.Setup(r => r.Update(It.IsAny<ServiceCategory>()));

        _unitOfWorkMock.Setup(u => u.Repository<ServiceCategory>()).Returns(repoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.DeleteServiceCategoryAsync(categoryId);

        // Assert
        result.Success.Should().BeTrue();
    }
}

