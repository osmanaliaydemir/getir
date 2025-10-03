namespace Getir.Application.DTO;

public record CreateOrderRequest(
    Guid MerchantId,
    List<OrderLineRequest> Items,
    string DeliveryAddress,
    decimal DeliveryLatitude,
    decimal DeliveryLongitude,
    string PaymentMethod,
    string? Notes);

public record OrderLineRequest(
    Guid ProductId,
    int Quantity,
    string? Notes,
    List<CreateOrderLineOptionRequest>? Options = null);

public record OrderResponse(
    Guid Id,
    string OrderNumber,
    Guid MerchantId,
    string MerchantName,
    string Status,
    decimal SubTotal,
    decimal DeliveryFee,
    decimal Discount,
    decimal Total,
    string PaymentMethod,
    string PaymentStatus,
    string DeliveryAddress,
    DateTime? EstimatedDeliveryTime,
    DateTime CreatedAt,
    List<OrderLineResponse> Items);

public record OrderLineResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    List<OrderLineOptionResponse> Options);

// Merchant-specific DTOs
public record RejectOrderRequest(
    Guid OrderId,
    string? Reason = null);

public record CancelOrderRequest(
    Guid OrderId,
    string Reason);

public record OrderStatisticsResponse(
    int TotalOrders,
    int PendingOrders,
    int ConfirmedOrders,
    int PreparingOrders,
    int ReadyOrders,
    int DeliveredOrders,
    int CancelledOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    int TodayOrders,
    int ThisWeekOrders,
    int ThisMonthOrders,
    DateTime GeneratedAt);

public record UpdateOrderStatusRequest(
    Guid OrderId,
    string NewStatus,
    string? Reason = null);

public record OrderDetailsResponse(
    Guid Id,
    string OrderNumber,
    Guid MerchantId,
    string MerchantName,
    Guid CustomerId,
    string CustomerName,
    string Status,
    decimal SubTotal,
    decimal DeliveryFee,
    decimal Discount,
    decimal Total,
    string PaymentMethod,
    string PaymentStatus,
    string DeliveryAddress,
    DateTime? EstimatedDeliveryTime,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    List<OrderLineResponse> Items,
    List<OrderTimelineResponse> Timeline);

public record OrderAnalyticsResponse(
    int TotalOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    Dictionary<string, int> OrdersByStatus,
    Dictionary<string, decimal> RevenueByDay,
    DateTime GeneratedAt);

public record OrderTimelineResponse(
    DateTime Timestamp,
    string Status,
    string Description,
    string? ActorName);