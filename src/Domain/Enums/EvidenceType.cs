namespace Getir.Domain.Enums;

/// <summary>
/// Kanıt türleri
/// </summary>
public enum EvidenceType
{
    /// <summary>
    /// Para toplama fotoğrafı
    /// </summary>
    CashCollectionPhoto = 1,
    
    /// <summary>
    /// Müşteri imzası
    /// </summary>
    CustomerSignature = 2,
    
    /// <summary>
    /// Para üstü fotoğrafı
    /// </summary>
    ChangePhoto = 3,
    
    /// <summary>
    /// Teslimat fotoğrafı
    /// </summary>
    DeliveryPhoto = 4,
    
    /// <summary>
    /// Video kaydı
    /// </summary>
    Video = 5,
    
    /// <summary>
    /// Ses kaydı
    /// </summary>
    Audio = 6
}
