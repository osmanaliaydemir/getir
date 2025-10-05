namespace Getir.Domain.Enums;

public enum TrackingStatus
{
    OrderPlaced = 1,
    OrderConfirmed = 2,
    Preparing = 3,
    ReadyForPickup = 4,
    PickedUp = 5,
    OnTheWay = 6,
    NearDestination = 7,
    Arrived = 8,
    Delivered = 9,
    Cancelled = 10
}

public enum LocationUpdateType
{
    Manual = 1,
    GPS = 2,
    Network = 3,
    Estimated = 4
}

public enum NotificationType
{
    StatusUpdate = 1,
    LocationUpdate = 2,
    ETAUpdate = 3,
    DeliveryAlert = 4,
    DelayAlert = 5,
    CompletionAlert = 6
}

public static class TrackingStatusExtensions
{
    public static string GetDisplayName(this TrackingStatus status)
    {
        return status switch
        {
            TrackingStatus.OrderPlaced => "Sipariş Verildi",
            TrackingStatus.OrderConfirmed => "Sipariş Onaylandı",
            TrackingStatus.Preparing => "Hazırlanıyor",
            TrackingStatus.ReadyForPickup => "Teslim İçin Hazır",
            TrackingStatus.PickedUp => "Alındı",
            TrackingStatus.OnTheWay => "Yolda",
            TrackingStatus.NearDestination => "Yaklaşıyor",
            TrackingStatus.Arrived => "Vardı",
            TrackingStatus.Delivered => "Teslim Edildi",
            TrackingStatus.Cancelled => "İptal Edildi",
            _ => "Bilinmiyor"
        };
    }

    public static string GetDescription(this TrackingStatus status)
    {
        return status switch
        {
            TrackingStatus.OrderPlaced => "Siparişiniz alındı ve işleme konuldu",
            TrackingStatus.OrderConfirmed => "Siparişiniz onaylandı ve hazırlığa başlandı",
            TrackingStatus.Preparing => "Siparişiniz hazırlanıyor",
            TrackingStatus.ReadyForPickup => "Siparişiniz teslim için hazır",
            TrackingStatus.PickedUp => "Siparişiniz teslimatçı tarafından alındı",
            TrackingStatus.OnTheWay => "Siparişiniz size doğru yolda",
            TrackingStatus.NearDestination => "Siparişiniz yaklaşıyor",
            TrackingStatus.Arrived => "Siparişiniz adresinize vardı",
            TrackingStatus.Delivered => "Siparişiniz başarıyla teslim edildi",
            TrackingStatus.Cancelled => "Siparişiniz iptal edildi",
            _ => "Durum bilinmiyor"
        };
    }

    public static bool IsActive(this TrackingStatus status)
    {
        return status != TrackingStatus.Delivered && status != TrackingStatus.Cancelled;
    }

    public static bool CanTransitionTo(this TrackingStatus from, TrackingStatus to)
    {
        return (from, to) switch
        {
            (TrackingStatus.OrderPlaced, TrackingStatus.OrderConfirmed) => true,
            (TrackingStatus.OrderPlaced, TrackingStatus.Cancelled) => true,
            (TrackingStatus.OrderConfirmed, TrackingStatus.Preparing) => true,
            (TrackingStatus.OrderConfirmed, TrackingStatus.Cancelled) => true,
            (TrackingStatus.Preparing, TrackingStatus.ReadyForPickup) => true,
            (TrackingStatus.Preparing, TrackingStatus.Cancelled) => true,
            (TrackingStatus.ReadyForPickup, TrackingStatus.PickedUp) => true,
            (TrackingStatus.ReadyForPickup, TrackingStatus.Cancelled) => true,
            (TrackingStatus.PickedUp, TrackingStatus.OnTheWay) => true,
            (TrackingStatus.OnTheWay, TrackingStatus.NearDestination) => true,
            (TrackingStatus.OnTheWay, TrackingStatus.Arrived) => true,
            (TrackingStatus.NearDestination, TrackingStatus.Arrived) => true,
            (TrackingStatus.Arrived, TrackingStatus.Delivered) => true,
            _ => false
        };
    }
}

public static class LocationUpdateTypeExtensions
{
    public static string GetDisplayName(this LocationUpdateType type)
    {
        return type switch
        {
            LocationUpdateType.Manual => "Manuel",
            LocationUpdateType.GPS => "GPS",
            LocationUpdateType.Network => "Ağ",
            LocationUpdateType.Estimated => "Tahmini",
            _ => "Bilinmiyor"
        };
    }

    public static int GetAccuracyLevel(this LocationUpdateType type)
    {
        return type switch
        {
            LocationUpdateType.GPS => 1, // En yüksek doğruluk
            LocationUpdateType.Network => 2,
            LocationUpdateType.Manual => 3,
            LocationUpdateType.Estimated => 4, // En düşük doğruluk
            _ => 5
        };
    }
}

public static class NotificationTypeExtensions
{
    public static string GetDisplayName(this NotificationType type)
    {
        return type switch
        {
            NotificationType.StatusUpdate => "Durum Güncellemesi",
            NotificationType.LocationUpdate => "Konum Güncellemesi",
            NotificationType.ETAUpdate => "Tahmini Varış Süresi",
            NotificationType.DeliveryAlert => "Teslimat Uyarısı",
            NotificationType.DelayAlert => "Gecikme Uyarısı",
            NotificationType.CompletionAlert => "Tamamlanma Bildirimi",
            _ => "Bilinmiyor"
        };
    }

    public static bool IsUrgent(this NotificationType type)
    {
        return type switch
        {
            NotificationType.DelayAlert => true,
            NotificationType.DeliveryAlert => true,
            NotificationType.CompletionAlert => true,
            _ => false
        };
    }
}
