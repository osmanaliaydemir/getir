namespace Getir.Domain.Entities;

/// <summary>
/// Stock management settings for merchants
/// </summary>
public class StockSettings
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public bool AutoStockReduction { get; set; } = true;
    public bool LowStockAlerts { get; set; } = true;
    public bool OverstockAlerts { get; set; } = false;
    public int DefaultMinimumStock { get; set; } = 10;
    public int DefaultMaximumStock { get; set; } = 1000;
    public bool EnableStockSync { get; set; } = false;
    public string? ExternalSystemId { get; set; }
    public string? SyncApiKey { get; set; }
    public string? SyncApiUrl { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public int SyncIntervalMinutes { get; set; } = 60;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Merchant Merchant { get; set; } = default!;
}
