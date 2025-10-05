namespace Getir.Domain.Entities;

/// <summary>
/// Teslimat kapasitesi yönetimi
/// Merchant'ların teslimat kapasitelerini ve kısıtlamalarını yönetir
/// </summary>
public class DeliveryCapacity
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public Guid? DeliveryZoneId { get; set; }
    
    // Kapasite ayarları
    public int MaxConcurrentDeliveries { get; set; } = 10; // Aynı anda maksimum teslimat sayısı
    public int MaxDailyDeliveries { get; set; } = 100; // Günlük maksimum teslimat sayısı
    public int MaxWeeklyDeliveries { get; set; } = 500; // Haftalık maksimum teslimat sayısı
    
    // Zaman kısıtlamaları
    public TimeSpan? PeakStartTime { get; set; } // Yoğun saat başlangıcı
    public TimeSpan? PeakEndTime { get; set; } // Yoğun saat bitişi
    public int PeakHourCapacityReduction { get; set; } = 0; // Yoğun saatlerde kapasite azaltma yüzdesi
    
    // Mesafe kısıtlamaları
    public double? MaxDeliveryDistanceKm { get; set; } // Maksimum teslimat mesafesi
    public decimal? DistanceBasedFeeMultiplier { get; set; } = 1.0m; // Mesafeye göre ücret çarpanı
    
    // Dinamik kapasite ayarları
    public bool IsDynamicCapacityEnabled { get; set; } = true; // Dinamik kapasite aktif mi
    public int CurrentActiveDeliveries { get; set; } = 0; // Şu anki aktif teslimat sayısı
    public int TodayDeliveryCount { get; set; } = 0; // Bugünkü teslimat sayısı
    public int ThisWeekDeliveryCount { get; set; } = 0; // Bu haftaki teslimat sayısı
    
    // Durum
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastResetDate { get; set; } // Son sıfırlama tarihi

    // Navigation properties
    public virtual Merchant Merchant { get; set; } = default!;
    public virtual DeliveryZone? DeliveryZone { get; set; }
}
