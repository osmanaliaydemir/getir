namespace Getir.Domain.Enums;

/// <summary>
/// İşletme dokümanı doğrulama durumları
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    /// Doküman yüklendi, doğrulama bekleniyor
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Doküman admin tarafından inceleniyor
    /// </summary>
    UnderReview = 1,
    
    /// <summary>
    /// Doküman admin tarafından onaylandı
    /// </summary>
    Approved = 2,
    
    /// <summary>
    /// Doküman admin tarafından reddedildi
    /// </summary>
    Rejected = 3,
    
    /// <summary>
    /// Doküman süresi doldu ve yenileme gerekiyor
    /// </summary>
    Expired = 4,
    
    /// <summary>
    /// Doküman geçersiz hale geldi
    /// </summary>
    Invalid = 5
}
