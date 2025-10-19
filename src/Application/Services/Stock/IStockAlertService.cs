using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Stock;

/// <summary>
/// Stock alert service interface
/// </summary>
public interface IStockAlertService
{
    /// <summary>
    /// Creates stock alerts for low stock items
    /// </summary>
    Task<Result> CreateLowStockAlertsAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Creates stock alerts for out of stock items
    /// </summary>
    Task<Result> CreateOutOfStockAlertsAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Creates stock alerts for overstock items
    /// </summary>
    Task<Result> CreateOverstockAlertsAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Resolves a stock alert
    /// </summary>
    Task<Result> ResolveStockAlertAsync(Guid alertId, Guid resolvedBy, string resolutionNotes, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets stock alert statistics
    /// </summary>
    Task<Result<StockAlertStatisticsResponse>> GetStockAlertStatisticsAsync(Guid merchantId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Sends stock alert notifications
    /// </summary>
    Task<Result> SendStockAlertNotificationsAsync(List<Guid> alertIds, CancellationToken cancellationToken = default);
    /// <summary>
    /// Configures stock alert settings
    /// </summary>
    Task<Result> ConfigureStockAlertSettingsAsync(StockAlertSettingsRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
}
