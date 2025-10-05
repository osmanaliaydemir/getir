using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Tracks all stock changes for audit and reporting purposes
/// </summary>
public class StockHistory
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int PreviousQuantity { get; set; }
    public int NewQuantity { get; set; }
    public int ChangeAmount { get; set; }
    public StockChangeType ChangeType { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public Guid? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public Guid? OrderId { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalValue { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = default!;
    public virtual MarketProductVariant? ProductVariant { get; set; }
    public virtual User? ChangedByUser { get; set; }
    public virtual Order? Order { get; set; }
}
