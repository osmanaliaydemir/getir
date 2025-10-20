using Getir.Application.Abstractions;
using Getir.Application.DTO;
using Getir.Application.Services.Stock;
using Getir.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Stok uyarı işlemleri için controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Stock Alerts")]
[Authorize]
public class StockAlertController : BaseController
{
    private readonly IStockAlertService _stockAlertService;
    private readonly IUnitOfWork _unitOfWork;

    public StockAlertController(
        IStockAlertService stockAlertService,
        IUnitOfWork unitOfWork)
    {
        _stockAlertService = stockAlertService;
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
    /// Creates low stock alerts
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("create-low-stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateLowStockAlerts(CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockAlertService.CreateLowStockAlertsAsync(merchantId, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Creates out of stock alerts
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("create-out-of-stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOutOfStockAlerts(CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockAlertService.CreateOutOfStockAlertsAsync(merchantId, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Creates overstock alerts
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("create-overstock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOverstockAlerts(CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockAlertService.CreateOverstockAlertsAsync(merchantId, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Resolves a stock alert
    /// </summary>
    /// <param name="alertId">Alert ID</param>
    /// <param name="request">Resolution request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPut("{alertId}/resolve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveStockAlert(
        [FromRoute] Guid alertId,
        [FromBody] ResolveStockAlertRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult is not null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult is not null) return unauthorizedResult;

        var result = await _stockAlertService.ResolveStockAlertAsync(alertId, userId, request.ResolutionNotes, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Gets stock alert statistics
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Stock alert statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(StockAlertStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStockAlertStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockAlertService.GetStockAlertStatisticsAsync(merchantId, fromDate, toDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sends stock alert notifications
    /// </summary>
    /// <param name="request">Alert notification request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("send-notifications")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendStockAlertNotifications(
        [FromBody] SendStockAlertNotificationsRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult is not null) return validationResult;

        var result = await _stockAlertService.SendStockAlertNotificationsAsync(request.AlertIds, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }

    /// <summary>
    /// Configures stock alert settings
    /// </summary>
    /// <param name="request">Stock alert settings request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPut("settings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ConfigureStockAlertSettings(
        [FromBody] StockAlertSettingsRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult is not null) return validationResult;

        var (error, merchantId) = await GetCurrentUserMerchantIdAsync(ct);
        if (error is not null) return error;

        var result = await _stockAlertService.ConfigureStockAlertSettingsAsync(request, merchantId, ct);
        return result.Success ? NoContent() : ToActionResult(result);
    }
}

// Additional DTOs for Stock Alert Controller
public record ResolveStockAlertRequest(string ResolutionNotes);
public record SendStockAlertNotificationsRequest(List<Guid> AlertIds);
