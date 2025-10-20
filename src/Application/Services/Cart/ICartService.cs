using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Cart;

/// <summary>
/// Sepet servisi interface'i: sepet görüntüleme, ürün ekleme/güncelleme/silme ve sepeti temizleme işlemleri.
/// </summary>
public interface ICartService
{
    /// <summary>Kullanıcının sepetini getirir.</summary>
    Task<Result<CartResponse>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>Sepete ürün ekler.</summary>
    Task<Result<CartItemResponse>> AddItemAsync(Guid userId, AddToCartRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>Sepetteki ürünü günceller.</summary>
    Task<Result<CartItemResponse>> UpdateItemAsync(Guid userId, Guid itemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>Sepetten ürün kaldırır.</summary>
    Task<Result> RemoveItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);
    
    /// <summary>Sepeti tamamen temizler.</summary>
    Task<Result> ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
}
