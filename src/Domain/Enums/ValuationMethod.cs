namespace Getir.Domain.Enums;

/// <summary>
/// Envanter değerleme yöntemleri
/// </summary>
public enum ValuationMethod
{
    /// <summary>
    /// İlk Giren, İlk Çıkar
    /// </summary>
    FIFO = 0,
    
    /// <summary>
    /// Son Giren, İlk Çıkar
    /// </summary>
    LIFO = 1,
    
    /// <summary>
    /// Ağırlıklı Ortalama Maliyet
    /// </summary>
    WeightedAverage = 2,
    
    /// <summary>
    /// Güncel Piyasa Fiyatı
    /// </summary>
    MarketPrice = 3,
    
    /// <summary>
    /// Standart Maliyet
    /// </summary>
    StandardCost = 4
}
