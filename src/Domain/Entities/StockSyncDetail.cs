using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Represents details of a stock synchronization item
/// </summary>
public class StockSyncDetail
{
    public Guid Id { get; set; }
    public Guid SyncSessionId { get; set; }
    public Guid ProductId { get; set; }
    public string ExternalProductId { get; set; } = default!;
    public string? ExternalVariantId { get; set; }
    public int PreviousQuantity { get; set; }
    public int NewQuantity { get; set; }
    public int QuantityDifference { get; set; }
    public StockSyncDetailStatus SyncStatus { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual StockSyncSession SyncSession { get; set; } = default!;
    public virtual Product Product { get; set; } = default!;
}
