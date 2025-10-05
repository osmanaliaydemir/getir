using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Stock alerts for low stock, overstock, etc.
/// </summary>
public class StockAlert
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public Guid MerchantId { get; set; }
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public StockAlertType AlertType { get; set; }
    public string Message { get; set; } = default!;
    public bool IsResolved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Product Product { get; set; } = default!;
    public virtual MarketProductVariant? ProductVariant { get; set; }
    public virtual Merchant Merchant { get; set; } = default!;
    public virtual User? ResolvedByUser { get; set; }
}
