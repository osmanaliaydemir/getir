using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Stock;
using Getir.Domain.Entities;
using Getir.UnitTests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DomainEnums = Getir.Domain.Enums;

namespace Getir.UnitTests.Services;

/// <summary>
/// Comprehensive unit tests for StockManagementService
/// Tests all public methods with happy paths, error cases, and edge cases
/// </summary>
public class StockManagementServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<StockManagementService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ISignalRService> _signalRServiceMock;
    private readonly StockManagementService _stockManagementService;

    public StockManagementServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<StockManagementService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _signalRServiceMock = new Mock<ISignalRService>();

        _stockManagementService = new StockManagementService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _signalRServiceMock.Object);
    }

    #region ReduceStockForOrderAsync Tests

    [Fact]
    public async Task ReduceStockForOrderAsync_ValidOrder_ShouldReduceStockSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct(merchantId, stockQuantity: 100);
        
        var order = new Order
        {
            Id = orderId,
            MerchantId = merchantId,
            OrderNumber = "ORD-001",
            OrderLines = new List<OrderLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = product.Id,
                    Product = product,
                    Quantity = 10,
                    UnitPrice = 50
                }
            }
        };

        SetupOrderMock(order);
        SetupProductRepositories();
        SetupStockHistoryRepository();
        SetupTransactionMocks();

        var productRepoMock = new Mock<IGenericRepository<Product>>();
        productRepoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);

        // Act
        var result = await _stockManagementService.ReduceStockForOrderAsync(orderId);

        // Assert
        result.Success.Should().BeTrue();
        product.StockQuantity.Should().Be(90); // 100 - 10
        product.IsAvailable.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReduceStockForOrderAsync_StockBecomesZero_ShouldMarkAsUnavailable()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct(merchantId, stockQuantity: 10);
        
        var order = new Order
        {
            Id = orderId,
            MerchantId = merchantId,
            OrderNumber = "ORD-002",
            OrderLines = new List<OrderLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = product.Id,
                    Product = product,
                    Quantity = 10 // Exactly depletes stock
                }
            }
        };

        SetupOrderMock(order);
        SetupProductRepositories();
        SetupStockHistoryRepository();
        SetupTransactionMocks();

        var productRepoMock = new Mock<IGenericRepository<Product>>();
        productRepoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);

        // Act
        var result = await _stockManagementService.ReduceStockForOrderAsync(orderId);

        // Assert
        result.Success.Should().BeTrue();
        product.StockQuantity.Should().Be(0);
        product.IsAvailable.Should().BeFalse(); // Should be marked unavailable
    }

    [Fact]
    public async Task ReduceStockForOrderAsync_OrderNotFound_ShouldReturnError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        SetupOrderMock(null);

        // Act
        var result = await _stockManagementService.ReduceStockForOrderAsync(orderId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ORDER_NOT_FOUND");
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReduceStockForOrderAsync_WithProductVariant_ShouldReduceVariantStock()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct(merchantId);
        var variant = new MarketProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Large Size",
            StockQuantity = 50,
            IsAvailable = true
        };

        var order = new Order
        {
            Id = orderId,
            MerchantId = merchantId,
            OrderNumber = "ORD-003",
            OrderLines = new List<OrderLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = product.Id,
                    ProductVariantId = variant.Id,
                    Product = product,
                    Quantity = 5,
                    ProductName = product.Name,
                    VariantName = variant.Name,
                    UnitPrice = 50,
                    TotalPrice = 250
                }
            }
        };

        SetupOrderMock(order);
        SetupProductRepositories();
        SetupStockHistoryRepository();
        SetupTransactionMocks();

        var variantRepoMock = new Mock<IGenericRepository<MarketProductVariant>>();
        variantRepoMock.Setup(r => r.GetByIdAsync(variant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(variant);
        _unitOfWorkMock.Setup(u => u.Repository<MarketProductVariant>()).Returns(variantRepoMock.Object);

        // Act
        var result = await _stockManagementService.ReduceStockForOrderAsync(orderId);

        // Assert
        result.Success.Should().BeTrue();
        variant.StockQuantity.Should().Be(45); // 50 - 5
        variant.IsAvailable.Should().BeTrue();
    }

    #endregion

    #region RestoreStockForOrderAsync Tests

    [Fact]
    public async Task RestoreStockForOrderAsync_ValidOrder_ShouldRestoreStockSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct(merchantId, stockQuantity: 90);
        
        var order = new Order
        {
            Id = orderId,
            MerchantId = merchantId,
            OrderNumber = "ORD-004",
            OrderLines = new List<OrderLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = product.Id,
                    Product = product,
                    Quantity = 10
                }
            }
        };

        SetupOrderMock(order);
        SetupProductRepositories();
        SetupStockHistoryRepository();
        SetupTransactionMocks();

        var productRepoMock = new Mock<IGenericRepository<Product>>();
        productRepoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);

        // Act
        var result = await _stockManagementService.RestoreStockForOrderAsync(orderId);

        // Assert
        result.Success.Should().BeTrue();
        product.StockQuantity.Should().Be(100); // 90 + 10
        product.IsAvailable.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreStockForOrderAsync_StockBecomesAvailable_ShouldMarkAsAvailable()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct(merchantId, stockQuantity: 0);
        product.IsAvailable = false; // Currently unavailable
        
        var order = new Order
        {
            Id = orderId,
            MerchantId = merchantId,
            OrderNumber = "ORD-005",
            OrderLines = new List<OrderLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = product.Id,
                    Product = product,
                    Quantity = 10
                }
            }
        };

        SetupOrderMock(order);
        SetupProductRepositories();
        SetupStockHistoryRepository();
        SetupTransactionMocks();

        var productRepoMock = new Mock<IGenericRepository<Product>>();
        productRepoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);

        // Act
        var result = await _stockManagementService.RestoreStockForOrderAsync(orderId);

        // Assert
        result.Success.Should().BeTrue();
        product.StockQuantity.Should().Be(10);
        product.IsAvailable.Should().BeTrue(); // Should be available again
    }

    [Fact]
    public async Task RestoreStockForOrderAsync_OrderNotFound_ShouldReturnError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        SetupOrderMock(null);

        // Act
        var result = await _stockManagementService.RestoreStockForOrderAsync(orderId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ORDER_NOT_FOUND");
    }

    #endregion

    #region CheckStockLevelsAndAlertAsync Tests

    [Fact]
    public async Task CheckStockLevelsAndAlertAsync_LowStock_ShouldCreateAlert()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var settings = new StockSettings
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            IsActive = true,
            LowStockAlerts = true,
            DefaultMinimumStock = 10
        };

        var product = TestDataGenerator.CreateProduct(merchantId, stockQuantity: 5); // Low stock

        SetupStockSettingsMock(settings);
        SetupProductsListMock(new List<Product> { product });
        SetupStockAlertRepository();

        // Act
        var result = await _stockManagementService.CheckStockLevelsAndAlertAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.Repository<StockAlert>()
            .AddAsync(It.Is<StockAlert>(a => a.AlertType == DomainEnums.StockAlertType.LowStock), 
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckStockLevelsAndAlertAsync_OutOfStock_ShouldCreateOutOfStockAlert()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var settings = new StockSettings
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            IsActive = true,
            LowStockAlerts = true,
            DefaultMinimumStock = 10
        };

        var product = TestDataGenerator.CreateProduct(merchantId, stockQuantity: 0); // Out of stock

        SetupStockSettingsMock(settings);
        SetupProductsListMock(new List<Product> { product });
        SetupStockAlertRepository();

        // Act
        var result = await _stockManagementService.CheckStockLevelsAndAlertAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.Repository<StockAlert>()
            .AddAsync(It.Is<StockAlert>(a => a.AlertType == DomainEnums.StockAlertType.OutOfStock), 
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckStockLevelsAndAlertAsync_NoSettings_ShouldSkipAlerts()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        SetupStockSettingsMock(null);

        // Act
        var result = await _stockManagementService.CheckStockLevelsAndAlertAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.Repository<StockAlert>()
            .AddAsync(It.IsAny<StockAlert>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckStockLevelsAndAlertAsync_AlertsDisabled_ShouldSkipAlerts()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var settings = new StockSettings
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            IsActive = true,
            LowStockAlerts = false // Alerts disabled
        };

        SetupStockSettingsMock(settings);

        // Act
        var result = await _stockManagementService.CheckStockLevelsAndAlertAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.Repository<StockAlert>()
            .AddAsync(It.IsAny<StockAlert>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckStockLevelsAndAlertAsync_ExistingAlertNotResolved_ShouldNotDuplicateAlert()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct(merchantId, stockQuantity: 5);
        
        var settings = new StockSettings
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            IsActive = true,
            LowStockAlerts = true,
            DefaultMinimumStock = 10
        };

        var existingAlert = new StockAlert
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            MerchantId = merchantId,
            AlertType = DomainEnums.StockAlertType.LowStock,
            IsResolved = false // Not yet resolved
        };

        SetupStockSettingsMock(settings);
        SetupProductsListMock(new List<Product> { product });
        SetupExistingStockAlert(existingAlert);

        // Act
        var result = await _stockManagementService.CheckStockLevelsAndAlertAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.Repository<StockAlert>()
            .AddAsync(It.IsAny<StockAlert>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetStockHistoryAsync Tests

    [Fact]
    public async Task GetStockHistoryAsync_ValidProduct_ShouldReturnHistory()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var user = TestDataGenerator.CreateUser();
        var product = TestDataGenerator.CreateProduct(Guid.NewGuid());
        product.Id = productId;

        var histories = new List<StockHistory>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Product = product,
                PreviousQuantity = 100,
                NewQuantity = 90,
                ChangeAmount = -10,
                ChangeType = DomainEnums.StockChangeType.Sale,
                Reason = "Order confirmed",
                ChangedAt = DateTime.UtcNow.AddHours(-2),
                ChangedByUser = user
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Product = product,
                PreviousQuantity = 90,
                NewQuantity = 100,
                ChangeAmount = 10,
                ChangeType = DomainEnums.StockChangeType.Return,
                Reason = "Order cancelled",
                ChangedAt = DateTime.UtcNow.AddHours(-1),
                ChangedByUser = user
            }
        };

        SetupStockHistoryListMock(histories);

        // Act
        var result = await _stockManagementService.GetStockHistoryAsync(productId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().Contain(h => h.ChangeType == DomainEnums.StockChangeType.Sale);
        result.Value.Should().Contain(h => h.ChangeType == DomainEnums.StockChangeType.Return);
    }

    [Fact]
    public async Task GetStockHistoryAsync_WithDateRange_ShouldFilterByDate()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var fromDate = DateTime.UtcNow.AddDays(-7);
        var toDate = DateTime.UtcNow;

        var product = TestDataGenerator.CreateProduct(Guid.NewGuid());
        product.Id = productId;

        var histories = new List<StockHistory>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Product = product,
                PreviousQuantity = 100,
                NewQuantity = 90,
                ChangeAmount = -10,
                ChangeType = DomainEnums.StockChangeType.Sale,
                ChangedAt = DateTime.UtcNow.AddDays(-2) // Within range
            }
        };

        SetupStockHistoryListMock(histories);

        // Act
        var result = await _stockManagementService.GetStockHistoryAsync(productId, fromDate, toDate);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    #endregion

    #region GetStockAlertsAsync Tests

    [Fact]
    public async Task GetStockAlertsAsync_ValidMerchant_ShouldReturnAlerts()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct(merchantId);

        var alerts = new List<StockAlert>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Product = product,
                MerchantId = merchantId,
                CurrentStock = 5,
                MinimumStock = 10,
                AlertType = DomainEnums.StockAlertType.LowStock,
                Message = "Low stock alert",
                IsActive = true,
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        SetupStockAlertListMock(alerts);

        // Act
        var result = await _stockManagementService.GetStockAlertsAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(1);
        result.Value[0].AlertType.Should().Be(DomainEnums.StockAlertType.LowStock);
    }

    [Fact]
    public async Task GetStockAlertsAsync_NoAlerts_ShouldReturnEmptyList()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        SetupStockAlertListMock(new List<StockAlert>());

        // Act
        var result = await _stockManagementService.GetStockAlertsAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
    }

    #endregion

    #region UpdateStockLevelAsync Tests

    [Fact]
    public async Task UpdateStockLevelAsync_ValidProduct_ShouldUpdateSuccessfully()
    {
        // Arrange
        var merchantOwnerId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct(Guid.NewGuid(), stockQuantity: 50);
        product.Merchant = TestDataGenerator.CreateMerchant();
        product.Merchant.OwnerId = merchantOwnerId;

        var request = new UpdateStockRequest(
            product.Id,
            null,
            100,
            "Manual stock adjustment");

        SetupProductWithMerchantMock(product);
        SetupStockHistoryRepository();

        // Act
        var result = await _stockManagementService.UpdateStockLevelAsync(request, merchantOwnerId);

        // Assert
        result.Success.Should().BeTrue();
        product.StockQuantity.Should().Be(100);
        product.IsAvailable.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStockLevelAsync_ProductNotFound_ShouldReturnError()
    {
        // Arrange
        var request = new UpdateStockRequest(
            Guid.NewGuid(),
            null,
            100,
            "Manual adjustment");

        SetupProductWithMerchantMock(null);

        // Act
        var result = await _stockManagementService.UpdateStockLevelAsync(request, Guid.NewGuid());

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("PRODUCT_NOT_FOUND");
    }

    [Fact]
    public async Task UpdateStockLevelAsync_WrongOwner_ShouldReturnAccessDenied()
    {
        // Arrange
        var merchantOwnerId = Guid.NewGuid();
        var wrongOwnerId = Guid.NewGuid();
        
        var product = TestDataGenerator.CreateProduct(Guid.NewGuid());
        product.Merchant = TestDataGenerator.CreateMerchant();
        product.Merchant.OwnerId = merchantOwnerId; // Different owner

        var request = new UpdateStockRequest(
            product.Id,
            null,
            100,
            "Manual adjustment");

        SetupProductWithMerchantMock(product);

        // Act
        var result = await _stockManagementService.UpdateStockLevelAsync(request, wrongOwnerId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ACCESS_DENIED");
    }

    [Fact]
    public async Task UpdateStockLevelAsync_WithVariant_ShouldUpdateVariantStock()
    {
        // Arrange
        var merchantOwnerId = Guid.NewGuid();
        var merchant = TestDataGenerator.CreateMerchant();
        merchant.OwnerId = merchantOwnerId;
        
        var market = new Market
        {
            Id = Guid.NewGuid(),
            MerchantId = merchant.Id,
            Merchant = merchant
        };

        var marketProduct = new MarketProduct
        {
            Id = Guid.NewGuid(),
            MarketId = market.Id,
            Market = market,
            StockQuantity = 30
        };

        var variant = new MarketProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = marketProduct.Id,
            Product = marketProduct,
            StockQuantity = 30,
            IsAvailable = true
        };

        var request = new UpdateStockRequest(
            marketProduct.Id,
            variant.Id,
            100,
            "Variant stock adjustment");

        SetupVariantWithMerchantMock(variant);
        SetupStockHistoryRepository();

        // Act
        var result = await _stockManagementService.UpdateStockLevelAsync(request, merchantOwnerId);

        // Assert
        result.Success.Should().BeTrue();
        variant.StockQuantity.Should().Be(100);
        variant.IsAvailable.Should().BeTrue();
    }

    #endregion

    #region BulkUpdateStockLevelsAsync Tests

    [Fact]
    public async Task BulkUpdateStockLevelsAsync_ValidRequests_ShouldUpdateAll()
    {
        // Arrange
        var merchantOwnerId = Guid.NewGuid();
        var product1 = TestDataGenerator.CreateProduct(Guid.NewGuid());
        product1.Merchant = TestDataGenerator.CreateMerchant();
        product1.Merchant.OwnerId = merchantOwnerId;

        var product2 = TestDataGenerator.CreateProduct(Guid.NewGuid());
        product2.Merchant = product1.Merchant;

        var requests = new List<UpdateStockRequest>
        {
            new(product1.Id, null, 100, "Bulk update 1"),
            new(product2.Id, null, 200, "Bulk update 2")
        };

        SetupTransactionMocks();
        SetupStockHistoryRepository();

        // Setup individual product mocks
        var productReadRepoMock = new Mock<IReadOnlyRepository<Product>>();
        productReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.Is<System.Linq.Expressions.Expression<Func<Product, bool>>>(expr => expr.Compile()(product1)),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(product1);
        
        productReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.Is<System.Linq.Expressions.Expression<Func<Product, bool>>>(expr => expr.Compile()(product2)),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(product2);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(productReadRepoMock.Object);

        var productRepoMock = new Mock<IGenericRepository<Product>>();
        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);

        // Act
        var result = await _stockManagementService.BulkUpdateStockLevelsAsync(requests, merchantOwnerId);

        // Assert
        result.Success.Should().BeTrue();
        product1.StockQuantity.Should().Be(100);
        product2.StockQuantity.Should().Be(200);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BulkUpdateStockLevelsAsync_OneFailure_ShouldRollbackAll()
    {
        // Arrange
        var merchantOwnerId = Guid.NewGuid();
        var requests = new List<UpdateStockRequest>
        {
            new(Guid.NewGuid(), null, 100, "Update 1"),
            new(Guid.NewGuid(), null, 200, "Update 2") // This will fail (not found)
        };

        SetupTransactionMocks();
        SetupProductWithMerchantMock(null); // Product not found

        // Act
        var result = await _stockManagementService.BulkUpdateStockLevelsAsync(requests, merchantOwnerId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("PRODUCT_NOT_FOUND");
        _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetStockReportAsync Tests

    [Fact]
    public async Task GetStockReportAsync_ValidMerchant_ShouldReturnReport()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var merchantOwnerId = Guid.NewGuid();
        
        var products = new List<Product>
        {
            TestDataGenerator.CreateProduct(merchantId, stockQuantity: 5), // Low stock (<=10)
            TestDataGenerator.CreateProduct(merchantId, stockQuantity: 0), // Out of stock (=0)
            TestDataGenerator.CreateProduct(merchantId, stockQuantity: 150) // Overstock (>100)
        };

        products[0].Price = 50;
        products[1].Price = 30;
        products[2].Price = 100;

        var request = new StockReportRequest(merchantId, null, null, StockReportType.CurrentStock);

        SetupProductsListMock(products);

        // Act
        var result = await _stockManagementService.GetStockReportAsync(request, merchantOwnerId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Summary.TotalProducts.Should().Be(3);
        // LowStockItems includes both stock <= 10 (includes 0 and 5) = 2 items
        result.Value.Summary.LowStockItems.Should().Be(2); // Fixed: 0 and 5 are both <= 10
        result.Value.Summary.OutOfStockItems.Should().Be(1); // stock == 0
        result.Value.Summary.OverstockItems.Should().Be(1); // stock > 100
        result.Value.Items.Should().HaveCount(3);
    }

    #endregion

    #region SynchronizeStockAsync Tests

    [Fact]
    public async Task SynchronizeStockAsync_SyncEnabled_ShouldUpdateSyncTime()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var settings = new StockSettings
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            IsActive = true,
            EnableStockSync = true
        };

        SetupStockSettingsMock(settings);

        var settingsRepoMock = new Mock<IGenericRepository<StockSettings>>();
        _unitOfWorkMock.Setup(u => u.Repository<StockSettings>()).Returns(settingsRepoMock.Object);

        // Act
        var result = await _stockManagementService.SynchronizeStockAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        settings.LastSyncAt.Should().NotBeNull();
        settingsRepoMock.Verify(r => r.Update(settings), Times.Once);
    }

    [Fact]
    public async Task SynchronizeStockAsync_SyncDisabled_ShouldReturnError()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var settings = new StockSettings
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            IsActive = true,
            EnableStockSync = false // Sync disabled
        };

        SetupStockSettingsMock(settings);

        // Act
        var result = await _stockManagementService.SynchronizeStockAsync(merchantId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("SYNC_NOT_ENABLED");
    }

    [Fact]
    public async Task SynchronizeStockAsync_NoSettings_ShouldReturnError()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        SetupStockSettingsMock(null);

        // Act
        var result = await _stockManagementService.SynchronizeStockAsync(merchantId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("SYNC_NOT_ENABLED");
    }

    #endregion

    #region Helper Methods

    private void SetupOrderMock(Order? order)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Order>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Order>()).Returns(readRepoMock.Object);
    }

    private void SetupProductRepositories()
    {
        var productRepoMock = new Mock<IGenericRepository<Product>>();
        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);
    }

    private void SetupStockHistoryRepository()
    {
        var stockHistoryRepoMock = new Mock<IGenericRepository<StockHistory>>();
        _unitOfWorkMock.Setup(u => u.Repository<StockHistory>()).Returns(stockHistoryRepoMock.Object);
    }

    private void SetupTransactionMocks()
    {
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupStockSettingsMock(StockSettings? settings)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<StockSettings>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StockSettings, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);

        _unitOfWorkMock.Setup(u => u.ReadRepository<StockSettings>()).Returns(readRepoMock.Object);
    }

    private void SetupProductsListMock(List<Product> products)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Product>>();
        readRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string?>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(readRepoMock.Object);
    }

    private void SetupStockAlertRepository()
    {
        var stockAlertRepoMock = new Mock<IGenericRepository<StockAlert>>();
        var readStockAlertRepoMock = new Mock<IReadOnlyRepository<StockAlert>>();
        
        readStockAlertRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StockAlert, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockAlert?)null); // No existing alert

        _unitOfWorkMock.Setup(u => u.Repository<StockAlert>()).Returns(stockAlertRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<StockAlert>()).Returns(readStockAlertRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupExistingStockAlert(StockAlert alert)
    {
        var readStockAlertRepoMock = new Mock<IReadOnlyRepository<StockAlert>>();
        readStockAlertRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StockAlert, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(alert);

        _unitOfWorkMock.Setup(u => u.ReadRepository<StockAlert>()).Returns(readStockAlertRepoMock.Object);
    }

    private void SetupStockHistoryListMock(List<StockHistory> histories)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<StockHistory>>();
        readRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StockHistory, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<StockHistory, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string?>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        _unitOfWorkMock.Setup(u => u.ReadRepository<StockHistory>()).Returns(readRepoMock.Object);
    }

    private void SetupStockAlertListMock(List<StockAlert> alerts)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<StockAlert>>();
        readRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StockAlert, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<StockAlert, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string?>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerts);

        _unitOfWorkMock.Setup(u => u.ReadRepository<StockAlert>()).Returns(readRepoMock.Object);
    }

    private void SetupProductWithMerchantMock(Product? product)
    {
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
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupVariantWithMerchantMock(MarketProductVariant variant)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<MarketProductVariant>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<MarketProductVariant, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(variant);

        var variantRepoMock = new Mock<IGenericRepository<MarketProductVariant>>();
        _unitOfWorkMock.Setup(u => u.ReadRepository<MarketProductVariant>()).Returns(readRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<MarketProductVariant>()).Returns(variantRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    #endregion
}

