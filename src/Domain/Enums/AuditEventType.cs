namespace Getir.Domain.Enums;

/// <summary>
/// Audit log event türleri
/// </summary>
public enum AuditEventType
{
    /// <summary>
    /// Nakit ödeme oluşturuldu
    /// </summary>
    CashPaymentCreated = 1,
    
    /// <summary>
    /// Nakit ödeme güncellendi
    /// </summary>
    CashPaymentUpdated = 2,
    
    /// <summary>
    /// Nakit ödeme tamamlandı
    /// </summary>
    CashPaymentCompleted = 3,
    
    /// <summary>
    /// Nakit ödeme iptal edildi
    /// </summary>
    CashPaymentCancelled = 4,
    
    /// <summary>
    /// Güvenlik riski tespit edildi
    /// </summary>
    SecurityRiskDetected = 5,
    
    /// <summary>
    /// Güvenlik riski çözüldü
    /// </summary>
    SecurityRiskResolved = 6,
    
    /// <summary>
    /// Kanıt oluşturuldu
    /// </summary>
    EvidenceCreated = 7,
    
    /// <summary>
    /// Kanıt doğrulandı
    /// </summary>
    EvidenceVerified = 8,
    
    /// <summary>
    /// Kanıt reddedildi
    /// </summary>
    EvidenceRejected = 9,
    
    /// <summary>
    /// Manuel onay gerekli
    /// </summary>
    ManualApprovalRequired = 10,
    
    /// <summary>
    /// Manuel onay verildi
    /// </summary>
    ManualApprovalGranted = 11,
    
    /// <summary>
    /// Manuel onay reddedildi
    /// </summary>
    ManualApprovalDenied = 12,
    
    /// <summary>
    /// Sahte para tespit edildi
    /// </summary>
    FakeMoneyDetected = 13,
    
    /// <summary>
    /// Para üstü hesaplama hatası
    /// </summary>
    ChangeCalculationError = 14,
    
    /// <summary>
    /// Kimlik doğrulama başarısız
    /// </summary>
    IdentityVerificationFailed = 15,
    
    /// <summary>
    /// Sistem hatası
    /// </summary>
    SystemError = 16,
    
    /// <summary>
    /// Kullanıcı girişi
    /// </summary>
    UserLogin = 17,
    
    /// <summary>
    /// Kullanıcı çıkışı
    /// </summary>
    UserLogout = 18,
    
    /// <summary>
    /// Yetkisiz erişim denemesi
    /// </summary>
    UnauthorizedAccessAttempt = 19,
    
    /// <summary>
    /// Veri değişikliği
    /// </summary>
    DataModification = 20,
    
    /// <summary>
    /// Kanıt güncellendi
    /// </summary>
    EvidenceUpdated = 21,
    
    /// <summary>
    /// Güvenlik kaydı güncellendi
    /// </summary>
    SecurityRecordUpdated = 22,
    
    /// <summary>
    /// Güvenlik kaydı onaylandı
    /// </summary>
    SecurityRecordApproved = 23,
    
    /// <summary>
    /// Güvenlik kaydı reddedildi
    /// </summary>
    SecurityRecordRejected = 24,
    
    /// <summary>
    /// Sahte para kontrolü yapıldı
    /// </summary>
    FakeMoneyCheckPerformed = 25,
    
    /// <summary>
    /// Kimlik doğrulama yapıldı
    /// </summary>
    IdentityVerificationPerformed = 26
}
