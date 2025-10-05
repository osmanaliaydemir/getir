using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Represents a stock synchronization session
/// </summary>
public class StockSyncSession
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string ExternalSystemId { get; set; } = default!;
    public StockSyncType SyncType { get; set; }
    public StockSyncStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int SyncedItemsCount { get; set; }
    public int FailedItemsCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Merchant Merchant { get; set; } = default!;
    public virtual ICollection<StockSyncDetail> SyncDetails { get; set; } = new List<StockSyncDetail>();
}
