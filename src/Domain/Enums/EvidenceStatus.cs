namespace Getir.Domain.Enums;

/// <summary>
/// Kanıt durumları
/// </summary>
public enum EvidenceStatus
{
    /// <summary>
    /// Beklemede
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Doğrulandı
    /// </summary>
    Verified = 1,
    
    /// <summary>
    /// Reddedildi
    /// </summary>
    Rejected = 2,
    
    /// <summary>
    /// İnceleme gerekli
    /// </summary>
    RequiresReview = 3
}
