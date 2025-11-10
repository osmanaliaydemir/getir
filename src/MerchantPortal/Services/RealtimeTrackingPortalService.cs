using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public class RealtimeTrackingPortalService : IRealtimeTrackingPortalService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<RealtimeTrackingPortalService> _logger;

    public RealtimeTrackingPortalService(IApiClient apiClient, ILogger<RealtimeTrackingPortalService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<List<OrderTrackingResponse>> GetActiveTrackingsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<List<OrderTrackingResponse>>>("api/realtimetracking/active", ct);
            return response?.Data ?? new List<OrderTrackingResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch active trackings");
            return new List<OrderTrackingResponse>();
        }
    }

    public async Task<OrderTrackingResponse?> GetTrackingByIdAsync(Guid trackingId, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<OrderTrackingResponse>>($"api/realtimetracking/{trackingId}", ct);
            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch tracking by id {TrackingId}", trackingId);
            return null;
        }
    }

    public async Task<OrderTrackingResponse?> GetTrackingByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<OrderTrackingResponse>>($"api/realtimetracking/order/{orderId}", ct);
            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch tracking by order {OrderId}", orderId);
            return null;
        }
    }

    public async Task<bool> UpdateStatusAsync(StatusUpdateRequestModel request, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.PostAsync<ApiResponse<StatusUpdateResponse>>("api/realtimetracking/status/update", request, ct);
            return response?.isSuccess == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tracking status for {TrackingId}", request.OrderTrackingId);
            return false;
        }
    }

    public async Task<bool> UpdateLocationAsync(LocationUpdateRequestModel request, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.PostAsync<ApiResponse<LocationUpdateResponse>>("api/realtimetracking/location/update", request, ct);
            return response?.isSuccess == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tracking location for {TrackingId}", request.OrderTrackingId);
            return false;
        }
    }

    public async Task<List<TrackingNotificationResponse>> GetNotificationsAsync(Guid trackingId, CancellationToken ct = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<ApiResponse<List<TrackingNotificationResponse>>>($"api/realtimetracking/{trackingId}/notifications", ct);
            return response?.Data ?? new List<TrackingNotificationResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch tracking notifications for {TrackingId}", trackingId);
            return new List<TrackingNotificationResponse>();
        }
    }
}

