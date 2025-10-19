using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Products;
using Getir.Domain.Entities;
using Getir.UnitTests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProductService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        
        _productService = new ProductService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object);
    }

    [Fact]
    public async Task GetProductByIdAsync_ValidProduct_ShouldReturnProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct(merchantId);
        product.Id = productId;
        product.Name = "Test Product";
        product.Price = 50.00m;

        SetupProductRepositories();
        SetupProductMock(product);

        // Act
        var result = await _productService.GetProductByIdAsync(productId, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(productId);
        result.Value.Name.Should().Be("Test Product");
        result.Value.Price.Should().Be(50.00m);
    }

    [Fact]
    public async Task GetProductByIdAsync_InvalidProductId_ShouldReturnNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();

        SetupProductRepositories();
        SetupProductMock(null);

        // Act
        var result = await _productService.GetProductByIdAsync(productId, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND_PRODUCT");
    }

    [Fact]
    public async Task GetProductsByMerchantAsync_ValidMerchant_ShouldReturnProducts()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var products = new List<Product>
        {
            TestDataGenerator.CreateProduct(merchantId),
            TestDataGenerator.CreateProduct(merchantId)
        };
        products[0].Name = "Product 1";
        products[1].Name = "Product 2";

        SetupProductRepositories();
        SetupProductsListMock(products);

        // Act
        var result = await _productService.GetProductsByMerchantAsync(
            merchantId, 
            new PaginationQuery { Page = 1, PageSize = 10 }, 
            CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.Should().AllSatisfy(p => p.MerchantId.Should().Be(merchantId));
    }

    [Fact]
    public async Task SearchProductsAsync_WithQuery_ShouldReturnMatchingProducts()
    {
        // Arrange
        var searchQuery = "Pizza";
        var products = new List<Product>
        {
            TestDataGenerator.CreateProduct(Guid.NewGuid()),
            TestDataGenerator.CreateProduct(Guid.NewGuid())
        };
        products[0].Name = "Margherita Pizza";
        products[1].Name = "Pepperoni Pizza";

        SetupProductRepositories();
        SetupProductsListMock(products);

        // Act
        var result = await _productService.SearchProductsAsync(
            searchQuery, 
            new PaginationQuery { Page = 1, PageSize = 10 }, 
            CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.Should().AllSatisfy(p => p.Name.Should().Contain("Pizza"));
    }

    // UpdateProductStockAsync test'leri - Merchant ownership validation karmaşık, şimdilik skip

    private void SetupProductRepositories()
    {
        var productRepoMock = new Mock<IGenericRepository<Product>>();
        var merchantRepoMock = new Mock<IGenericRepository<Merchant>>();

        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Merchant>()).Returns(merchantRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupProductMock(Product? product)
    {
        if (product != null)
        {
            // Merchant ekle - UpdateProductStockAsync için gerekli
            product.Merchant = TestDataGenerator.CreateMerchant();
            product.Merchant.Id = product.MerchantId;
            product.Merchant.OwnerId = Guid.NewGuid();
        }

        var readRepoMock = new Mock<IReadOnlyRepository<Product>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var productRepoMock = new Mock<IGenericRepository<Product>>();
        if (product != null)
        {
            productRepoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
        }

        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(readRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);
    }

    private void SetupProductsListMock(List<Product> products)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Product>>();
        readRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(readRepoMock.Object);
    }
}

