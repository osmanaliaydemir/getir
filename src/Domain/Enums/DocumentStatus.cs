namespace Getir.Domain.Enums;

/// <summary>
/// Merchant document verification status
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    /// Document uploaded, waiting for verification
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Document is under review by admin
    /// </summary>
    UnderReview = 1,
    
    /// <summary>
    /// Document approved by admin
    /// </summary>
    Approved = 2,
    
    /// <summary>
    /// Document rejected by admin
    /// </summary>
    Rejected = 3,
    
    /// <summary>
    /// Document expired and needs renewal
    /// </summary>
    Expired = 4,
    
    /// <summary>
    /// Document is no longer valid
    /// </summary>
    Invalid = 5
}
