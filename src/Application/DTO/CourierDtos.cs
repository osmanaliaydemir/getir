namespace Getir.Application.DTO;

public record CourierLocationUpdateRequest(
    decimal Latitude,
    decimal Longitude);

public record SetAvailabilityRequest(
    bool IsAvailable);

public record CourierOrderResponse(
    Guid Id,
    string OrderNumber,
    string Status,
    string DeliveryAddress,
    decimal DeliveryLatitude,
    decimal DeliveryLongitude,
    decimal Total,
    DateTime? EstimatedDeliveryTime);

public record CourierResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string VehicleType,
    bool IsAvailable,
    decimal? CurrentLatitude,
    decimal? CurrentLongitude,
    int TotalDeliveries,
    decimal? Rating);
