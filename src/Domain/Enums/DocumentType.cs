namespace Getir.Domain.Enums;

/// <summary>
/// İşletme onboarding için gerekli doküman tipleri
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Vergi Levhası
    /// </summary>
    TaxCertificate = 1,
    
    /// <summary>
    /// İşletme Ruhsatı
    /// </summary>
    BusinessLicense = 2,
    
    /// <summary>
    /// Kimlik Belgesi
    /// </summary>
    IdentityCard = 3,
    
    /// <summary>
    /// Ticaret Sicil Belgesi
    /// </summary>
    TradeRegistry = 4,
    
    /// <summary>
    /// Gıda İşletme Belgesi
    /// </summary>
    FoodServiceLicense = 5,
    
    /// <summary>
    /// Sağlık Belgesi
    /// </summary>
    HealthCertificate = 6,
    
    /// <summary>
    /// Sigorta Belgesi
    /// </summary>
    InsuranceCertificate = 7,
    
    /// <summary>
    /// Banka Hesap Dökümü
    /// </summary>
    BankStatement = 8,
    
    /// <summary>
    /// Diğer destekleyici dokümanlar
    /// </summary>
    Other = 99
}
