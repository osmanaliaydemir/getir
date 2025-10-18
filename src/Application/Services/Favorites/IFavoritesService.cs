using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Favorites;

public interface IFavoritesService
{
    Task<Result<PagedResult<FavoriteProductResponse>>> GetUserFavoritesAsync(
        Guid userId,
        PaginationQuery query,
        CancellationToken cancellationToken = default);

    Task<Result> AddToFavoritesAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<Result> RemoveFromFavoritesAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> IsFavoriteAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default);
}

