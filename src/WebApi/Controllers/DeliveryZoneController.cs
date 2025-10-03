using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.DeliveryZones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Delivery zone controller for managing delivery zones
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Delivery Zones")]
public class DeliveryZoneController : BaseController
{
    private readonly IDeliveryZoneService _deliveryZoneService;

    public DeliveryZoneController(IDeliveryZoneService deliveryZoneService)
    {
        _deliveryZoneService = deliveryZoneService;
    }

    /// <summary>
    /// Get delivery zones for a merchant
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of delivery zones</returns>
    [HttpGet("merchant/{merchantId:guid}")]
    [ProducesResponseType(typeof(List<DeliveryZoneResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeliveryZonesByMerchant(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var result = await _deliveryZoneService.GetDeliveryZonesByMerchantAsync(merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get specific delivery zone
    /// </summary>
    /// <param name="id">Delivery zone ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Delivery zone details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DeliveryZoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeliveryZoneById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await _deliveryZoneService.GetDeliveryZoneByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Create delivery zone
    /// </summary>
    /// <param name="request">Create delivery zone request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created delivery zone</returns>
    [HttpPost]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(DeliveryZoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateDeliveryZone(
        [FromBody] CreateDeliveryZoneRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _deliveryZoneService.CreateDeliveryZoneAsync(request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update delivery zone
    /// </summary>
    /// <param name="id">Delivery zone ID</param>
    /// <param name="request">Update delivery zone request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated delivery zone</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(DeliveryZoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDeliveryZone(
        [FromRoute] Guid id,
        [FromBody] UpdateDeliveryZoneRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _deliveryZoneService.UpdateDeliveryZoneAsync(id, request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete delivery zone
    /// </summary>
    /// <param name="id">Delivery zone ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDeliveryZone(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _deliveryZoneService.DeleteDeliveryZoneAsync(id, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Check if location is in delivery zone
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="request">Check delivery zone request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Check result</returns>
    [HttpPost("merchant/{merchantId:guid}/check")]
    [ProducesResponseType(typeof(CheckDeliveryZoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckDeliveryZone(
        [FromRoute] Guid merchantId,
        [FromBody] CheckDeliveryZoneRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _deliveryZoneService.CheckDeliveryZoneAsync(merchantId, request, ct);
        return ToActionResult(result);
    }
}
