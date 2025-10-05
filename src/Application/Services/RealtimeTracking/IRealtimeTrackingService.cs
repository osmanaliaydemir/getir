using Getir.Application.DTO;

namespace Getir.Application.Services.RealtimeTracking;

public interface IRealtimeTrackingService
{
    Task<RealtimeTrackingData?> GetRealtimeDataAsync(Guid orderTrackingId);
    Task<List<RealtimeTrackingData>> GetActiveRealtimeDataAsync();
    Task<List<RealtimeTrackingData>> GetRealtimeDataByUserAsync(Guid userId);
    Task<List<RealtimeTrackingData>> GetRealtimeDataByCourierAsync(Guid courierId);
    Task<bool> StartTrackingAsync(Guid orderId, Guid? courierId = null);
    Task<bool> StopTrackingAsync(Guid orderTrackingId);
    Task<bool> PauseTrackingAsync(Guid orderTrackingId);
    Task<bool> ResumeTrackingAsync(Guid orderTrackingId);
    Task<bool> IsTrackingActiveAsync(Guid orderTrackingId);
    Task<Dictionary<string, object>> GetTrackingMetricsAsync(Guid orderTrackingId);
    Task<bool> ValidateLocationAsync(double latitude, double longitude);
    Task<double> CalculateDistanceToDestinationAsync(Guid orderTrackingId, double latitude, double longitude);
    Task<bool> IsNearDestinationAsync(Guid orderTrackingId, double latitude, double longitude, double thresholdMeters = 500);
}
