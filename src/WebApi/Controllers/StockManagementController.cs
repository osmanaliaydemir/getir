using Getir.Application.Abstractions;
using Getir.Application.DTO;
using Getir.Application.Services.Stock;
using Getir.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Stok yönetimi işlemleri için controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Stock Management")]
[Authorize]
public class StockManagementController : BaseController
{
    private readonly IStockManagementService _stockService;
    private readonly IUnitOfWork _unitOfWork;

    public StockManagementController(
        IStockManagementService stockService,
        IUnitOfWork unitOfWork)
    {
        _stockService = stockService;
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
    /// Update stock level for a product
    /// </summary>
    /// <param name="request">Stock update request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPut("update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStockLevel(
        [FromBody] UpdateStockRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult is not null) return validationResult;

        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockService.UpdateStockLevelAsync(request, merchantId, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Bulk update stock levels
    /// </summary>
    /// <param name="request">Bulk stock update request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPut("bulk-update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkUpdateStockLevels(
        [FromBody] BulkUpdateStockRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult is not null) return validationResult;

        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockService.BulkUpdateStockLevelsAsync(request.StockUpdates, merchantId, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Get stock history for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Stock history</returns>
    [HttpGet("history/{productId}")]
    [ProducesResponseType(typeof(List<StockHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStockHistory(
        [FromRoute] Guid productId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out _);
        if (unauthorizedResult is not null) return unauthorizedResult;

        var result = await _stockService.GetStockHistoryAsync(productId, fromDate, toDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get stock alerts for current merchant
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Stock alerts</returns>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(List<StockAlertResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStockAlerts(
        CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockService.GetStockAlertsAsync(merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get stock report
    /// </summary>
    /// <param name="request">Stock report request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Stock report</returns>
    [HttpPost("report")]
    [ProducesResponseType(typeof(StockReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStockReport(
        [FromBody] StockReportRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult is not null) return validationResult;

        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockService.GetStockReportAsync(request, merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Check stock levels and send alerts
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("check-alerts/{merchantId}")]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CheckStockAlerts(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out _);
        if (unauthorizedResult is not null) return unauthorizedResult;

        var result = await _stockService.CheckStockLevelsAndAlertAsync(merchantId, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Synchronize stock with external systems
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("sync/{merchantId}")]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SynchronizeStock(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out _);
        if (unauthorizedResult is not null) return unauthorizedResult;

        var result = await _stockService.SynchronizeStockAsync(merchantId, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Reduce stock for an order (internal use)
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("reduce-for-order/{orderId}")]
    [Authorize(Roles = "Admin,System")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReduceStockForOrder(
        [FromRoute] Guid orderId,
        CancellationToken ct = default)
    {
        var result = await _stockService.ReduceStockForOrderAsync(orderId, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Restore stock for an order (internal use)
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("restore-for-order/{orderId}")]
    [Authorize(Roles = "Admin,System")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RestoreStockForOrder(
        [FromRoute] Guid orderId,
        CancellationToken ct = default)
    {
        var result = await _stockService.RestoreStockForOrderAsync(orderId, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }
}
