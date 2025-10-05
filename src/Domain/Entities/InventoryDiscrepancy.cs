using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Represents inventory discrepancies found during counting
/// </summary>
public class InventoryDiscrepancy
{
    public Guid Id { get; set; }
    public Guid CountSessionId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int ExpectedQuantity { get; set; }
    public int ActualQuantity { get; set; }
    public int Variance { get; set; }
    public decimal VariancePercentage { get; set; }
    public InventoryDiscrepancyStatus Status { get; set; }
    public string? ResolutionNotes { get; set; }
    public Guid? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual InventoryCountSession CountSession { get; set; } = default!;
    public virtual Product Product { get; set; } = default!;
    public virtual MarketProductVariant? ProductVariant { get; set; }
    public virtual User? ResolvedByUser { get; set; }
}
