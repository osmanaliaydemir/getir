using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Nakit ödeme audit log kayıtları
/// </summary>
public class CashPaymentAuditLog
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// İlgili ödeme ID'si
    /// </summary>
    public Guid? PaymentId { get; set; }
    
    /// <summary>
    /// İlgili kurye ID'si
    /// </summary>
    public Guid? CourierId { get; set; }
    
    /// <summary>
    /// İlgili müşteri ID'si
    /// </summary>
    public Guid? CustomerId { get; set; }
    
    /// <summary>
    /// İlgili admin ID'si (onaylayan/reddeden)
    /// </summary>
    public Guid? AdminId { get; set; }
    
    /// <summary>
    /// Audit event türü
    /// </summary>
    public AuditEventType EventType { get; set; }
    
    /// <summary>
    /// Severity seviyesi
    /// </summary>
    public AuditSeverityLevel SeverityLevel { get; set; }
    
    /// <summary>
    /// Event başlığı
    /// </summary>
    public string Title { get; set; } = default!;
    
    /// <summary>
    /// Event açıklaması
    /// </summary>
    public string Description { get; set; } = default!;
    
    /// <summary>
    /// Detaylı bilgiler (JSON format)
    /// </summary>
    public string? Details { get; set; }
    
    /// <summary>
    /// Risk seviyesi (eğer güvenlik olayı ise)
    /// </summary>
    public SecurityRiskLevel? RiskLevel { get; set; }
    
    /// <summary>
    /// IP adresi
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User Agent
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Device bilgisi
    /// </summary>
    public string? DeviceInfo { get; set; }
    
    /// <summary>
    /// Konum bilgisi (latitude)
    /// </summary>
    public double? Latitude { get; set; }
    
    /// <summary>
    /// Konum bilgisi (longitude)
    /// </summary>
    public double? Longitude { get; set; }
    
    /// <summary>
    /// Session ID
    /// </summary>
    public string? SessionId { get; set; }
    
    /// <summary>
    /// Request ID (trace için)
    /// </summary>
    public string? RequestId { get; set; }
    
    /// <summary>
    /// Correlation ID (ilişkili işlemler için)
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Güncellenme tarihi
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Silinme tarihi (soft delete için)
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// Silindi mi?
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public virtual Payment? Payment { get; set; }
    public virtual Courier? Courier { get; set; }
    public virtual User? Customer { get; set; }
    public virtual User? Admin { get; set; }
}
