namespace Getir.Domain.Enums;

/// <summary>
/// Types of documents required for merchant onboarding
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Tax certificate (Vergi Levhası)
    /// </summary>
    TaxCertificate = 1,
    
    /// <summary>
    /// Business license (İşletme Ruhsatı)
    /// </summary>
    BusinessLicense = 2,
    
    /// <summary>
    /// Identity card (Kimlik Belgesi)
    /// </summary>
    IdentityCard = 3,
    
    /// <summary>
    /// Trade registry certificate (Ticaret Sicil Belgesi)
    /// </summary>
    TradeRegistry = 4,
    
    /// <summary>
    /// Food service license (Gıda İşletme Belgesi)
    /// </summary>
    FoodServiceLicense = 5,
    
    /// <summary>
    /// Health certificate (Sağlık Belgesi)
    /// </summary>
    HealthCertificate = 6,
    
    /// <summary>
    /// Insurance certificate (Sigorta Belgesi)
    /// </summary>
    InsuranceCertificate = 7,
    
    /// <summary>
    /// Bank account statement (Banka Hesap Dökümü)
    /// </summary>
    BankStatement = 8,
    
    /// <summary>
    /// Other supporting documents
    /// </summary>
    Other = 99
}
