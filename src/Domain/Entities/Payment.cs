using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Ölçeklenebilir Payment entity - tüm ödeme yöntemlerini destekler
/// Şu anda sadece Cash payment aktif, diğerleri gelecekte eklenecek
/// </summary>
public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// Ödeme yöntemi (Cash, CreditCard, VodafonePay, vb.)
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    
    /// <summary>
    /// Ödeme durumu
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    
    /// <summary>
    /// Ödeme tutarı
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Para üstü (sadece Cash için)
    /// </summary>
    public decimal? ChangeAmount { get; set; }
    
    /// <summary>
    /// Ödeme işlem tarihi
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// Ödeme tamamlanma tarihi
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Kurye tarafından para toplanma tarihi (sadece Cash için)
    /// </summary>
    public DateTime? CollectedAt { get; set; }
    
    /// <summary>
    /// Merchant'a ödeme aktarım tarihi
    /// </summary>
    public DateTime? SettledAt { get; set; }
    
    /// <summary>
    /// Para toplayan kurye ID (sadece Cash için)
    /// </summary>
    public Guid? CollectedByCourierId { get; set; }
    
    /// <summary>
    /// Ödeme notları
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Hata sebebi (başarısız ödemeler için)
    /// </summary>
    public string? FailureReason { get; set; }
    
    /// <summary>
    /// External payment provider transaction ID (CreditCard, VodafonePay vb. için)
    /// </summary>
    public string? ExternalTransactionId { get; set; }
    
    /// <summary>
    /// External payment provider response (JSON format)
    /// </summary>
    public string? ExternalResponse { get; set; }
    
    /// <summary>
    /// İade tutarı (refund için)
    /// </summary>
    public decimal? RefundAmount { get; set; }
    
    /// <summary>
    /// İade tarihi
    /// </summary>
    public DateTime? RefundedAt { get; set; }
    
    /// <summary>
    /// İade sebebi
    /// </summary>
    public string? RefundReason { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Order Order { get; set; } = default!;
    public virtual Courier? CollectedByCourier { get; set; }
    public virtual ICollection<CourierCashCollection> CourierCashCollections { get; set; } = new List<CourierCashCollection>();
    public virtual ICollection<CashSettlement> CashSettlements { get; set; } = new List<CashSettlement>();
}
