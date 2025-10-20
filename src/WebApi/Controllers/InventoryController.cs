using Getir.Application.Abstractions;
using Getir.Application.DTO;
using Getir.Application.Services.Stock;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Envanter yönetimi işlemleri için controller
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
    /// Mevcut kullanıcı için merchant ID'sini alır
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
    /// Envanter sayımı gerçekleştirir
    /// </summary>
    /// <param name="request">Envanter sayım isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>İçerik yok</returns>
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
    /// Envanter sayım geçmişini alır
    /// </summary>
    /// <param name="fromDate">Başlangıç tarihi</param>
    /// <param name="toDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Envanter sayım geçmişi</returns>
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
    /// Mevcut envanter seviyelerini alır
    /// </summary>
    /// <param name="includeVariants">Ürün varyantlarını dahil et</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Mevcut envanter seviyeleri</returns>
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
    /// Envanter uyumsuzluklarını alır
    /// </summary>
    /// <param name="fromDate">Başlangıç tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Envanter uyumsuzlukları</returns>
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
    /// Envanter seviyelerini ayarlar
    /// </summary>
    /// <param name="request">Envanter ayarlama isteği</param>
    /// <param name="reason">Ayarlama nedeni</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>İçerik yok</returns>
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
    /// Envanter devir raporunu alır
    /// </summary>
    /// <param name="fromDate">Başlangıç tarihi</param>
    /// <param name="toDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Envanter devir raporu</returns>
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
    /// Yavaş hareket eden envanter öğelerini alır
    /// </summary>
    /// <param name="daysThreshold">Yavaş hareket için gün eşiği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Yavaş hareket eden envanter öğeleri</returns>
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
    /// Envanter değerlemesini alır
    /// </summary>
    /// <param name="method">Değerleme yöntemi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Envanter değerlemesi</returns>
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
