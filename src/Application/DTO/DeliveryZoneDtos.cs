namespace Getir.Application.DTO;

public record CreateDeliveryZoneRequest(
    Guid MerchantId,
    string Name,
    string? Description,
    decimal DeliveryFee,
    int EstimatedDeliveryTime,
    List<DeliveryZonePointRequest> Points);

public record UpdateDeliveryZoneRequest(
    string Name,
    string? Description,
    decimal DeliveryFee,
    int EstimatedDeliveryTime,
    bool IsActive,
    List<DeliveryZonePointRequest> Points);

public record DeliveryZoneResponse(
    Guid Id,
    Guid MerchantId,
    string Name,
    string? Description,
    decimal DeliveryFee,
    int EstimatedDeliveryTime,
    bool IsActive,
    List<DeliveryZonePointResponse> Points,
    DateTime CreatedAt);

public record DeliveryZonePointRequest(
    decimal Latitude,
    decimal Longitude,
    int Order);

public record DeliveryZonePointResponse(
    Guid Id,
    Guid DeliveryZoneId,
    decimal Latitude,
    decimal Longitude,
    int Order);

public record CheckDeliveryZoneRequest(
    decimal Latitude,
    decimal Longitude);

public record CheckDeliveryZoneResponse(
    bool IsInZone,
    Guid? DeliveryZoneId,
    decimal? DeliveryFee,
    int? EstimatedDeliveryTime);
