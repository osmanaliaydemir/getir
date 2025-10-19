using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.ProductOptions;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class ProductOptionGroupServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ProductOptionGroupService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly ProductOptionGroupService _service;

    public ProductOptionGroupServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProductOptionGroupService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        
        _service = new ProductOptionGroupService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task GetProductOptionGroupsAsync_ValidProduct_ShouldReturnGroups()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var query = new PaginationQuery { Page = 1, PageSize = 10 };

        var productReadRepoMock = new Mock<IReadOnlyRepository<Product>>();
        productReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = productId });

        var groupReadRepoMock = new Mock<IReadOnlyRepository<ProductOptionGroup>>();
        groupReadRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductOptionGroup, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductOptionGroup, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductOptionGroup>
            {
                new() { Id = Guid.NewGuid(), ProductId = productId, Name = "Size", IsActive = true }
            });

        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(productReadRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<ProductOptionGroup>()).Returns(groupReadRepoMock.Object);

        // Act
        var result = await _service.GetProductOptionGroupsAsync(productId, query);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateProductOptionGroupAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateProductOptionGroupRequest(productId, "Size", "Choose size", true, 1, 1, 1);

        var productReadRepoMock = new Mock<IReadOnlyRepository<Product>>();
        productReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = productId, Merchant = new Merchant { OwnerId = userId } });

        var groupRepoMock = new Mock<IGenericRepository<ProductOptionGroup>>();
        groupRepoMock.Setup(r => r.AddAsync(It.IsAny<ProductOptionGroup>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductOptionGroup());

        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(productReadRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<ProductOptionGroup>()).Returns(groupRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateProductOptionGroupAsync(request, userId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateProductOptionGroupAsync_ProductNotFound_ShouldFail()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateProductOptionGroupRequest(productId, "Size", "Choose size", true, 1, 1, 1);

        var productReadRepoMock = new Mock<IReadOnlyRepository<Product>>();
        productReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(productReadRepoMock.Object);

        // Act
        var result = await _service.CreateProductOptionGroupAsync(request, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("PRODUCT_NOT_FOUND");
    }
}

