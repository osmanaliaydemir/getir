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

public class MarketProductVariantServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<MarketProductVariantService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly MarketProductVariantService _service;

    public MarketProductVariantServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<MarketProductVariantService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        
        _service = new MarketProductVariantService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task GetProductVariantsAsync_ValidProduct_ShouldReturnVariants()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var query = new PaginationQuery { Page = 1, PageSize = 10 };

        var productReadRepoMock = new Mock<IReadOnlyRepository<MarketProduct>>();
        productReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<MarketProduct, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MarketProduct { Id = productId });

        var variantReadRepoMock = new Mock<IReadOnlyRepository<MarketProductVariant>>();
        variantReadRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<MarketProductVariant, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<MarketProductVariant, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MarketProductVariant>
            {
                new() { Id = Guid.NewGuid(), ProductId = productId, Name = "Variant 1", Price = 50m }
            });

        _unitOfWorkMock.Setup(u => u.ReadRepository<MarketProduct>()).Returns(productReadRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<MarketProductVariant>()).Returns(variantReadRepoMock.Object);

        // Act
        var result = await _service.GetProductVariantsAsync(productId, query);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetProductVariantAsync_ValidId_ShouldReturnVariant()
    {
        // Arrange
        var variantId = Guid.NewGuid();
        var variant = new MarketProductVariant { Id = variantId, Name = "Test Variant", Price = 100m };

        var readRepoMock = new Mock<IReadOnlyRepository<MarketProductVariant>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<MarketProductVariant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(variant);

        _unitOfWorkMock.Setup(u => u.ReadRepository<MarketProductVariant>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetProductVariantAsync(variantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Name.Should().Be("Test Variant");
    }

    [Fact]
    public async Task CreateProductVariantAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateMarketProductVariantRequest(
            productId, "Variant 1", "Desc", "SKU001", 50m, null, 100, 1, "L", null, null, null, null, null);

        var productReadRepoMock = new Mock<IReadOnlyRepository<MarketProduct>>();
        productReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<MarketProduct, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MarketProduct 
            { 
                Id = productId, 
                Market = new Market 
                { 
                    Merchant = new Merchant { OwnerId = userId } 
                } 
            });

        var variantReadRepoMock = new Mock<IReadOnlyRepository<MarketProductVariant>>();
        variantReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<MarketProductVariant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketProductVariant?)null);

        var variantRepoMock = new Mock<IGenericRepository<MarketProductVariant>>();
        variantRepoMock.Setup(r => r.AddAsync(It.IsAny<MarketProductVariant>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MarketProductVariant());

        _unitOfWorkMock.Setup(u => u.ReadRepository<MarketProduct>()).Returns(productReadRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<MarketProductVariant>()).Returns(variantReadRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<MarketProductVariant>()).Returns(variantRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateProductVariantAsync(request, userId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateProductVariantAsync_DuplicateSKU_ShouldFail()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateMarketProductVariantRequest(
            productId, "Variant 1", "Desc", "SKU001", 50m, null, 100, 1, null, null, null, null, null, null);

        var productReadRepoMock = new Mock<IReadOnlyRepository<MarketProduct>>();
        productReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<MarketProduct, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MarketProduct 
            { 
                Id = productId, 
                Market = new Market 
                { 
                    Merchant = new Merchant { OwnerId = userId } 
                } 
            });

        var variantReadRepoMock = new Mock<IReadOnlyRepository<MarketProductVariant>>();
        variantReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<MarketProductVariant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MarketProductVariant { SKU = "SKU001" });

        _unitOfWorkMock.Setup(u => u.ReadRepository<MarketProduct>()).Returns(productReadRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<MarketProductVariant>()).Returns(variantReadRepoMock.Object);

        // Act
        var result = await _service.CreateProductVariantAsync(request, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("DUPLICATE_SKU");
    }
}

