namespace Getir.Domain.Entities;

/// <summary>
/// Merchant çalışma saatleri
/// Her gün için ayrı kayıt
/// </summary>
public class WorkingHours
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan? OpenTime { get; set; } // Null = kapalı
    public TimeSpan? CloseTime { get; set; } // Null = kapalı
    public bool IsClosed { get; set; } // Gün kapalı mı?
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Merchant Merchant { get; set; } = default!;
}
