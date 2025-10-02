namespace Getir.Domain.Entities;

/// <summary>
/// Merchant nakit ödeme settlement kayıtları
/// </summary>
public class CashSettlement
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    
    /// <summary>
    /// Toplam tutar
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Komisyon tutarı
    /// </summary>
    public decimal Commission { get; set; }
    
    /// <summary>
    /// Net ödeme tutarı (TotalAmount - Commission)
    /// </summary>
    public decimal NetAmount { get; set; }
    
    /// <summary>
    /// Settlement tarihi
    /// </summary>
    public DateTime SettlementDate { get; set; }
    
    /// <summary>
    /// Settlement durumu (Pending, Completed, Failed)
    /// </summary>
    public string Status { get; set; } = "Pending";
    
    /// <summary>
    /// Settlement notları
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// İşlem yapan admin ID
    /// </summary>
    public Guid? ProcessedByAdminId { get; set; }
    
    /// <summary>
    /// İşlem tamamlanma tarihi
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Banka transfer referans numarası
    /// </summary>
    public string? BankTransferReference { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Merchant Merchant { get; set; } = default!;
    public virtual User? ProcessedByAdmin { get; set; }
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
