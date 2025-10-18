using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Favorites;

public class FavoritesService : BaseService, IFavoritesService
{
    public FavoritesService(
        IUnitOfWork unitOfWork,
        ILogger<FavoritesService> logger,
        ILoggingService loggingService,
        ICacheService cacheService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
    }

    public async Task<Result<PagedResult<FavoriteProductResponse>>> GetUserFavoritesAsync(
        Guid userId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetUserFavoritesInternalAsync(userId, query, cancellationToken),
            "GetUserFavorites",
            new { userId, Page = query.Page, PageSize = query.PageSize },
            cancellationToken);
    }

    private async Task<Result<PagedResult<FavoriteProductResponse>>> GetUserFavoritesInternalAsync(
        Guid userId,
        PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"user_favorites_{userId}_page_{query.Page}_size_{query.PageSize}";
        var cachedResult = await CacheService.GetAsync<PagedResult<FavoriteProductResponse>>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            Logger.LogInformation("Favorites retrieved from cache for user {UserId}", userId);
            return Result<PagedResult<FavoriteProductResponse>>.Success(cachedResult);
        }

        // Query favorites from database
        var favoritesQuery = UnitOfWork.Repository<FavoriteProduct>()
            .GetQueryable()
            .Where(f => f.UserId == userId)
            .Include(f => f.Product)
            .ThenInclude(p => p.Merchant)
            .OrderByDescending(f => f.AddedAt);

        var totalCount = await favoritesQuery.CountAsync(cancellationToken);
        
        var favorites = await favoritesQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

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

        var result = new PagedResult<FavoriteProductResponse>
        {
            Data = favoriteResponses,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };

        // Cache result for 5 minutes
        await CacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);

        Logger.LogInformation(
            "Retrieved {Count} favorites for user {UserId} (Page {Page}/{TotalPages})",
            favoriteResponses.Count,
            userId,
            query.Page,
            result.TotalPages);

        return Result<PagedResult<FavoriteProductResponse>>.Success(result);
    }

    public async Task<Result> AddToFavoritesAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await AddToFavoritesInternalAsync(userId, productId, cancellationToken),
            "AddToFavorites",
            new { userId, productId },
            cancellationToken);
    }

    private async Task<Result> AddToFavoritesInternalAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        // Check if product exists
        var product = await UnitOfWork.Repository<Product>()
            .GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
        {
            return Result.Failure(ErrorCodes.ProductNotFound, "Product not found");
        }

        // Check if already in favorites
        var existingFavorite = await UnitOfWork.Repository<FavoriteProduct>()
            .GetQueryable()
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId, cancellationToken);

        if (existingFavorite != null)
        {
            return Result.Failure(ErrorCodes.AlreadyExists, "Product already in favorites");
        }

        // Add to favorites
        var favorite = new FavoriteProduct
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            AddedAt = DateTime.UtcNow
        };

        await UnitOfWork.Repository<FavoriteProduct>().AddAsync(favorite, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await InvalidateFavoritesCacheAsync(userId, cancellationToken);

        Logger.LogInformation("Product {ProductId} added to favorites for user {UserId}", productId, userId);
        return Result.Success();
    }

    public async Task<Result> RemoveFromFavoritesAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await RemoveFromFavoritesInternalAsync(userId, productId, cancellationToken),
            "RemoveFromFavorites",
            new { userId, productId },
            cancellationToken);
    }

    private async Task<Result> RemoveFromFavoritesInternalAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var favorite = await UnitOfWork.Repository<FavoriteProduct>()
            .GetQueryable()
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId, cancellationToken);

        if (favorite == null)
        {
            return Result.Failure(ErrorCodes.NotFound, "Favorite not found");
        }

        UnitOfWork.Repository<FavoriteProduct>().Delete(favorite);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await InvalidateFavoritesCacheAsync(userId, cancellationToken);

        Logger.LogInformation("Product {ProductId} removed from favorites for user {UserId}", productId, userId);
        return Result.Success();
    }

    public async Task<Result<bool>> IsFavoriteAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var exists = await UnitOfWork.Repository<FavoriteProduct>()
            .GetQueryable()
            .AnyAsync(f => f.UserId == userId && f.ProductId == productId, cancellationToken);

        return Result<bool>.Success(exists);
    }

    private async Task InvalidateFavoritesCacheAsync(Guid userId, CancellationToken cancellationToken)
    {
        // Invalidate all cache entries for this user's favorites
        var cacheKeyPattern = $"user_favorites_{userId}_*";
        await CacheService.RemoveByPatternAsync(cacheKeyPattern, cancellationToken);
    }
}

