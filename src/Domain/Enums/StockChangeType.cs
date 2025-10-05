namespace Getir.Domain.Enums;

/// <summary>
/// Stok değişiklik türleri
/// </summary>
public enum StockChangeType
{
    /// <summary>
    /// Satış nedeniyle stok düşürme
    /// </summary>
    Sale = 0,
    
    /// <summary>
    /// Yeniden stok ekleme
    /// </summary>
    Restock = 1,
    
    /// <summary>
    /// Manuel stok ayarlama
    /// </summary>
    ManualAdjustment = 2,
    
    /// <summary>
    /// Stok transferi
    /// </summary>
    Transfer = 3,
    
    /// <summary>
    /// Stok düzeltmesi
    /// </summary>
    Correction = 4,
    
    /// <summary>
    /// Stok sayımı
    /// </summary>
    InventoryCount = 5,
    
    /// <summary>
    /// Stok kaybı
    /// </summary>
    Loss = 6,
    
    /// <summary>
    /// Stok kazancı
    /// </summary>
    Gain = 7,
    
    /// <summary>
    /// Stok iadesi
    /// </summary>
    Return = 8,
    
    /// <summary>
    /// Stok senkronizasyonu
    /// </summary>
    Sync = 9,
    
    /// <summary>
    /// Diğer
    /// </summary>
    Other = 10
}
