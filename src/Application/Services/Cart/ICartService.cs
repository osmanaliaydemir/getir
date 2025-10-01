using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Cart;

public interface ICartService
{
    Task<Result<CartResponse>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<CartItemResponse>> AddItemAsync(Guid userId, AddToCartRequest request, CancellationToken cancellationToken = default);
    Task<Result<CartItemResponse>> UpdateItemAsync(Guid userId, Guid itemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default);
    Task<Result> RemoveItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);
    Task<Result> ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
}
