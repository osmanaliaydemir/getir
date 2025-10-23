namespace Getir.Domain.Enums;

/// <summary>
/// Envanter sayım durumları
/// </summary>
public enum InventoryCountStatus
{
    /// <summary>
    /// Envanter sayımı devam ediyor
    /// </summary>
    InProgress = 0,
    
    /// <summary>
    /// Envanter sayımı tamamlandı
    /// </summary>
    Completed = 1,
    
    /// <summary>
    /// Envanter sayımı iptal edildi
    /// </summary>
    Cancelled = 2,
    
    /// <summary>
    /// Envanter sayımı onay bekliyor
    /// </summary>
    PendingApproval = 3,
    
    /// <summary>
    /// Envanter sayımı onaylandı
    /// </summary>
    Approved = 4
}
