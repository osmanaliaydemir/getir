namespace Getir.MerchantPortal.Models;

public class RealtimeTrackingViewModel
{
    public List<OrderTrackingResponse> ActiveTrackings { get; set; } = new();
    public OrderTrackingResponse? SelectedTracking { get; set; }
    public List<TrackingNotificationResponse> Notifications { get; set; } = new();
}

