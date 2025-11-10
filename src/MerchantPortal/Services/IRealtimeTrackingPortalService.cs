using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public interface IRealtimeTrackingPortalService
{
    Task<List<OrderTrackingResponse>> GetActiveTrackingsAsync(CancellationToken ct = default);
    Task<OrderTrackingResponse?> GetTrackingByIdAsync(Guid trackingId, CancellationToken ct = default);
    Task<OrderTrackingResponse?> GetTrackingByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(StatusUpdateRequestModel request, CancellationToken ct = default);
    Task<bool> UpdateLocationAsync(LocationUpdateRequestModel request, CancellationToken ct = default);
    Task<List<TrackingNotificationResponse>> GetNotificationsAsync(Guid trackingId, CancellationToken ct = default);
}

