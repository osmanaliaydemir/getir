using Getir.Application.DTO;
using Getir.Application.Services.UserPreferences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Bildirim tercihlerini yönetmek için kullanıcı tercihleri controller'ı
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("User Preferences")]
[Authorize]
public class UserPreferencesController : BaseController
{
    private readonly IUserPreferencesService _userPreferencesService;

    public UserPreferencesController(IUserPreferencesService userPreferencesService)
    {
        _userPreferencesService = userPreferencesService;
    }

    /// <summary>
    /// Mevcut kullanıcının bildirim tercihlerini getir
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(UserNotificationPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyPreferences(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userPreferencesService.GetOrCreateUserPreferencesAsync(userId, ct);
        return ToActionResult<UserNotificationPreferencesResponse>(result);
    }

    /// <summary>
    /// Update current user's notification preferences
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(UserNotificationPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMyPreferences(
        [FromBody] UpdateUserNotificationPreferencesRequest request,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userPreferencesService.UpdateUserPreferencesAsync(userId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get simplified merchant portal preferences
    /// </summary>
    [HttpGet("merchant")]
    [Authorize(Roles = "MerchantOwner,Admin")]
    [ProducesResponseType(typeof(MerchantNotificationPreferencesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMerchantPreferences(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userPreferencesService.GetMerchantPreferencesAsync(userId, ct);
        return ToActionResult<MerchantNotificationPreferencesResponse>(result);
    }

    /// <summary>
    /// Update simplified merchant portal preferences
    /// </summary>
    [HttpPut("merchant")]
    [Authorize(Roles = "MerchantOwner,Admin")]
    [ProducesResponseType(typeof(MerchantNotificationPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMerchantPreferences(
        [FromBody] UpdateMerchantNotificationPreferencesRequest request,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userPreferencesService.UpdateMerchantPreferencesAsync(userId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete current user's preferences (Admin only)
    /// </summary>
    [HttpDelete]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMyPreferences(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userPreferencesService.DeleteUserPreferencesAsync(userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get specific user's preferences (Admin only)
    /// </summary>
    [HttpGet("{userId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserNotificationPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserPreferences(
        [FromRoute] Guid userId,
        CancellationToken ct = default)
    {
        var result = await _userPreferencesService.GetUserPreferencesAsync(userId, ct);
        return ToActionResult<UserNotificationPreferencesResponse>(result);
    }

    /// <summary>
    /// Update specific user's preferences (Admin only)
    /// </summary>
    [HttpPut("{userId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserNotificationPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUserPreferences(
        [FromRoute] Guid userId,
        [FromBody] UpdateUserNotificationPreferencesRequest request,
        CancellationToken ct = default)
    {
        var result = await _userPreferencesService.UpdateUserPreferencesAsync(userId, request, ct);
        return ToActionResult(result);
    }
}

