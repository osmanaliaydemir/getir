using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.RealtimeTracking;

/// <summary>
/// Sipariş takip servisi: konum/durum güncelleme, geçmiş, arama, istatistikler, durum geçiş kontrolü.
/// </summary>
public interface IOrderTrackingService
{
    /// <summary>Sipariş ID'sine göre tracking kaydını getirir.</summary>
    Task<OrderTrackingDto?> GetTrackingByOrderIdAsync(Guid orderId);
    /// <summary>Tracking ID'sine göre kaydı getirir.</summary>
    Task<OrderTrackingDto?> GetTrackingByIdAsync(Guid trackingId);
    /// <summary>Yeni tracking kaydı oluşturur.</summary>
    Task<OrderTrackingDto> CreateTrackingAsync(Guid orderId, Guid? courierId = null);
    /// <summary>Konum günceller (mesafe/ETA hesaplama, konum geçmişi).</summary>
    Task<LocationUpdateResponse> UpdateLocationAsync(LocationUpdateRequest request);
    /// <summary>Durum günceller (validasyon, bildirim).</summary>
    Task<StatusUpdateResponse> UpdateStatusAsync(StatusUpdateRequest request);
    /// <summary>Tracking kaydını siler (soft delete).</summary>
    Task<bool> DeleteTrackingAsync(Guid trackingId);
    /// <summary>Aktif tracking kayıtlarını getirir.</summary>
    Task<List<OrderTrackingDto>> GetActiveTrackingsAsync();
    /// <summary>Kurye bazlı tracking kayıtlarını getirir.</summary>
    Task<List<OrderTrackingDto>> GetTrackingsByCourierAsync(Guid courierId);
    /// <summary>Kullanıcı bazlı tracking kayıtlarını getirir.</summary>
    Task<List<OrderTrackingDto>> GetTrackingsByUserAsync(Guid userId);
    /// <summary>Tracking kayıtlarını arar (filtreleme ve sayfalama).</summary>
    Task<TrackingSearchResponse> SearchTrackingsAsync(TrackingSearchRequest request);
    /// <summary>Konum geçmişini getirir (son N kayıt).</summary>
    Task<List<LocationHistoryDto>> GetLocationHistoryAsync(Guid trackingId, int count = 50);
    /// <summary>Tracking istatistiklerini getirir.</summary>
    Task<TrackingStatisticsDto> GetTrackingStatisticsAsync(DateTime startDate, DateTime endDate);
    /// <summary>Durum geçişinin geçerli olup olmadığını kontrol eder.</summary>
    Task<bool> CanTransitionToStatusAsync(Guid trackingId, TrackingStatus newStatus);
    /// <summary>Mevcut geçerli durum geçişlerini getirir.</summary>
    Task<List<TrackingStatus>> GetAvailableTransitionsAsync(Guid trackingId);
}
