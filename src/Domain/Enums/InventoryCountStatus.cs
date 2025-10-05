namespace Getir.Domain.Enums;

/// <summary>
/// Inventory count status
/// </summary>
public enum InventoryCountStatus
{
    /// <summary>
    /// Count is in progress
    /// </summary>
    InProgress = 0,
    
    /// <summary>
    /// Count has been completed
    /// </summary>
    Completed = 1,
    
    /// <summary>
    /// Count has been cancelled
    /// </summary>
    Cancelled = 2,
    
    /// <summary>
    /// Count is pending approval
    /// </summary>
    PendingApproval = 3,
    
    /// <summary>
    /// Count has been approved
    /// </summary>
    Approved = 4
}
