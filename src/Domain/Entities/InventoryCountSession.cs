using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Represents an inventory count session
/// </summary>
public class InventoryCountSession
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public DateTime CountDate { get; set; }
    public InventoryCountType CountType { get; set; }
    public InventoryCountStatus Status { get; set; }
    public int DiscrepancyCount { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Merchant Merchant { get; set; } = default!;
    public virtual User CreatedByUser { get; set; } = default!;
    public virtual ICollection<InventoryCountItem> CountItems { get; set; } = new List<InventoryCountItem>();
    public virtual ICollection<InventoryDiscrepancy> Discrepancies { get; set; } = new List<InventoryDiscrepancy>();
}
