using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Stock;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Getir.UnitTests.Services;

public class InventoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<InventoryService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly InventoryService _service;

    public InventoryServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<InventoryService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();

        _service = new InventoryService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object);
    }

    #region GetCurrentInventoryLevelsAsync Tests

    [Fact]
    public async Task GetCurrentInventoryLevelsAsync_ValidMerchant_ShouldReturnLevels()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var products = CreateSampleProducts(merchantId, 2);

        SetupProductsListMock(products);
        SetupVariantsListMock(new List<MarketProductVariant>());

        // Act
        var result = await _service.GetCurrentInventoryLevelsAsync(merchantId, includeVariants: false);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.First().ProductName.Should().NotBeNullOrEmpty();
        result.Value.First().CategoryName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCurrentInventoryLevelsAsync_WithVariants_ShouldIncludeVariants()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var products = CreateSampleProducts(merchantId, 1);
        var variants = CreateSampleVariants(products.First().Id, 2);

        SetupProductsListMock(products);
        SetupVariantsListMock(variants);

        // Act
        var result = await _service.GetCurrentInventoryLevelsAsync(merchantId, includeVariants: true);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCountGreaterOrEqualTo(3); // 1 product + 2 variants
    }

    #endregion

    #region GetInventoryDiscrepanciesAsync Tests

    [Fact]
    public async Task GetInventoryDiscrepanciesAsync_ValidMerchant_ShouldReturnDiscrepancies()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var discrepancies = CreateSampleDiscrepancies(merchantId, 3);

        SetupDiscrepanciesListMock(discrepancies);

        // Act
        var result = await _service.GetInventoryDiscrepanciesAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value.First().ProductName.Should().NotBeNullOrEmpty();
        result.Value.First().ExpectedQuantity.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetInventoryDiscrepanciesAsync_WithDateFilter_ShouldFilterByDate()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var fromDate = DateTime.UtcNow.AddDays(-7);
        var discrepancies = CreateSampleDiscrepancies(merchantId, 2);

        SetupDiscrepanciesListMock(discrepancies);

        // Act
        var result = await _service.GetInventoryDiscrepanciesAsync(merchantId, fromDate);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    #endregion

    #region GetSlowMovingInventoryAsync Tests

    [Fact]
    public async Task GetSlowMovingInventoryAsync_ValidMerchant_ShouldReturnSlowMovingItems()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var products = CreateSampleProducts(merchantId, 2);
        var stockHistory = CreateSampleStockHistory(products.First().Id, 1);

        SetupProductsListMock(products);
        SetupStockHistoryListMock(stockHistory);

        // Act
        var result = await _service.GetSlowMovingInventoryAsync(merchantId, daysThreshold: 30);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSlowMovingInventoryAsync_NoProducts_ShouldReturnEmptyList()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        SetupProductsListMock(new List<Product>());

        // Act
        var result = await _service.GetSlowMovingInventoryAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region GetInventoryValuationAsync Tests

    [Fact]
    public async Task GetInventoryValuationAsync_ValidMerchant_ShouldReturnValuation()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var products = CreateSampleProducts(merchantId, 3);

        SetupProductsListMock(products);

        // Act
        var result = await _service.GetInventoryValuationAsync(merchantId, ValuationMethod.FIFO);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TotalValue.Should().BeGreaterThan(0);
        result.Value.TotalItems.Should().Be(3);
        result.Value.Method.Should().Be(ValuationMethod.FIFO);
    }

    [Fact]
    public async Task GetInventoryValuationAsync_NoProducts_ShouldReturnZeroValuation()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        SetupProductsListMock(new List<Product>());

        // Act
        var result = await _service.GetInventoryValuationAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TotalValue.Should().Be(0);
        result.Value.TotalItems.Should().Be(0);
    }

    #endregion

    #region GetInventoryCountHistoryAsync Tests

    [Fact]
    public async Task GetInventoryCountHistoryAsync_ValidMerchant_ShouldReturnHistory()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var sessions = CreateSampleCountSessions(merchantId, 2);

        SetupCountSessionsListMock(sessions);

        // Act
        var result = await _service.GetInventoryCountHistoryAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.First().Status.Should().Be(InventoryCountStatus.Completed);
    }

    #endregion

    #region Helper Methods

    private List<Product> CreateSampleProducts(Guid merchantId, int count)
    {
        var products = new List<Product>();
        for (int i = 0; i < count; i++)
        {
            products.Add(new Product
            {
                Id = Guid.NewGuid(),
                MerchantId = merchantId,
                Name = $"Product {i + 1}",
                Description = $"Description {i + 1}",
                Price = 100 + (i * 10),
                StockQuantity = 50 + (i * 5),
                IsActive = true,
                ProductCategory = new ProductCategory
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Category"
                },
                CreatedAt = DateTime.UtcNow
            });
        }
        return products;
    }

    private List<MarketProductVariant> CreateSampleVariants(Guid productId, int count)
    {
        var variants = new List<MarketProductVariant>();
        for (int i = 0; i < count; i++)
        {
            variants.Add(new MarketProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Name = $"Variant {i + 1}",
                SKU = $"SKU-{i + 1}",
                Price = 80 + (i * 5),
                StockQuantity = 30 + (i * 3),
                CreatedAt = DateTime.UtcNow
            });
        }
        return variants;
    }

    private List<InventoryDiscrepancy> CreateSampleDiscrepancies(Guid merchantId, int count)
    {
        var discrepancies = new List<InventoryDiscrepancy>();
        for (int i = 0; i < count; i++)
        {
            var productId = Guid.NewGuid();
            discrepancies.Add(new InventoryDiscrepancy
            {
                Id = Guid.NewGuid(),
                CountSessionId = Guid.NewGuid(),
                ProductId = productId,
                ExpectedQuantity = 100,
                ActualQuantity = 95,
                Variance = -5,
                VariancePercentage = -5m,
                Status = InventoryDiscrepancyStatus.Pending,
                CountSession = new InventoryCountSession
                {
                    Id = Guid.NewGuid(),
                    MerchantId = merchantId,
                    CountDate = DateTime.UtcNow,
                    CountType = InventoryCountType.Full,
                    Status = InventoryCountStatus.Completed
                },
                Product = new Product
                {
                    Id = productId,
                    Name = $"Product {i + 1}",
                    MerchantId = merchantId
                },
                CreatedAt = DateTime.UtcNow
            });
        }
        return discrepancies;
    }

    private List<StockHistory> CreateSampleStockHistory(Guid productId, int count)
    {
        var history = new List<StockHistory>();
        for (int i = 0; i < count; i++)
        {
            history.Add(new StockHistory
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                PreviousQuantity = 100,
                NewQuantity = 90,
                ChangeAmount = -10,
                ChangeType = Domain.Enums.StockChangeType.Sale,
                ChangedAt = DateTime.UtcNow.AddDays(-60) // 60 days ago
            });
        }
        return history;
    }

    private List<InventoryCountSession> CreateSampleCountSessions(Guid merchantId, int count)
    {
        var sessions = new List<InventoryCountSession>();
        for (int i = 0; i < count; i++)
        {
            sessions.Add(new InventoryCountSession
            {
                Id = Guid.NewGuid(),
                MerchantId = merchantId,
                CountDate = DateTime.UtcNow.AddDays(-i),
                CountType = InventoryCountType.Full,
                Status = InventoryCountStatus.Completed,
                DiscrepancyCount = i + 1,
                Notes = $"Count session {i + 1}",
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                CompletedAt = DateTime.UtcNow.AddDays(-i).AddHours(2),
                CreatedByUser = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe"
                }
            });
        }
        return sessions;
    }

    private void SetupProductsListMock(List<Product> products)
    {
        var productRepo = new Mock<IReadOnlyRepository<Product>>();
        productRepo
            .Setup(r => r.ListAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Product>()).Returns(productRepo.Object);
    }

    private void SetupVariantsListMock(List<MarketProductVariant> variants)
    {
        var variantRepo = new Mock<IReadOnlyRepository<MarketProductVariant>>();
        variantRepo
            .Setup(r => r.ListAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<MarketProductVariant, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<MarketProductVariant, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(variants);
        _unitOfWorkMock.Setup(u => u.ReadRepository<MarketProductVariant>()).Returns(variantRepo.Object);
    }

    private void SetupDiscrepanciesListMock(List<InventoryDiscrepancy> discrepancies)
    {
        var discrepancyRepo = new Mock<IReadOnlyRepository<InventoryDiscrepancy>>();
        discrepancyRepo
            .Setup(r => r.ListAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<InventoryDiscrepancy, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<InventoryDiscrepancy, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(discrepancies);
        _unitOfWorkMock.Setup(u => u.ReadRepository<InventoryDiscrepancy>()).Returns(discrepancyRepo.Object);
    }

    private void SetupStockHistoryListMock(List<StockHistory> history)
    {
        var historyRepo = new Mock<IReadOnlyRepository<StockHistory>>();
        historyRepo
            .Setup(r => r.ListAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<StockHistory, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<StockHistory, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);
        _unitOfWorkMock.Setup(u => u.ReadRepository<StockHistory>()).Returns(historyRepo.Object);
    }

    private void SetupCountSessionsListMock(List<InventoryCountSession> sessions)
    {
        var sessionRepo = new Mock<IReadOnlyRepository<InventoryCountSession>>();
        sessionRepo
            .Setup(r => r.ListAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<InventoryCountSession, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<InventoryCountSession, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);
        _unitOfWorkMock.Setup(u => u.ReadRepository<InventoryCountSession>()).Returns(sessionRepo.Object);
    }

    #endregion
}

