namespace Getir.Domain.Entities;

/// <summary>
/// Kurye nakit para toplama kayıtları (sadece Cash payment için)
/// </summary>
public class CourierCashCollection
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public Guid CourierId { get; set; }
    
    /// <summary>
    /// Toplanan para miktarı
    /// </summary>
    public decimal CollectedAmount { get; set; }
    
    /// <summary>
    /// Para toplama tarihi
    /// </summary>
    public DateTime CollectedAt { get; set; }
    
    /// <summary>
    /// Para toplama notları
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Para durumu (Collected, HandedToMerchant, HandedToCompany)
    /// </summary>
    public string Status { get; set; } = "Collected";
    
    /// <summary>
    /// Merchant'a teslim tarihi
    /// </summary>
    public DateTime? HandedToMerchantAt { get; set; }
    
    /// <summary>
    /// Şirkete teslim tarihi
    /// </summary>
    public DateTime? HandedToCompanyAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Payment Payment { get; set; } = default!;
    public virtual Courier Courier { get; set; } = default!;
}
