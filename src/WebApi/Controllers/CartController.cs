using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Shopping cart controller for managing cart operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Shopping Cart")]
[Authorize]
public class CartController : BaseController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Get user's cart
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Cart details</returns>
    [HttpGet]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCart(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _cartService.GetCartAsync(userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Add item to cart
    /// </summary>
    /// <param name="request">Add to cart request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Added cart item</returns>
    [HttpPost("items")]
    [ProducesResponseType(typeof(CartItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddToCart(
        [FromBody] AddToCartRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _cartService.AddItemAsync(userId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update cart item
    /// </summary>
    /// <param name="id">Cart item ID</param>
    /// <param name="request">Update cart item request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated cart item</returns>
    [HttpPut("items/{id:guid}")]
    [ProducesResponseType(typeof(CartItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCartItem(
        [FromRoute] Guid id,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _cartService.UpdateItemAsync(userId, id, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    /// <param name="id">Cart item ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("items/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCartItem(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _cartService.RemoveItemAsync(userId, id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Clear entire cart
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("clear")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ClearCart(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _cartService.ClearCartAsync(userId, ct);
        return ToActionResult(result);
    }
}
