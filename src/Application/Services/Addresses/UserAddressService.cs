using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.Addresses;

public class UserAddressService : IUserAddressService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserAddressService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<AddressResponse>>> GetUserAddressesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var addresses = await _unitOfWork.ReadRepository<UserAddress>()
            .ListAsync(
                filter: a => a.UserId == userId && a.IsActive,
                orderBy: a => a.IsDefault,
                ascending: false,
                cancellationToken: cancellationToken);

        var response = addresses.Select(a => new AddressResponse(
            a.Id,
            a.Title,
            a.FullAddress,
            a.City,
            a.District,
            a.Latitude,
            a.Longitude,
            a.IsDefault,
            a.CreatedAt
        )).ToList();

        return Result.Ok(response);
    }

    public async Task<Result<AddressResponse>> AddAddressAsync(
        Guid userId,
        CreateAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var address = new UserAddress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title,
            FullAddress = request.FullAddress,
            City = request.City,
            District = request.District,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsDefault = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // İlk adres ise default yap
        var hasAddresses = await _unitOfWork.ReadRepository<UserAddress>()
            .AnyAsync(a => a.UserId == userId && a.IsActive, cancellationToken);

        if (!hasAddresses)
        {
            address.IsDefault = true;
        }

        await _unitOfWork.Repository<UserAddress>().AddAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AddressResponse(
            address.Id,
            address.Title,
            address.FullAddress,
            address.City,
            address.District,
            address.Latitude,
            address.Longitude,
            address.IsDefault,
            address.CreatedAt
        );

        return Result.Ok(response);
    }

    public async Task<Result<AddressResponse>> UpdateAddressAsync(
        Guid userId,
        Guid addressId,
        UpdateAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var address = await _unitOfWork.Repository<UserAddress>()
            .GetAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken: cancellationToken);

        if (address == null)
        {
            return Result.Fail<AddressResponse>("Address not found", "NOT_FOUND_ADDRESS");
        }

        address.Title = request.Title;
        address.FullAddress = request.FullAddress;
        address.City = request.City;
        address.District = request.District;
        address.Latitude = request.Latitude;
        address.Longitude = request.Longitude;
        address.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<UserAddress>().Update(address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AddressResponse(
            address.Id,
            address.Title,
            address.FullAddress,
            address.City,
            address.District,
            address.Latitude,
            address.Longitude,
            address.IsDefault,
            address.CreatedAt
        );

        return Result.Ok(response);
    }

    public async Task<Result> DeleteAddressAsync(
        Guid userId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        var address = await _unitOfWork.Repository<UserAddress>()
            .GetAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken: cancellationToken);

        if (address == null)
        {
            return Result.Fail("Address not found", "NOT_FOUND_ADDRESS");
        }

        // Soft delete
        address.IsActive = false;
        address.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<UserAddress>().Update(address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Eğer default adres silindiyse başka birini default yap
        if (address.IsDefault)
        {
            var newDefaultAddress = await _unitOfWork.Repository<UserAddress>()
                .GetAsync(a => a.UserId == userId && a.IsActive && a.Id != addressId, cancellationToken: cancellationToken);

            if (newDefaultAddress != null)
            {
                newDefaultAddress.IsDefault = true;
                _unitOfWork.Repository<UserAddress>().Update(newDefaultAddress);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        return Result.Ok();
    }

    public async Task<Result> SetDefaultAddressAsync(
        Guid userId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        var address = await _unitOfWork.Repository<UserAddress>()
            .GetAsync(a => a.Id == addressId && a.UserId == userId && a.IsActive, cancellationToken: cancellationToken);

        if (address == null)
        {
            return Result.Fail("Address not found", "NOT_FOUND_ADDRESS");
        }

        // Diğer adreslerin default'unu kaldır
        var otherAddresses = await _unitOfWork.Repository<UserAddress>()
            .GetPagedAsync(
                filter: a => a.UserId == userId && a.IsDefault && a.Id != addressId,
                cancellationToken: cancellationToken);

        foreach (var addr in otherAddresses)
        {
            addr.IsDefault = false;
            _unitOfWork.Repository<UserAddress>().Update(addr);
        }

        address.IsDefault = true;
        _unitOfWork.Repository<UserAddress>().Update(address);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
