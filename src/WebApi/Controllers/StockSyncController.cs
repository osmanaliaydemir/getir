using Getir.Application.Abstractions;
using Getir.Application.DTO;
using Getir.Application.Services.Stock;
using Getir.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Stok senkronizasyon işlemleri için controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Stock Synchronization")]
[Authorize]
public class StockSyncController : BaseController
{
    private readonly IStockSyncService _stockSyncService;
    private readonly IUnitOfWork _unitOfWork;

    public StockSyncController(
        IStockSyncService stockSyncService,
        IUnitOfWork unitOfWork)
    {
        _stockSyncService = stockSyncService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Gets the merchant ID for the current user
    /// </summary>
    private async Task<(IActionResult? Error, Guid MerchantId)> GetCurrentUserMerchantIdAsync(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult is not null) return (unauthorizedResult, Guid.Empty);

        var user = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Id == userId, "OwnedMerchants", ct);

        if (user is null || !user.OwnedMerchants.Any())
        {
            return (BadRequest("User is not associated with a merchant"), Guid.Empty);
        }

        return (null, user.OwnedMerchants.First().Id);
    }

    /// <summary>
    /// Synchronizes stock with external system
    /// </summary>
    /// <param name="request">Stock sync request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Stock sync response</returns>
    [HttpPost("sync")]
    [ProducesResponseType(typeof(StockSyncResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SynchronizeStock(
        [FromBody] StockSyncRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult is not null) return validationResult;

        var result = await _stockSyncService.SynchronizeWithExternalSystemAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets synchronization history
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Synchronization history</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<StockSyncHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSynchronizationHistory(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockSyncService.GetSynchronizationHistoryAsync(merchantId, fromDate, toDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets synchronization status
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Synchronization status</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(StockSyncStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSynchronizationStatus(CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockSyncService.GetSynchronizationStatusAsync(merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Configures external system connection
    /// </summary>
    /// <param name="request">External system config request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPut("configure")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ConfigureExternalSystem(
        [FromBody] ExternalSystemConfigRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult is not null) return validationResult;

        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockSyncService.ConfigureExternalSystemAsync(request, merchantId, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Tests external system connection
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Connection test response</returns>
    [HttpPost("test-connection")]
    [ProducesResponseType(typeof(ConnectionTestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TestExternalSystemConnection(CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockSyncService.TestExternalSystemConnectionAsync(merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Schedules automatic synchronization
    /// </summary>
    /// <param name="request">Schedule sync request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("schedule")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ScheduleAutomaticSync(
        [FromBody] ScheduleSyncRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult is not null) return validationResult;

        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockSyncService.ScheduleAutomaticSyncAsync(merchantId, request.IntervalMinutes, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Cancels automatic synchronization
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("cancel-schedule")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelAutomaticSync(CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockSyncService.CancelAutomaticSyncAsync(merchantId, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }
}

// Additional DTOs for Stock Sync Controller
public record ScheduleSyncRequest(int IntervalMinutes);
