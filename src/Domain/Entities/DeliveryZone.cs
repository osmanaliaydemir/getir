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

/// <summary>
/// Delivery zone polygon noktaları
/// </summary>
public class DeliveryZonePoint
{
    public Guid Id { get; set; }
    public Guid DeliveryZoneId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int Order { get; set; } // Polygon noktalarının sırası
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual DeliveryZone DeliveryZone { get; set; } = default!;
}
