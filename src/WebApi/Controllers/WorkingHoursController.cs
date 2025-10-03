using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.WorkingHours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Working hours controller for managing working hours
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Working Hours")]
public class WorkingHoursController : BaseController
{
    private readonly IWorkingHoursService _workingHoursService;

    public WorkingHoursController(IWorkingHoursService workingHoursService)
    {
        _workingHoursService = workingHoursService;
    }

    /// <summary>
    /// Get working hours for a merchant
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of working hours</returns>
    [HttpGet("merchant/{merchantId:guid}")]
    [ProducesResponseType(typeof(List<WorkingHoursResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkingHoursByMerchant(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var result = await _workingHoursService.GetWorkingHoursByMerchantAsync(merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get specific working hours
    /// </summary>
    /// <param name="id">Working hours ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Working hours details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkingHoursResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkingHoursById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var result = await _workingHoursService.GetWorkingHoursByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Create working hours
    /// </summary>
    /// <param name="request">Create working hours request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created working hours</returns>
    [HttpPost]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(WorkingHoursResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateWorkingHours(
        [FromBody] CreateWorkingHoursRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _workingHoursService.CreateWorkingHoursAsync(request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update working hours
    /// </summary>
    /// <param name="id">Working hours ID</param>
    /// <param name="request">Update working hours request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated working hours</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(WorkingHoursResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWorkingHours(
        [FromRoute] Guid id,
        [FromBody] UpdateWorkingHoursRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _workingHoursService.UpdateWorkingHoursAsync(id, request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete working hours
    /// </summary>
    /// <param name="id">Working hours ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWorkingHours(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _workingHoursService.DeleteWorkingHoursAsync(id, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Bulk update working hours for a merchant
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="request">Bulk update request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPut("merchant/{merchantId:guid}/bulk")]
    [Authorize]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkUpdateWorkingHours(
        [FromRoute] Guid merchantId,
        [FromBody] BulkUpdateWorkingHoursRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _workingHoursService.BulkUpdateWorkingHoursAsync(merchantId, request, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Check if merchant is open
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="checkTime">Time to check (optional, defaults to now)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Whether merchant is open</returns>
    [HttpGet("merchant/{merchantId:guid}/is-open")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IsMerchantOpen(
        [FromRoute] Guid merchantId,
        [FromQuery] DateTime? checkTime = null,
        CancellationToken ct = default)
    {
        var result = await _workingHoursService.IsMerchantOpenAsync(merchantId, checkTime, ct);
        return ToActionResult(result);
    }
}
