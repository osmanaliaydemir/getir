namespace Getir.Application.DTO;

public record CreateAddressRequest(
    string Title,
    string FullAddress,
    string City,
    string District,
    decimal Latitude,
    decimal Longitude);

public record UpdateAddressRequest(
    string Title,
    string FullAddress,
    string City,
    string District,
    decimal Latitude,
    decimal Longitude);

public record AddressResponse(
    Guid Id,
    string Title,
    string FullAddress,
    string City,
    string District,
    decimal Latitude,
    decimal Longitude,
    bool IsDefault,
    DateTime CreatedAt);

public record UserAddressResponse(
    Guid Id,
    string Title,
    string FullAddress,
    string City,
    string District,
    decimal Latitude,
    decimal Longitude,
    bool IsDefault,
    DateTime CreatedAt);