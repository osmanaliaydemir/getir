namespace Getir.Domain.Entities;

/// <summary>
/// Özel tatil günleri ve geçici kapanış/açılış durumları
/// Resmi tatiller, özel günler, tadilat vb. durumlar için kullanılır
/// </summary>
public class SpecialHoliday
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    
    /// <summary>
    /// Tatil/Kapalı olma nedeni (örn: "Ramazan Bayramı", "Tadilat", "Özel Etkinlik")
    /// </summary>
    public string Title { get; set; } = default!;
    
    /// <summary>
    /// Detaylı açıklama (opsiyonel)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Başlangıç tarihi ve saati
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Bitiş tarihi ve saati
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Kapalı mı yoksa özel açılış saatleri mi?
    /// true = Kapalı, false = Özel açılış saatleri var
    /// </summary>
    public bool IsClosed { get; set; }
    
    /// <summary>
    /// Özel açılış saati (IsClosed = false ise)
    /// </summary>
    public TimeSpan? SpecialOpenTime { get; set; }
    
    /// <summary>
    /// Özel kapanış saati (IsClosed = false ise)
    /// </summary>
    public TimeSpan? SpecialCloseTime { get; set; }
    
    /// <summary>
    /// Her yıl tekrar ediyor mu? (örn: Yılbaşı, Kurban Bayramı)
    /// </summary>
    public bool IsRecurring { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Merchant Merchant { get; set; } = default!;
}

