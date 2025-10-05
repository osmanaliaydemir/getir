namespace Getir.Domain.Enums;

/// <summary>
/// Stock synchronization types
/// </summary>
public enum StockSyncType
{
    /// <summary>
    /// Manual synchronization
    /// </summary>
    Manual = 0,
    
    /// <summary>
    /// Automatic synchronization
    /// </summary>
    Automatic = 1,
    
    /// <summary>
    /// Scheduled synchronization
    /// </summary>
    Scheduled = 2,
    
    /// <summary>
    /// Real-time synchronization
    /// </summary>
    Realtime = 3
}
