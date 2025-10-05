namespace Getir.Domain.Enums;

/// <summary>
/// Stock synchronization detail status
/// </summary>
public enum StockSyncDetailStatus
{
    /// <summary>
    /// Item synchronized successfully
    /// </summary>
    Success = 0,
    
    /// <summary>
    /// Item synchronization failed
    /// </summary>
    Failed = 1,
    
    /// <summary>
    /// Item was skipped
    /// </summary>
    Skipped = 2,
    
    /// <summary>
    /// Item is pending synchronization
    /// </summary>
    Pending = 3
}
