namespace Getir.Domain.Entities;

public class Courier
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string VehicleType { get; set; } = default!;
    public string? LicensePlate { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }
    public decimal? CurrentLatitude { get; set; }
    public decimal? CurrentLongitude { get; set; }
    public DateTime? LastLocationUpdate { get; set; }
    public int TotalDeliveries { get; set; }
    public decimal? Rating { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public virtual User User { get; set; } = default!;
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
