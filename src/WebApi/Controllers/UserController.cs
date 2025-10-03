using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Addresses;
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

    public UserController(IUserAddressService userAddressService)
    {
        _userAddressService = userAddressService;
    }

    /// <summary>
    /// Get user addresses
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User addresses</returns>
    [HttpGet("addresses")]
    [ProducesResponseType(typeof(IEnumerable<UserAddressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserAddresses(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userAddressService.GetUserAddressesAsync(userId, ct);
        return ToActionResult(result);
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
    public async Task<IActionResult> AddAddress(
        [FromBody] CreateAddressRequest request,
        CancellationToken ct = default)
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
    public async Task<IActionResult> UpdateAddress(
        [FromRoute] Guid id,
        [FromBody] UpdateAddressRequest request,
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
    public async Task<IActionResult> DeleteAddress(
        [FromRoute] Guid id,
        CancellationToken ct = default)
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
    public async Task<IActionResult> SetDefaultAddress(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _userAddressService.SetDefaultAddressAsync(userId, id, ct);
        return ToActionResult(result);
    }
}
