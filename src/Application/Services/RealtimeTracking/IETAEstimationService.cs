using Getir.Application.DTO;

namespace Getir.Application.Services.RealtimeTracking;

/// <summary>
/// ETA tahmini servisi: tahmini varış süresi hesaplama, geçmiş, mesafe/süre hesaplama (Haversine).
/// </summary>
public interface IETAEstimationService
{
    /// <summary>Sipariş takibi için güncel ETA tahminini getirir.</summary>
    Task<ETAEstimationDto?> GetCurrentETAAsync(Guid orderTrackingId);
    /// <summary>Yeni ETA tahmini oluşturur.</summary>
    Task<ETAEstimationDto> CreateETAEstimationAsync(CreateETAEstimationRequest request);
    /// <summary>ETA tahminini günceller.</summary>
    Task<ETAEstimationDto> UpdateETAEstimationAsync(Guid id, UpdateETAEstimationRequest request);
    /// <summary>ETA tahminini siler (soft delete).</summary>
    Task<bool> DeleteETAEstimationAsync(Guid id);
    /// <summary>ETA tahmin geçmişini getirir (zaman sıralı).</summary>
    Task<List<ETAEstimationDto>> GetETAHistoryAsync(Guid orderTrackingId);
    /// <summary>ETA hesaplar (mevcut konum, mesafe, ortalama hız bazlı).</summary>
    Task<ETAEstimationDto> CalculateETAAsync(Guid orderTrackingId, double? currentLatitude = null, double? currentLongitude = null);
    /// <summary>ETA'nın makul olup olmadığını doğrular (5 dk - 2 saat arası).</summary>
    Task<bool> ValidateETAAsync(Guid orderTrackingId, DateTime estimatedArrivalTime);
    /// <summary>Aktif ETA tahminlerini getirir.</summary>
    Task<List<ETAEstimationDto>> GetActiveETAEstimationsAsync();
    /// <summary>İki koordinat arası mesafeyi hesaplar (Haversine formula).</summary>
    Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2);
    /// <summary>Tahmini dakika hesaplar (mesafe ve ortalama hız bazlı).</summary>
    Task<int> CalculateEstimatedMinutesAsync(double distanceKm, double? averageSpeed = null);
}
