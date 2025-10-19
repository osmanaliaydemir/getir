using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Stock;

public interface IStockManagementService
{
    /// <summary>
    /// Automatically reduces stock when order is confirmed
    /// </summary>
    Task<Result> ReduceStockForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Restores stock when order is cancelled
    /// </summary>
    Task<Result> RestoreStockForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Checks stock levels and sends alerts if needed
    /// </summary>
    Task<Result> CheckStockLevelsAndAlertAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets stock history for a product
    /// </summary>
    Task<Result<List<StockHistoryResponse>>> GetStockHistoryAsync(Guid productId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets stock alerts for a merchant
    /// </summary>
    Task<Result<List<StockAlertResponse>>> GetStockAlertsAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates stock levels manually
    /// </summary>
    Task<Result> UpdateStockLevelAsync(UpdateStockRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Bulk updates stock levels
    /// </summary>
    Task<Result> BulkUpdateStockLevelsAsync(List<UpdateStockRequest> requests, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets stock reports
    /// </summary>
    Task<Result<StockReportResponse>> GetStockReportAsync(StockReportRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Synchronizes stock with external systems
    /// </summary>
    Task<Result> SynchronizeStockAsync(Guid merchantId, CancellationToken cancellationToken = default);
}
