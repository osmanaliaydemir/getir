namespace Getir.Domain.Entities;

/// <summary>
/// Teslimat rotası bilgileri
/// Alternatif rota önerileri ve rota optimizasyonu için
/// </summary>
public class DeliveryRoute
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? CourierId { get; set; }
    
    // Rota bilgileri
    public string RouteName { get; set; } = default!; // "Primary", "Alternative1", "Fastest", etc.
    public string RouteType { get; set; } = default!; // "Primary", "Alternative", "Emergency", "Optimized"
    
    // Koordinatlar (JSON formatında saklanacak)
    public string Waypoints { get; set; } = default!; // JSON array of coordinates
    public string Polyline { get; set; } = default!; // Google Maps polyline string
    
    // Rota metrikleri
    public double TotalDistanceKm { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public int EstimatedTrafficDelayMinutes { get; set; } = 0;
    public decimal EstimatedFuelCost { get; set; } = 0;
    
    // Rota kalitesi
    public decimal RouteScore { get; set; } = 0; // 0-100 arası rota kalite skoru
    public bool HasTollRoads { get; set; } = false;
    public bool HasHighTrafficAreas { get; set; } = false;
    public bool IsHighwayPreferred { get; set; } = false;
    
    // Durum
    public bool IsSelected { get; set; } = false; // Bu rota seçildi mi
    public bool IsCompleted { get; set; } = false; // Rota tamamlandı mı
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Metadata
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = default!;
    public virtual Courier? Courier { get; set; }
}
