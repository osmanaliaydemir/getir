namespace Getir.Domain.Enums;

/// <summary>
/// Ödeme durumları - tüm ödeme yöntemleri için ortak
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Ödeme bekleniyor
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Ödeme başarılı
    /// </summary>
    Completed = 1,
    
    /// <summary>
    /// Ödeme başarısız
    /// </summary>
    Failed = 2,
    
    /// <summary>
    /// Ödeme iptal edildi
    /// </summary>
    Cancelled = 3,
    
    /// <summary>
    /// Ödeme geri alındı (refund)
    /// </summary>
    Refunded = 4,
    
    /// <summary>
    /// Ödeme işleniyor (3D Secure, banka onayı vb.)
    /// </summary>
    Processing = 5,
    
    /// <summary>
    /// Ödeme zaman aşımına uğradı
    /// </summary>
    Expired = 6
}
