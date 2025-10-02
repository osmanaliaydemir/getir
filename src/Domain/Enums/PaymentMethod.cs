namespace Getir.Domain.Enums;

/// <summary>
/// Desteklenen ödeme yöntemleri
/// Şu anda sadece Cash aktif, diğerleri gelecekte eklenecek
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Kapıda nakit ödeme (Aktif)
    /// </summary>
    Cash = 1,
    
    /// <summary>
    /// Kredi kartı ödeme (Gelecekte eklenecek)
    /// </summary>
    CreditCard = 2,
    
    /// <summary>
    /// Vodafone Pay ödeme (Gelecekte eklenecek)
    /// </summary>
    VodafonePay = 3,
    
    /// <summary>
    /// Havale/EFT ödeme (Gelecekte eklenecek)
    /// </summary>
    BankTransfer = 4,
    
    /// <summary>
    /// BKM Express ödeme (Gelecekte eklenecek)
    /// </summary>
    BkmExpress = 5,
    
    /// <summary>
    /// Papara ödeme (Gelecekte eklenecek)
    /// </summary>
    Papara = 6,
    
    /// <summary>
    /// QR Code ödeme (Gelecekte eklenecek)
    /// </summary>
    QrCode = 7
}
