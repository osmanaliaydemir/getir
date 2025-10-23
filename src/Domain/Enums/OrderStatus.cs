namespace Getir.Domain.Enums;

/// <summary>
/// Sipariş durumları
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Beklemede
    /// </summary>
    Pending = 0,
    /// <summary>
    /// Onaylandı
    /// </summary>
    Confirmed = 1,
    /// <summary>
    /// Hazırlanıyor
    /// </summary>
    Preparing = 2,
    /// <summary>
    /// Hazır
    /// </summary>
    Ready = 3,
    /// <summary>
    /// Teslim Edildi
    /// </summary>
    PickedUp = 4,
    /// <summary>
    /// Yolda
    /// </summary>
    OnTheWay = 5,
    /// <summary>
    /// Teslim Edildi
    /// </summary>
    Delivered = 6,
    /// <summary>
    /// İptal Edildi
    /// </summary>
    Cancelled = 7
}

public static class OrderStatusExtensions
{
    /// <summary>
    /// Sipariş durumunun string değerini döndürür
    /// </summary>
    /// <param name="status">Sipariş durumu</param>
    /// <returns>Sipariş durumunun string değeri</returns>
    public static string ToStringValue(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Pending",
            OrderStatus.Confirmed => "Confirmed",
            OrderStatus.Preparing => "Preparing",
            OrderStatus.Ready => "Ready",
            OrderStatus.PickedUp => "PickedUp",
            OrderStatus.OnTheWay => "OnTheWay",
            OrderStatus.Delivered => "Delivered",
            OrderStatus.Cancelled => "Cancelled",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    /// <summary>
    /// Sipariş durumunun string değerinden döndürür
    /// </summary>
    /// <param name="status">Sipariş durumunun string değeri</param>
    /// <returns>Sipariş durumu</returns>
    public static OrderStatus FromString(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "pending" => OrderStatus.Pending,
            "confirmed" => OrderStatus.Confirmed,
            "preparing" => OrderStatus.Preparing,
            "ready" => OrderStatus.Ready,
            "pickedup" => OrderStatus.PickedUp,
            "ontheway" => OrderStatus.OnTheWay,
            "delivered" => OrderStatus.Delivered,
            "cancelled" => OrderStatus.Cancelled,
            _ => throw new ArgumentException($"Invalid order status: {status}", nameof(status))
        };
    }

    /// <summary>
    /// Sipariş durumunun bir sonraki duruma geçebilip geçemeyeceğini döndürür
    /// </summary>
    /// <param name="from">Önceki sipariş durumu</param>
    /// <param name="to">Sonraki sipariş durumu</param>
    /// <returns>Sipariş durumunun bir sonraki duruma geçebilip geçemeyeceği</returns>
    public static bool CanTransitionTo(this OrderStatus from, OrderStatus to)
    {
        return (from, to) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Preparing) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            (OrderStatus.Preparing, OrderStatus.Ready) => true,
            (OrderStatus.Preparing, OrderStatus.Cancelled) => true,
            (OrderStatus.Ready, OrderStatus.PickedUp) => true,
            (OrderStatus.Ready, OrderStatus.Cancelled) => true,
            (OrderStatus.PickedUp, OrderStatus.OnTheWay) => true,
            (OrderStatus.PickedUp, OrderStatus.Cancelled) => true,
            (OrderStatus.OnTheWay, OrderStatus.Delivered) => true,
            (OrderStatus.OnTheWay, OrderStatus.Cancelled) => true,
            _ => false
        };
    }
}
