using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Stock;

/// <summary>
/// Stock synchronization service interface
/// </summary>
public interface IStockSyncService
{
    /// <summary>
    /// Synchronizes stock with external system
    /// </summary>
    Task<Result<StockSyncResponse>> SynchronizeWithExternalSystemAsync(
        StockSyncRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets synchronization history
    /// </summary>
    Task<Result<List<StockSyncHistoryResponse>>> GetSynchronizationHistoryAsync(
        Guid merchantId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets synchronization status
    /// </summary>
    Task<Result<StockSyncStatusResponse>> GetSynchronizationStatusAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Configures external system connection
    /// </summary>
    Task<Result> ConfigureExternalSystemAsync(
        ExternalSystemConfigRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests external system connection
    /// </summary>
    Task<Result<ConnectionTestResponse>> TestExternalSystemConnectionAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules automatic synchronization
    /// </summary>
    Task<Result> ScheduleAutomaticSyncAsync(
        Guid merchantId,
        int intervalMinutes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels automatic synchronization
    /// </summary>
    Task<Result> CancelAutomaticSyncAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default);
}
