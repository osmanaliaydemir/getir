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
    string? Notes);

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
    decimal TotalPrice);
