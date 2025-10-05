namespace Getir.Domain.Entities;

/// <summary>
/// Merchant teslimat bölgeleri
/// Polygon koordinatları ile teslimat alanını tanımlar
/// </summary>
public class DeliveryZone
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal DeliveryFee { get; set; }
    public int EstimatedDeliveryTime { get; set; } // Dakika cinsinden
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Merchant Merchant { get; set; } = default!;
    public virtual ICollection<DeliveryZonePoint> Points { get; set; } = new List<DeliveryZonePoint>();
}

