import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../entities/address.dart';
import '../repositories/address_repository.dart';
import '../../data/datasources/address_datasource.dart';

/// Get User Addresses Use Case
class GetUserAddressesUseCase {
  final IAddressRepository _repository;

  GetUserAddressesUseCase(this._repository);

  Future<Result<List<UserAddress>>> call() async {
    return await _repository.getUserAddresses();
  }
}

/// Get Address By ID Use Case
class GetAddressByIdUseCase {
  final IAddressRepository _repository;

  GetAddressByIdUseCase(this._repository);

  Future<Result<UserAddress>> call(String addressId) async {
    if (addressId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Address ID cannot be empty',
          code: 'EMPTY_ADDRESS_ID',
        ),
      );
    }

    return await _repository.getAddressById(addressId);
  }
}

/// Create Address Use Case
class CreateAddressUseCase {
  final IAddressRepository _repository;

  CreateAddressUseCase(this._repository);

  Future<Result<UserAddress>> call(CreateAddressRequest request) async {
    return await _repository.createAddress(request);
  }
}

/// Update Address Use Case
class UpdateAddressUseCase {
  final IAddressRepository _repository;

  UpdateAddressUseCase(this._repository);

  Future<Result<UserAddress>> call(
    String addressId,
    UpdateAddressRequest request,
  ) async {
    if (addressId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Address ID cannot be empty',
          code: 'EMPTY_ADDRESS_ID',
        ),
      );
    }

    return await _repository.updateAddress(addressId, request);
  }
}

/// Delete Address Use Case
class DeleteAddressUseCase {
  final IAddressRepository _repository;

  DeleteAddressUseCase(this._repository);

  Future<Result<void>> call(String addressId) async {
    if (addressId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Address ID cannot be empty',
          code: 'EMPTY_ADDRESS_ID',
        ),
      );
    }

    return await _repository.deleteAddress(addressId);
  }
}

/// Set Default Address Use Case
class SetDefaultAddressUseCase {
  final IAddressRepository _repository;

  SetDefaultAddressUseCase(this._repository);

  Future<Result<UserAddress>> call(String addressId) async {
    if (addressId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Address ID cannot be empty',
          code: 'EMPTY_ADDRESS_ID',
        ),
      );
    }

    return await _repository.setDefaultAddress(addressId);
  }
}
