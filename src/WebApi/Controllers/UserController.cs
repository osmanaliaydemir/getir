using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Addresses;
using Getir.Application.Services.Auth;
using Getir.Application.Services.UserPreferences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// User controller for managing user addresses
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Users")]
[Authorize]
public class UserController : BaseController
{
    private readonly IUserAddressService _userAddressService;
    private readonly IAuthService _authService;
    private readonly IUserPreferencesService _userPreferencesService;

    public UserController(
        IUserAddressService userAddressService,
        IAuthService authService,
        IUserPreferencesService userPreferencesService)
    {
        _userAddressService = userAddressService;
        _authService = authService;
        _userPreferencesService = userPreferencesService;
    }

    /// <summary>
    /// Get user addresses
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User addresses</returns>
    [HttpGet("addresses")]
    [ProducesResponseType(typeof(List<AddressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserAddresses(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userAddressService.GetUserAddressesAsync(userId, ct);
        return ToActionResult<List<AddressResponse>>(result);
    }

    /// <summary>
    /// Add user address
    /// </summary>
    /// <param name="request">Create address request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created address</returns>
    [HttpPost("addresses")]
    [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddAddress([FromBody] CreateAddressRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userAddressService.AddAddressAsync(userId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update user address
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <param name="request">Update address request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated address</returns>
    [HttpPut("addresses/{id:guid}")]
    [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateAddress([FromRoute] Guid id, [FromBody] UpdateAddressRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userAddressService.UpdateAddressAsync(userId, id, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete user address
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("addresses/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAddress([FromRoute] Guid id, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userAddressService.DeleteAddressAsync(userId, id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Set default address
    /// </summary>
    /// <param name="id">Address ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPut("addresses/{id:guid}/set-default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetDefaultAddress([FromRoute] Guid id, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userAddressService.SetDefaultAddressAsync(userId, id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User profile</returns>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _authService.GetUserProfileAsync(userId, ct);
        return ToActionResult<UserProfileResponse>(result);
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    /// <param name="request">Update profile request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated user profile</returns>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _authService.UpdateUserProfileAsync(userId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get user's favorite products
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of favorite products</returns>
    [HttpGet("favorites")]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFavorites(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // TODO: Implement favorites service
        return Ok(new List<object>());
    }

    /// <summary>
    /// Add product to favorites
    /// </summary>
    /// <param name="request">Add to favorites request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("favorites")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddToFavorites([FromBody] AddToFavoritesRequest request, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // TODO: Implement favorites service
        return Ok();
    }

    /// <summary>
    /// Remove product from favorites
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("favorites/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveFromFavorites([FromRoute] string productId, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // TODO: Implement favorites service
        return Ok();
    }

    /// <summary>
    /// Check if product is in favorites
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if product is in favorites</returns>
    [HttpGet("favorites/{productId}/status")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> IsFavorite([FromRoute] string productId, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // TODO: Implement favorites service
        return Ok(false);
    }

    /// <summary>
    /// Get user's order history
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of user orders</returns>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserOrders(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // TODO: Implement orders service to get user orders
        return Ok(new List<object>());
    }

    /// <summary>
    /// Get order details
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Order details</returns>
    [HttpGet("orders/{orderId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrderDetails([FromRoute] string orderId, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // TODO: Implement orders service to get order details
        return NotFound("Order not found");
    }

    /// <summary>
    /// Cancel user order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("orders/{orderId}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelOrder([FromRoute] string orderId, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // TODO: Implement orders service to cancel order
        return Ok();
    }

    /// <summary>
    /// Reorder from previous order
    /// </summary>
    /// <param name="orderId">Original order ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>New order details</returns>
    [HttpPost("orders/{orderId}/reorder")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Reorder(
        [FromRoute] string orderId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // TODO: Implement orders service to create new order from existing one
        return NotFound("Order not found");
    }


    /// <summary>
    /// Get user's notification preferences
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Notification preferences</returns>
    [HttpGet("notification-preferences")]
    [ProducesResponseType(typeof(UserNotificationPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotificationPreferences(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userPreferencesService.GetOrCreateUserPreferencesAsync(userId, ct);
        return ToActionResult<UserNotificationPreferencesResponse>(result);
    }

    /// <summary>
    /// Update user's notification preferences
    /// </summary>
    /// <param name="request">Notification preferences</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated preferences</returns>
    [HttpPut("notification-preferences")]
    [ProducesResponseType(typeof(UserNotificationPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateNotificationPreferences([FromBody] UpdateUserNotificationPreferencesRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userPreferencesService.UpdateUserPreferencesAsync(userId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Reset notification preferences to defaults
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Default preferences</returns>
    [HttpPost("notification-preferences/reset")]
    [ProducesResponseType(typeof(UserNotificationPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResetNotificationPreferences(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // Delete existing preferences to trigger recreation with defaults
        await _userPreferencesService.DeleteUserPreferencesAsync(userId, ct);
        var result = await _userPreferencesService.GetOrCreateUserPreferencesAsync(userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get user's language preference
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Language preference</returns>
    [HttpGet("language")]
    [ProducesResponseType(typeof(LanguagePreferenceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLanguage(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userPreferencesService.GetOrCreateUserPreferencesAsync(userId, ct);

        if (!result.Success)
            return ToActionResult(result);

        var languageResponse = new LanguagePreferenceResponse(result.Value.Language);
        return Ok(languageResponse);
    }

    /// <summary>
    /// Update user's language preference
    /// </summary>
    /// <param name="request">Language preference request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated language preference</returns>
    [HttpPut("language")]
    [ProducesResponseType(typeof(LanguagePreferenceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateLanguage([FromBody] UpdateUserLanguagePreferenceRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var updateRequest = new UpdateUserNotificationPreferencesRequest
        {
            Language = request.LanguageCode
        };

        var result = await _userPreferencesService.UpdateUserPreferencesAsync(userId, updateRequest, ct);

        if (!result.Success)
            return ToActionResult(result);

        var languageResponse = new LanguagePreferenceResponse(result.Value.Language);
        return Ok(languageResponse);
    }
}

public record AddToFavoritesRequest(string ProductId);
