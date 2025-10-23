namespace Getir.Domain.Enums;

/// <summary>
/// Envanter sayım türleri
/// </summary>
public enum InventoryCountType
{
    /// <summary>
    /// Tam envanter sayımı
    /// </summary>
    Full = 0,
    
    /// <summary>
    /// Kısmi envanter sayımı
    /// </summary>
    Partial = 1,
    
    /// <summary>
    /// Döngü sayımı
    /// </summary>
    Cycle = 2,
    
    /// <summary>
    /// Nokta kontrolü
    /// </summary>
    SpotCheck = 3,
    
    /// <summary>
    /// Yıllık sayımı
    /// </summary>
    Annual = 4
}
