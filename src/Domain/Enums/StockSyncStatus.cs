namespace Getir.Domain.Enums;

/// <summary>
/// Stok senkronizasyon durumları
/// </summary>
public enum StockSyncStatus
{
    /// <summary>
    /// Senkronizasyon devam ediyor
    /// </summary>
    InProgress = 0,
    
    /// <summary>
    /// Senkronizasyon başarıyla tamamlandı
    /// </summary>
    Success = 1,
    
    /// <summary>
    /// Senkronizasyon başarıyla tamamlandı fakat hatalar oluştu
    /// </summary>
    PartialSuccess = 2,
    
    /// <summary>
    /// Senkronizasyon başarısız oldu
    /// </summary>
    Failed = 3,
    
    /// <summary>
    /// Senkronizasyon iptal edildi
    /// </summary>
    Cancelled = 4
}
