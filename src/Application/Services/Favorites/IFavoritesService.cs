using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Favorites;

/// <summary>
/// Favori ürünler servisi: kullanıcı favorilerine ekleme/çıkarma ve listeleme işlemleri.
/// </summary>
public interface IFavoritesService
{
    /// <summary>Kullanıcının favori ürünlerini sayfalama ile getirir.</summary>
    Task<Result<PagedResult<FavoriteProductResponse>>> GetUserFavoritesAsync(Guid userId, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Ürünü favorilere ekler.</summary>
    Task<Result> AddToFavoritesAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    /// <summary>Ürünü favorilerden çıkarır.</summary>
    Task<Result> RemoveFromFavoritesAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    /// <summary>Ürünün favori olup olmadığını kontrol eder.</summary>
    Task<Result<bool>> IsFavoriteAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
}

