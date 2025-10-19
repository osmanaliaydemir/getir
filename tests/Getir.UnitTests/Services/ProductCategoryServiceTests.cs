using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.ProductCategories;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class ProductCategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ProductCategoryService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly ProductCategoryService _service;

    public ProductCategoryServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProductCategoryService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        
        _service = new ProductCategoryService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object);
    }

    [Fact]
    public async Task GetMerchantCategoriesAsync_ValidMerchant_ShouldReturnCategories()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var categories = new List<ProductCategory>
        {
            new() { Id = Guid.NewGuid(), MerchantId = merchantId, Name = "Cat 1", IsActive = true, Merchant = new Merchant { Name = "Test" } },
            new() { Id = Guid.NewGuid(), MerchantId = merchantId, Name = "Cat 2", IsActive = true, Merchant = new Merchant { Name = "Test" } }
        };

        var readRepoMock = new Mock<IReadOnlyRepository<ProductCategory>>();
        readRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductCategory, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductCategory, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        _unitOfWorkMock.Setup(u => u.ReadRepository<ProductCategory>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetMerchantCategoriesAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProductCategoryByIdAsync_ValidId_ShouldReturnCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new ProductCategory
        {
            Id = categoryId,
            MerchantId = Guid.NewGuid(),
            Name = "Test Category",
            IsActive = true,
            Merchant = new Merchant { Name = "Test" }
        };

        var repoMock = new Mock<IGenericRepository<ProductCategory>>();
        repoMock.Setup(r => r.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductCategory, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _unitOfWorkMock.Setup(u => u.Repository<ProductCategory>()).Returns(repoMock.Object);

        // Act
        var result = await _service.GetProductCategoryByIdAsync(categoryId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductCategoryByIdAsync_NotFound_ShouldFail()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        var repoMock = new Mock<IGenericRepository<ProductCategory>>();
        repoMock.Setup(r => r.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductCategory, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategory?)null);

        _unitOfWorkMock.Setup(u => u.Repository<ProductCategory>()).Returns(repoMock.Object);

        // Act
        var result = await _service.GetProductCategoryByIdAsync(categoryId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND_PRODUCT_CATEGORY");
    }

    [Fact]
    public async Task CreateProductCategoryAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var request = new CreateProductCategoryRequest("New Category", "Description", null, null, 1);

        var merchantRepoMock = new Mock<IReadOnlyRepository<Merchant>>();
        merchantRepoMock.Setup(r => r.AnyAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var categoryRepoMock = new Mock<IGenericRepository<ProductCategory>>();
        categoryRepoMock.Setup(r => r.AddAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductCategory());
        
        categoryRepoMock.Setup(r => r.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductCategory, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductCategory
            {
                Id = Guid.NewGuid(),
                MerchantId = merchantId,
                Name = "New Category",
                IsActive = true,
                Merchant = new Merchant { Id = merchantId, Name = "Test" }
            });

        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(merchantRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<ProductCategory>()).Returns(categoryRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateProductCategoryAsync(request, merchantId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProductCategoryAsync_HasSubCategories_ShouldFail()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var category = new ProductCategory
        {
            Id = categoryId,
            Name = "Category",
            Merchant = new Merchant { OwnerId = userId, Name = "Test" },
            SubCategories = new List<ProductCategory>
            {
                new() { Id = Guid.NewGuid(), Name = "Sub" }
            },
            Products = new List<Product>()
        };

        var repoMock = new Mock<IGenericRepository<ProductCategory>>();
        repoMock.Setup(r => r.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductCategory, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _unitOfWorkMock.Setup(u => u.Repository<ProductCategory>()).Returns(repoMock.Object);

        // Act
        var result = await _service.DeleteProductCategoryAsync(categoryId, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("HAS_SUB_CATEGORIES");
    }
}