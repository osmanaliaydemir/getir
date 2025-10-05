namespace Getir.Domain.Entities;

/// <summary>
/// Delivery zone polygon noktaları
/// Teslimat bölgelerinin polygon koordinatlarını temsil eder
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
