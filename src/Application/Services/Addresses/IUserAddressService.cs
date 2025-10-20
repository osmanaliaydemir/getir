using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Addresses;

/// <summary>
/// Kullanıcı adres servisi interface'i: adres listeleme, ekleme, güncelleme, silme ve varsayılan ayarlama işlemleri.
/// </summary>
public interface IUserAddressService
{
    /// <summary>Kullanıcının adreslerini getirir.</summary>
    Task<Result<List<AddressResponse>>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Kullanıcıya adres ekler.</summary>
    Task<Result<AddressResponse>> AddAddressAsync(Guid userId, CreateAddressRequest request, CancellationToken cancellationToken = default);

    /// <summary>Kullanıcının adresini günceller.</summary>
    Task<Result<AddressResponse>> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressRequest request, CancellationToken cancellationToken = default);

    /// <summary>Kullanıcının adresini siler.</summary>
    Task<Result> DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);

    /// <summary>Kullanıcının varsayılan adresini ayarlar.</summary>
    Task<Result> SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
}
