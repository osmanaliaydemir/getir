namespace Getir.Domain.Enums;

/// <summary>
/// Stock synchronization status
/// </summary>
public enum StockSyncStatus
{
    /// <summary>
    /// Synchronization is in progress
    /// </summary>
    InProgress = 0,
    
    /// <summary>
    /// Synchronization completed successfully
    /// </summary>
    Success = 1,
    
    /// <summary>
    /// Synchronization completed with errors
    /// </summary>
    PartialSuccess = 2,
    
    /// <summary>
    /// Synchronization failed
    /// </summary>
    Failed = 3,
    
    /// <summary>
    /// Synchronization was cancelled
    /// </summary>
    Cancelled = 4
}
