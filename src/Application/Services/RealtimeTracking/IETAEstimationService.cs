using Getir.Application.DTO;

namespace Getir.Application.Services.RealtimeTracking;

public interface IETAEstimationService
{
    Task<ETAEstimationDto?> GetCurrentETAAsync(Guid orderTrackingId);
    Task<ETAEstimationDto> CreateETAEstimationAsync(CreateETAEstimationRequest request);
    Task<ETAEstimationDto> UpdateETAEstimationAsync(Guid id, UpdateETAEstimationRequest request);
    Task<bool> DeleteETAEstimationAsync(Guid id);
    Task<List<ETAEstimationDto>> GetETAHistoryAsync(Guid orderTrackingId);
    Task<ETAEstimationDto> CalculateETAAsync(Guid orderTrackingId, double? currentLatitude = null, double? currentLongitude = null);
    Task<bool> ValidateETAAsync(Guid orderTrackingId, DateTime estimatedArrivalTime);
    Task<List<ETAEstimationDto>> GetActiveETAEstimationsAsync();
    Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2);
    Task<int> CalculateEstimatedMinutesAsync(double distanceKm, double? averageSpeed = null);
}
