namespace Getir.Domain.Enums;

/// <summary>
/// Stok senkronizasyon detay durumları
/// </summary>
public enum StockSyncDetailStatus
{
    /// <summary>
    /// Öğe başarıyla senkronize edildi
    /// </summary>
    Success = 0,
    
    /// <summary>
    /// Öğe senkronizasyonu başarısız oldu
    /// </summary>
    Failed = 1,
    
    /// <summary>
    /// Öğe atlandı
    /// </summary>
    Skipped = 2,
    
    /// <summary>
    /// Öğe senkronizasyonu bekleniyor
    /// </summary>
    Pending = 3
}
