using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Nakit ödeme güvenlik kontrolleri
/// </summary>
public class CashPaymentSecurity
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    
    /// <summary>
    /// Para üstü hesaplama doğruluğu
    /// </summary>
    public bool ChangeCalculationVerified { get; set; }
    
    /// <summary>
    /// Hesaplanan para üstü
    /// </summary>
    public decimal CalculatedChange { get; set; }
    
    /// <summary>
    /// Verilen para üstü
    /// </summary>
    public decimal GivenChange { get; set; }
    
    /// <summary>
    /// Para üstü farkı
    /// </summary>
    public decimal ChangeDifference { get; set; }
    
    /// <summary>
    /// Sahte para kontrolü yapıldı mı?
    /// </summary>
    public bool FakeMoneyCheckPerformed { get; set; }
    
    /// <summary>
    /// Sahte para tespit edildi mi?
    /// </summary>
    public bool FakeMoneyDetected { get; set; }
    
    /// <summary>
    /// Sahte para notları
    /// </summary>
    public string? FakeMoneyNotes { get; set; }
    
    /// <summary>
    /// Müşteri kimlik doğrulaması yapıldı mı?
    /// </summary>
    public bool CustomerIdentityVerified { get; set; }
    
    /// <summary>
    /// Kimlik doğrulama türü (TC, Pasaport, Ehliyet)
    /// </summary>
    public string? IdentityVerificationType { get; set; }
    
    /// <summary>
    /// Kimlik numarası (hash'lenmiş)
    /// </summary>
    public string? IdentityNumberHash { get; set; }
    
    /// <summary>
    /// Güvenlik riski seviyesi (Low, Medium, High, Critical)
    /// </summary>
    public SecurityRiskLevel RiskLevel { get; set; } = SecurityRiskLevel.Low;
    
    /// <summary>
    /// Risk faktörleri (JSON format)
    /// </summary>
    public string? RiskFactors { get; set; }
    
    /// <summary>
    /// Güvenlik notları
    /// </summary>
    public string? SecurityNotes { get; set; }
    
    /// <summary>
    /// Manuel onay gerekli mi?
    /// </summary>
    public bool RequiresManualApproval { get; set; }
    
    /// <summary>
    /// Onaylayan admin ID
    /// </summary>
    public Guid? ApprovedByAdminId { get; set; }
    
    /// <summary>
    /// Onay tarihi
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Payment Payment { get; set; } = default!;
    public virtual User? ApprovedByAdmin { get; set; }
}

