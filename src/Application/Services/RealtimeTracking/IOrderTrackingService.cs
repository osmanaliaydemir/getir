using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.RealtimeTracking;

public interface IOrderTrackingService
{
    Task<OrderTrackingDto?> GetTrackingByOrderIdAsync(Guid orderId);
    Task<OrderTrackingDto?> GetTrackingByIdAsync(Guid trackingId);
    Task<OrderTrackingDto> CreateTrackingAsync(Guid orderId, Guid? courierId = null);
    Task<LocationUpdateResponse> UpdateLocationAsync(LocationUpdateRequest request);
    Task<StatusUpdateResponse> UpdateStatusAsync(StatusUpdateRequest request);
    Task<bool> DeleteTrackingAsync(Guid trackingId);
    Task<List<OrderTrackingDto>> GetActiveTrackingsAsync();
    Task<List<OrderTrackingDto>> GetTrackingsByCourierAsync(Guid courierId);
    Task<List<OrderTrackingDto>> GetTrackingsByUserAsync(Guid userId);
    Task<TrackingSearchResponse> SearchTrackingsAsync(TrackingSearchRequest request);
    Task<List<LocationHistoryDto>> GetLocationHistoryAsync(Guid trackingId, int count = 50);
    Task<TrackingStatisticsDto> GetTrackingStatisticsAsync(DateTime startDate, DateTime endDate);
    Task<bool> CanTransitionToStatusAsync(Guid trackingId, TrackingStatus newStatus);
    Task<List<TrackingStatus>> GetAvailableTransitionsAsync(Guid trackingId);
}
