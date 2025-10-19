using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Cart;

public class CartService : BaseService, ICartService
{
    private readonly IBackgroundTaskService _backgroundTaskService;
    public CartService(IUnitOfWork unitOfWork, ILogger<CartService> logger, ILoggingService loggingService, ICacheService cacheService, IBackgroundTaskService backgroundTaskService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _backgroundTaskService = backgroundTaskService;
    }
    public async Task<Result<CartResponse>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cartItems = await _unitOfWork.ReadRepository<CartItem>()
            .ListAsync(
                filter: c => c.UserId == userId,
                include: "Product,Merchant",
                cancellationToken: cancellationToken);

        if (!cartItems.Any())
        {
            return Result.Ok(new CartResponse(Guid.Empty, "", new List<CartItemResponse>(), 0, 0, 0, 0));
        }

        var merchant = cartItems.First().Merchant;
        var items = cartItems.Select(c => new CartItemResponse(
            c.Id,
            c.ProductId,
            c.Product.Name,
            c.Product.ImageUrl ?? "",
            c.Product.Price,
            c.Product.DiscountedPrice,
            c.Quantity,
            (c.Product.DiscountedPrice ?? c.Product.Price) * c.Quantity,
            c.Notes
        )).ToList();

        var subTotal = items.Sum(i => i.TotalPrice);
        var deliveryFee = merchant.DeliveryFee;
        var total = subTotal + deliveryFee;

        var response = new CartResponse(
            merchant.Id,
            merchant.Name,
            items,
            subTotal,
            deliveryFee,
            total,
            items.Count
        );

        return Result.Ok(response);
    }
    public async Task<Result<CartItemResponse>> AddItemAsync(Guid userId, AddToCartRequest request, CancellationToken cancellationToken = default)
    {
        // Sepette başka merchanttan ürün var mı kontrol et
        var existingCartItems = await _unitOfWork.ReadRepository<CartItem>()
            .ListAsync(filter: c => c.UserId == userId, cancellationToken: cancellationToken);

        if (existingCartItems.Any() && existingCartItems.First().MerchantId != request.MerchantId)
        {
            return Result.Fail<CartItemResponse>(
                "You can only add items from one merchant at a time. Please clear your cart first.",
                "CART_DIFFERENT_MERCHANT");
        }

        // Ürün var mı kontrol et
        var product = await _unitOfWork.ReadRepository<Product>()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive, cancellationToken: cancellationToken);

        if (product == null)
        {
            return Result.Fail<CartItemResponse>("Product not found", "NOT_FOUND_PRODUCT");
        }

        // Stok kontrolü
        if (product.StockQuantity < request.Quantity)
        {
            return Result.Fail<CartItemResponse>("Insufficient stock", "INSUFFICIENT_STOCK");
        }

        // Aynı ürün sepette var mı kontrol et
        var existingItem = await _unitOfWork.Repository<CartItem>()
            .GetAsync(c => c.UserId == userId && c.ProductId == request.ProductId, cancellationToken: cancellationToken);

        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.Notes = request.Notes;
            existingItem.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<CartItem>().Update(existingItem);
        }
        else
        {
            existingItem = new CartItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MerchantId = request.MerchantId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<CartItem>().AddAsync(existingItem, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CartItemResponse(
            existingItem.Id,
            product.Id,
            product.Name,
            product.ImageUrl ?? "",
            product.Price,
            product.DiscountedPrice,
            existingItem.Quantity,
            (product.DiscountedPrice ?? product.Price) * existingItem.Quantity,
            existingItem.Notes
        );

        return Result.Ok(response);
    }
    public async Task<Result<CartItemResponse>> UpdateItemAsync(Guid userId, Guid itemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default)
    {
        var cartItem = await _unitOfWork.Repository<CartItem>()
            .GetAsync(c => c.Id == itemId && c.UserId == userId, include: "Product", cancellationToken: cancellationToken);

        if (cartItem == null)
        {
            return Result.Fail<CartItemResponse>("Cart item not found", "NOT_FOUND_CART_ITEM");
        }

        cartItem.Quantity = request.Quantity;
        cartItem.Notes = request.Notes;
        cartItem.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<CartItem>().Update(cartItem);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CartItemResponse(
            cartItem.Id,
            cartItem.Product.Id,
            cartItem.Product.Name,
            cartItem.Product.ImageUrl ?? "",
            cartItem.Product.Price,
            cartItem.Product.DiscountedPrice,
            cartItem.Quantity,
            (cartItem.Product.DiscountedPrice ?? cartItem.Product.Price) * cartItem.Quantity,
            cartItem.Notes
        );

        return Result.Ok(response);
    }
    public async Task<Result> RemoveItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var cartItem = await _unitOfWork.Repository<CartItem>()
            .GetAsync(c => c.Id == itemId && c.UserId == userId, cancellationToken: cancellationToken);

        if (cartItem == null)
        {
            return Result.Fail("Cart item not found", "NOT_FOUND_CART_ITEM");
        }

        _unitOfWork.Repository<CartItem>().Remove(cartItem);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
    public async Task<Result> ClearCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cartItems = await _unitOfWork.Repository<CartItem>()
            .GetPagedAsync(filter: c => c.UserId == userId, cancellationToken: cancellationToken);

        if (cartItems.Any())
        {
            _unitOfWork.Repository<CartItem>().RemoveRange(cartItems);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok();
    }
}
