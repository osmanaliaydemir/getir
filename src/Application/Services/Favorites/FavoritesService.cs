using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Favorites;

public class FavoritesService : BaseService, IFavoritesService
{
    public FavoritesService(IUnitOfWork unitOfWork, ILogger<FavoritesService> logger, ILoggingService loggingService, ICacheService cacheService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
    }
    public async Task<Result<PagedResult<FavoriteProductResponse>>> GetUserFavoritesAsync(Guid userId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetUserFavoritesInternalAsync(userId, query, cancellationToken),
            "GetUserFavorites",
            new { userId, Page = query.Page, PageSize = query.PageSize },
            cancellationToken);
    }
    private async Task<Result<PagedResult<FavoriteProductResponse>>> GetUserFavoritesInternalAsync(Guid userId, PaginationQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = string.Concat("user_favorites_", userId, "_", query.Page, "_", query.PageSize);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    // Query favorites from database using GetPagedAsync
                    var favorites = await _unitOfWork.Repository<FavoriteProduct>().GetPagedAsync(
                        filter: f => f.UserId == userId,
                        orderBy: f => f.AddedAt,
                        ascending: false,
                        page: query.Page,
                        pageSize: query.PageSize,
                        include: "Product,Product.Merchant",
                        cancellationToken: cancellationToken);

                    var total = await _unitOfWork.ReadRepository<FavoriteProduct>()
                        .CountAsync(f => f.UserId == userId, cancellationToken);

                    var favoriteResponses = favorites.Select(f => new FavoriteProductResponse
                    {
                        Id = f.Id,
                        ProductId = f.ProductId,
                        ProductName = f.Product.Name,
                        ProductDescription = f.Product.Description,
                        Price = f.Product.Price,
                        ImageUrl = f.Product.ImageUrl,
                        MerchantId = f.Product.MerchantId,
                        MerchantName = f.Product.Merchant?.Name ?? "",
                        IsAvailable = f.Product.IsAvailable,
                        AddedAt = f.AddedAt
                    }).ToList();

                    var pagedResult = PagedResult<FavoriteProductResponse>.Create(
                        favoriteResponses,
                        total,
                        query.Page,
                        query.PageSize);

                    return ServiceResult.Success(pagedResult);
                },
                TimeSpan.FromMinutes(ApplicationConstants.ShortCacheMinutes),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting user favorites", ex, new { userId, Page = query.Page, PageSize = query.PageSize });
            return ServiceResult.HandleException<PagedResult<FavoriteProductResponse>>(ex, _logger, "GetUserFavorites");
        }
    }
    public async Task<Result> AddToFavoritesAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await AddToFavoritesInternalAsync(userId, productId, cancellationToken),
            "AddToFavorites",
            new { userId, productId },
            cancellationToken);
    }
    private async Task<Result> AddToFavoritesInternalAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
    {
        // Check if product exists
        var product = await _unitOfWork.Repository<Product>()
            .GetByIdAsync(productId, cancellationToken);

        if (product == null)
        {
            return Result.Fail("Product not found", ErrorCodes.PRODUCT_NOT_FOUND);
        }

        // Check if already in favorites
        var existingFavorite = await _unitOfWork.ReadRepository<FavoriteProduct>()
            .FirstOrDefaultAsync(
                f => f.UserId == userId && f.ProductId == productId,
                cancellationToken: cancellationToken);

        if (existingFavorite != null)
        {
            return Result.Fail("Product already in favorites", ErrorCodes.PRODUCT_ALREADY_IN_FAVORITES);
        }

        // Add to favorites
        var favorite = new FavoriteProduct
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            AddedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<FavoriteProduct>().AddAsync(favorite, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await InvalidateCacheAsync($"user_favorites_{userId}_*", cancellationToken);

        _logger.LogInformation("Product {ProductId} added to favorites for user {UserId}", productId, userId);
        return Result.Ok();
    }
    public async Task<Result> RemoveFromFavoritesAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await RemoveFromFavoritesInternalAsync(userId, productId, cancellationToken),
            "RemoveFromFavorites",
            new { userId, productId },
            cancellationToken);
    }
    private async Task<Result> RemoveFromFavoritesInternalAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
    {
        var favorite = await _unitOfWork.ReadRepository<FavoriteProduct>()
            .FirstOrDefaultAsync(
                f => f.UserId == userId && f.ProductId == productId,
                cancellationToken: cancellationToken);

        if (favorite == null)
        {
            return Result.Fail("Favorite not found", ErrorCodes.PRODUCT_NOT_IN_FAVORITES);
        }

        _unitOfWork.Repository<FavoriteProduct>().Delete(favorite);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await InvalidateCacheAsync($"user_favorites_{userId}_*", cancellationToken);

        _logger.LogInformation("Product {ProductId} removed from favorites for user {UserId}", productId, userId);
        return Result.Ok();
    }
    public async Task<Result<bool>> IsFavoriteAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.ReadRepository<FavoriteProduct>()
            .AnyAsync(f => f.UserId == userId && f.ProductId == productId, cancellationToken);

        return Result.Ok(exists);
    }
}
