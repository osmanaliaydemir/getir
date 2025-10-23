namespace Getir.Domain.Enums;

/// <summary>
/// Stok senkronizasyon türleri
/// </summary>
public enum StockSyncType
{
    /// <summary>
    /// Manuel senkronizasyon
    /// </summary>
    Manual = 0,
    
    /// <summary>
    /// Otomatik senkronizasyon
    /// </summary>
    Automatic = 1,
    
    /// <summary>
    /// Planlanmış senkronizasyon
    /// </summary>
    Scheduled = 2,
    
    /// <summary>
    /// Gerçek zamanlı senkronizasyon
    /// </summary>
    Realtime = 3
}
