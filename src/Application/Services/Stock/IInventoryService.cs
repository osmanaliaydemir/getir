using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Stock;

/// <summary>
/// Inventory management service interface
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// Performs inventory count for products
    /// </summary>
    Task<Result> PerformInventoryCountAsync(
        InventoryCountRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inventory count history
    /// </summary>
    Task<Result<List<InventoryCountResponse>>> GetInventoryCountHistoryAsync(
        Guid merchantId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current inventory levels
    /// </summary>
    Task<Result<List<InventoryLevelResponse>>> GetCurrentInventoryLevelsAsync(
        Guid merchantId,
        bool includeVariants = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inventory discrepancies
    /// </summary>
    Task<Result<List<InventoryDiscrepancyResponse>>> GetInventoryDiscrepanciesAsync(
        Guid merchantId,
        DateTime? fromDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adjusts inventory levels based on discrepancies
    /// </summary>
    Task<Result> AdjustInventoryLevelsAsync(
        List<InventoryAdjustmentRequest> adjustments,
        Guid merchantOwnerId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inventory turnover report
    /// </summary>
    Task<Result<InventoryTurnoverResponse>> GetInventoryTurnoverReportAsync(
        Guid merchantId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets slow moving inventory items
    /// </summary>
    Task<Result<List<SlowMovingInventoryResponse>>> GetSlowMovingInventoryAsync(
        Guid merchantId,
        int daysThreshold = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inventory valuation
    /// </summary>
    Task<Result<InventoryValuationResponse>> GetInventoryValuationAsync(
        Guid merchantId,
        ValuationMethod method = ValuationMethod.FIFO,
        CancellationToken cancellationToken = default);
}
