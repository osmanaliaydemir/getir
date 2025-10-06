using Getir.Application.Abstractions;
using Getir.Application.DTO;
using Getir.Application.Services.Stock;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Controller for inventory management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Inventory Management")]
[Authorize]
public class InventoryController : BaseController
{
    private readonly IInventoryService _inventoryService;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryController(
        IInventoryService inventoryService,
        IUnitOfWork unitOfWork)
    {
        _inventoryService = inventoryService;
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
    /// Performs inventory count
    /// </summary>
    /// <param name="request">Inventory count request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("count")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PerformInventoryCount(
        [FromBody] InventoryCountRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult is not null) return validationResult;

        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _inventoryService.PerformInventoryCountAsync(request, merchantId, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Gets inventory count history
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Inventory count history</returns>
    [HttpGet("count/history")]
    [ProducesResponseType(typeof(List<InventoryCountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInventoryCountHistory(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _inventoryService.GetInventoryCountHistoryAsync(merchantId, fromDate, toDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets current inventory levels
    /// </summary>
    /// <param name="includeVariants">Include product variants</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Current inventory levels</returns>
    [HttpGet("levels")]
    [ProducesResponseType(typeof(List<InventoryLevelResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentInventoryLevels(
        [FromQuery] bool includeVariants = true,
        CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _inventoryService.GetCurrentInventoryLevelsAsync(merchantId, includeVariants, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets inventory discrepancies
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Inventory discrepancies</returns>
    [HttpGet("discrepancies")]
    [ProducesResponseType(typeof(List<InventoryDiscrepancyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInventoryDiscrepancies(
        [FromQuery] DateTime? fromDate = null,
        CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _inventoryService.GetInventoryDiscrepanciesAsync(merchantId, fromDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Adjusts inventory levels
    /// </summary>
    /// <param name="request">Inventory adjustment request</param>
    /// <param name="reason">Reason for adjustment</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPut("adjust")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AdjustInventoryLevels(
        [FromBody] List<InventoryAdjustmentRequest> request,
        [FromQuery] string reason = "Manual adjustment",
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult is not null) return validationResult;

        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _inventoryService.AdjustInventoryLevelsAsync(request, merchantId, reason, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Gets inventory turnover report
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Inventory turnover report</returns>
    [HttpGet("turnover-report")]
    [ProducesResponseType(typeof(InventoryTurnoverResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInventoryTurnoverReport(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _inventoryService.GetInventoryTurnoverReportAsync(merchantId, fromDate, toDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets slow moving inventory items
    /// </summary>
    /// <param name="daysThreshold">Days threshold for slow moving</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Slow moving inventory items</returns>
    [HttpGet("slow-moving")]
    [ProducesResponseType(typeof(List<SlowMovingInventoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSlowMovingInventory(
        [FromQuery] int daysThreshold = 30,
        CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _inventoryService.GetSlowMovingInventoryAsync(merchantId, daysThreshold, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets inventory valuation
    /// </summary>
    /// <param name="method">Valuation method</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Inventory valuation</returns>
    [HttpGet("valuation")]
    [ProducesResponseType(typeof(InventoryValuationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInventoryValuation(
        [FromQuery] ValuationMethod method = ValuationMethod.FIFO,
        CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _inventoryService.GetInventoryValuationAsync(merchantId, method, ct);
        return ToActionResult(result);
    }
}
