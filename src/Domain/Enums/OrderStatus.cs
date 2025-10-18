namespace Getir.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Preparing = 2,
    Ready = 3,
    PickedUp = 4,
    OnTheWay = 5,
    Delivered = 6,
    Cancelled = 7
}

public static class OrderStatusExtensions
{
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
