using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Favorites;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class FavoritesServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<FavoritesService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly FavoritesService _service;

    public FavoritesServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<FavoritesService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        
        _service = new FavoritesService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task GetUserFavoritesAsync_WithFavorites_ShouldReturnList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new PaginationQuery { Page = 1, PageSize = 10 };
        var favorites = new List<FavoriteProduct>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, ProductId = Guid.NewGuid(), AddedAt = DateTime.UtcNow, Product = new Product { Name = "Pizza", Price = 50, Merchant = new Merchant { Name = "Restaurant" } } }
        };

        var favoriteRepoMock = new Mock<IGenericRepository<FavoriteProduct>>();
        favoriteRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<FavoriteProduct, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<FavoriteProduct, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(favorites);

        var readRepoMock = new Mock<IReadOnlyRepository<FavoriteProduct>>();
        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<FavoriteProduct, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _unitOfWorkMock.Setup(u => u.Repository<FavoriteProduct>()).Returns(favoriteRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<FavoriteProduct>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetUserFavoritesAsync(userId, query);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddToFavoritesAsync_ValidProduct_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var productRepoMock = new Mock<IGenericRepository<Product>>();
        productRepoMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = productId, Name = "Test Product" });

        var favoriteReadRepoMock = new Mock<IReadOnlyRepository<FavoriteProduct>>();
        favoriteReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<FavoriteProduct, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((FavoriteProduct?)null);

        var favoriteRepoMock = new Mock<IGenericRepository<FavoriteProduct>>();
        favoriteRepoMock.Setup(r => r.AddAsync(It.IsAny<FavoriteProduct>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FavoriteProduct());

        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<FavoriteProduct>()).Returns(favoriteReadRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<FavoriteProduct>()).Returns(favoriteRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.AddToFavoritesAsync(userId, productId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AddToFavoritesAsync_ProductNotFound_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var productRepoMock = new Mock<IGenericRepository<Product>>();
        productRepoMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);

        // Act
        var result = await _service.AddToFavoritesAsync(userId, productId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("PRODUCT_NOT_FOUND");
    }

    [Fact]
    public async Task AddToFavoritesAsync_AlreadyInFavorites_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var productRepoMock = new Mock<IGenericRepository<Product>>();
        productRepoMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = productId, Name = "Test Product" });

        var favoriteReadRepoMock = new Mock<IReadOnlyRepository<FavoriteProduct>>();
        favoriteReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<FavoriteProduct, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FavoriteProduct { UserId = userId, ProductId = productId });

        _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<FavoriteProduct>()).Returns(favoriteReadRepoMock.Object);

        // Act
        var result = await _service.AddToFavoritesAsync(userId, productId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("PRODUCT_ALREADY_IN_FAVORITES");
    }

    [Fact]
    public async Task RemoveFromFavoritesAsync_ValidFavorite_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var favoriteReadRepoMock = new Mock<IReadOnlyRepository<FavoriteProduct>>();
        favoriteReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<FavoriteProduct, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FavoriteProduct { Id = Guid.NewGuid(), UserId = userId, ProductId = productId });

        var favoriteRepoMock = new Mock<IGenericRepository<FavoriteProduct>>();
        favoriteRepoMock.Setup(r => r.Delete(It.IsAny<FavoriteProduct>()));

        _unitOfWorkMock.Setup(u => u.ReadRepository<FavoriteProduct>()).Returns(favoriteReadRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<FavoriteProduct>()).Returns(favoriteRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.RemoveFromFavoritesAsync(userId, productId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveFromFavoritesAsync_NotInFavorites_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var favoriteReadRepoMock = new Mock<IReadOnlyRepository<FavoriteProduct>>();
        favoriteReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<FavoriteProduct, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((FavoriteProduct?)null);

        _unitOfWorkMock.Setup(u => u.ReadRepository<FavoriteProduct>()).Returns(favoriteReadRepoMock.Object);

        // Act
        var result = await _service.RemoveFromFavoritesAsync(userId, productId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("PRODUCT_NOT_IN_FAVORITES");
    }

    [Fact]
    public async Task IsFavoriteAsync_ProductInFavorites_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var readRepoMock = new Mock<IReadOnlyRepository<FavoriteProduct>>();
        readRepoMock.Setup(r => r.AnyAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<FavoriteProduct, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(u => u.ReadRepository<FavoriteProduct>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.IsFavoriteAsync(userId, productId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeTrue();
    }
}

