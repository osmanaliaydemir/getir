using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Addresses;

public interface IUserAddressService
{
    Task<Result<List<AddressResponse>>> GetUserAddressesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<Result<AddressResponse>> AddAddressAsync(
        Guid userId,
        CreateAddressRequest request,
        CancellationToken cancellationToken = default);
    
    Task<Result<AddressResponse>> UpdateAddressAsync(
        Guid userId,
        Guid addressId,
        UpdateAddressRequest request,
        CancellationToken cancellationToken = default);
    
    Task<Result> DeleteAddressAsync(
        Guid userId,
        Guid addressId,
        CancellationToken cancellationToken = default);
    
    Task<Result> SetDefaultAddressAsync(
        Guid userId,
        Guid addressId,
        CancellationToken cancellationToken = default);
}
