import '../entities/address.dart';
import '../repositories/address_repository.dart';
import '../../data/datasources/address_datasource.dart';

class GetUserAddressesUseCase {
  final IAddressRepository _repository;

  GetUserAddressesUseCase(this._repository);

  Future<List<UserAddress>> call() async {
    return await _repository.getUserAddresses();
  }
}

class GetAddressByIdUseCase {
  final IAddressRepository _repository;

  GetAddressByIdUseCase(this._repository);

  Future<UserAddress> call(String addressId) async {
    return await _repository.getAddressById(addressId);
  }
}

class CreateAddressUseCase {
  final IAddressRepository _repository;

  CreateAddressUseCase(this._repository);

  Future<UserAddress> call(CreateAddressRequest request) async {
    return await _repository.createAddress(request);
  }
}

class UpdateAddressUseCase {
  final IAddressRepository _repository;

  UpdateAddressUseCase(this._repository);

  Future<UserAddress> call(String addressId, UpdateAddressRequest request) async {
    return await _repository.updateAddress(addressId, request);
  }
}

class DeleteAddressUseCase {
  final IAddressRepository _repository;

  DeleteAddressUseCase(this._repository);

  Future<void> call(String addressId) async {
    return await _repository.deleteAddress(addressId);
  }
}

class SetDefaultAddressUseCase {
  final IAddressRepository _repository;

  SetDefaultAddressUseCase(this._repository);

  Future<UserAddress> call(String addressId) async {
    return await _repository.setDefaultAddress(addressId);
  }
}
