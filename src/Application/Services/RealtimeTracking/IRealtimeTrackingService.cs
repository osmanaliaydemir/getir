using Getir.Application.DTO;

namespace Getir.Application.Services.RealtimeTracking;

/// <summary>
/// Gerçek zamanlı takip servisi: aktif tracking verileri, start/stop/pause, metrikler, mesafe hesaplama.
/// </summary>
public interface IRealtimeTrackingService
{
    /// <summary>Gerçek zamanlı tracking verisini getirir.</summary>
    Task<RealtimeTrackingData?> GetRealtimeDataAsync(Guid orderTrackingId);
    /// <summary>Aktif gerçek zamanlı tracking verilerini getirir.</summary>
    Task<List<RealtimeTrackingData>> GetActiveRealtimeDataAsync();
    /// <summary>Kullanıcı bazlı gerçek zamanlı verileri getirir.</summary>
    Task<List<RealtimeTrackingData>> GetRealtimeDataByUserAsync(Guid userId);
    /// <summary>Kurye bazlı gerçek zamanlı verileri getirir.</summary>
    Task<List<RealtimeTrackingData>> GetRealtimeDataByCourierAsync(Guid courierId);
    /// <summary>Tracking başlatır (duplicate kontrolü).</summary>
    Task<bool> StartTrackingAsync(Guid orderId, Guid? courierId = null);
    /// <summary>Tracking durdurur (soft delete).</summary>
    Task<bool> StopTrackingAsync(Guid orderTrackingId);
    /// <summary>Tracking duraklatır.</summary>
    Task<bool> PauseTrackingAsync(Guid orderTrackingId);
    /// <summary>Tracking devam ettirir.</summary>
    Task<bool> ResumeTrackingAsync(Guid orderTrackingId);
    /// <summary>Tracking aktif mi kontrol eder.</summary>
    Task<bool> IsTrackingActiveAsync(Guid orderTrackingId);
    /// <summary>Tracking metriklerini getirir (hız, bearing, accuracy, süre).</summary>
    Task<Dictionary<string, object>> GetTrackingMetricsAsync(Guid orderTrackingId);
    /// <summary>Koordinatların geçerli olup olmadığını kontrol eder (Türkiye sınırları).</summary>
    Task<bool> ValidateLocationAsync(double latitude, double longitude);
    /// <summary>Hedefe olan mesafeyi hesaplar (Haversine).</summary>
    Task<double> CalculateDistanceToDestinationAsync(Guid orderTrackingId, double latitude, double longitude);
    /// <summary>Hedefe yakın olup olmadığını kontrol eder (eşik metre).</summary>
    Task<bool> IsNearDestinationAsync(Guid orderTrackingId, double latitude, double longitude, double thresholdMeters = 500);
}
