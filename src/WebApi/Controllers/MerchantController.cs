using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Merchants;
using Getir.Domain.Enums;
using Getir.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Merchant controller for managing merchants
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Merchants")]
public class MerchantController : BaseController
{
    private readonly IMerchantService _merchantService;

    public MerchantController(IMerchantService merchantService)
    {
        _merchantService = merchantService;
    }

    /// <summary>
    /// Get all merchants with pagination
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged list of merchants</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MerchantResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMerchants(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _merchantService.GetMerchantsAsync(query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get merchant by ID
    /// </summary>
    /// <param name="id">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Merchant details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MerchantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMerchantById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await _merchantService.GetMerchantByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get my merchant (current user's merchant)
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Merchant details</returns>
    [HttpGet("my-merchant")]
    [Authorize]
    [Authorize(Roles = "MerchantOwner")]
    [ProducesResponseType(typeof(MerchantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyMerchant(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _merchantService.GetMerchantByOwnerIdAsync(userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Create a new merchant
    /// </summary>
    /// <param name="request">Create merchant request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created merchant</returns>
    [HttpPost]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(MerchantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateMerchant(
        [FromBody] CreateMerchantRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _merchantService.CreateMerchantAsync(request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update merchant
    /// </summary>
    /// <param name="id">Merchant ID</param>
    /// <param name="request">Update merchant request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated merchant</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(MerchantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMerchant(
        [FromRoute] Guid id,
        [FromBody] UpdateMerchantRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _merchantService.UpdateMerchantAsync(id, request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete merchant
    /// </summary>
    /// <param name="id">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMerchant(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await _merchantService.DeleteMerchantAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get merchants by category type
    /// </summary>
    /// <param name="categoryType">Service category type</param>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged list of merchants</returns>
    [HttpGet("by-category-type/{categoryType}")]
    [ProducesResponseType(typeof(PagedResult<MerchantResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMerchantsByCategoryType(
        [FromRoute] ServiceCategoryType categoryType,
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _merchantService.GetMerchantsByCategoryTypeAsync(categoryType, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get active merchants by category type (without pagination)
    /// </summary>
    /// <param name="categoryType">Service category type</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of active merchants</returns>
    [HttpGet("active/by-category-type/{categoryType}")]
    [ProducesResponseType(typeof(IEnumerable<MerchantResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveMerchantsByCategoryType(
        [FromRoute] ServiceCategoryType categoryType,
        CancellationToken ct = default)
    {
        var result = await _merchantService.GetActiveMerchantsByCategoryTypeAsync(categoryType, ct);
        return ToActionResult(result);
    }
}
