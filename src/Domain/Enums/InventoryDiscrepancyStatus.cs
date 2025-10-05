namespace Getir.Domain.Enums;

/// <summary>
/// Inventory discrepancy status
/// </summary>
public enum InventoryDiscrepancyStatus
{
    /// <summary>
    /// Discrepancy is pending resolution
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Discrepancy has been resolved
    /// </summary>
    Resolved = 1,
    
    /// <summary>
    /// Discrepancy is under investigation
    /// </summary>
    Investigating = 2,
    
    /// <summary>
    /// Discrepancy has been approved
    /// </summary>
    Approved = 3,
    
    /// <summary>
    /// Discrepancy has been rejected
    /// </summary>
    Rejected = 4
}
