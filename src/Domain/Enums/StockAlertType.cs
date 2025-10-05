namespace Getir.Domain.Enums;

/// <summary>
/// Stok uyarı türleri
/// </summary>
public enum StockAlertType
{
    /// <summary>
    /// Düşük stok uyarısı
    /// </summary>
    LowStock = 0,
    
    /// <summary>
    /// Stok tükendi uyarısı
    /// </summary>
    OutOfStock = 1,
    
    /// <summary>
    /// Aşırı stok uyarısı
    /// </summary>
    Overstock = 2,
    
    /// <summary>
    /// Stok hareket uyarısı
    /// </summary>
    StockMovement = 3,
    
    /// <summary>
    /// Stok senkronizasyon uyarısı
    /// </summary>
    SyncError = 4,
    
    /// <summary>
    /// Stok ayarları uyarısı
    /// </summary>
    SettingsWarning = 5
}
