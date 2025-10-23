namespace Getir.Domain.Enums;

/// <summary>
/// Stok sapması durumları
/// </summary>
public enum InventoryDiscrepancyStatus
{
    /// <summary>
    /// Stok sapması bekleniyor
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Stok sapması çözüldü
    /// </summary>
    Resolved = 1,
    
    /// <summary>
    /// Stok sapması incelemeye alındı
    /// </summary>
    Investigating = 2,
    
    /// <summary>
    /// Stok sapması onaylandı
    /// </summary>
    Approved = 3,
    
    /// <summary>
    /// Stok sapması reddedildi
    /// </summary>
    Rejected = 4
}
