namespace Getir.Domain.Entities;

/// <summary>
/// Represents an item counted during inventory count
/// </summary>
public class InventoryCountItem
{
    public Guid Id { get; set; }
    public Guid CountSessionId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int ExpectedQuantity { get; set; }
    public int CountedQuantity { get; set; }
    public int Variance { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual InventoryCountSession CountSession { get; set; } = default!;
    public virtual Product Product { get; set; } = default!;
    public virtual MarketProductVariant? ProductVariant { get; set; }
}
