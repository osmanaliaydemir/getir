import '../../core/errors/result.dart';
import '../entities/address.dart';
import '../repositories/address_repository.dart';
import '../../data/datasources/address_datasource.dart';

/// Address Service
///
/// Centralized service for all address-related operations.
/// Replaces 6 separate UseCase classes.
class AddressService {
  final IAddressRepository _repository;

  const AddressService(this._repository);

  Future<Result<List<UserAddress>>> getUserAddresses() async {
    return await _repository.getUserAddresses();
  }

  Future<Result<UserAddress>> getAddressById(String addressId) async {
    return await _repository.getAddressById(addressId);
  }

  Future<Result<UserAddress>> createAddress(
    CreateAddressRequest request,
  ) async {
    return await _repository.createAddress(request);
  }

  Future<Result<UserAddress>> updateAddress(
    String addressId,
    UpdateAddressRequest request,
  ) async {
    return await _repository.updateAddress(addressId, request);
  }

  Future<Result<void>> deleteAddress(String addressId) async {
    return await _repository.deleteAddress(addressId);
  }

  Future<Result<UserAddress>> setDefaultAddress(String addressId) async {
    return await _repository.setDefaultAddress(addressId);
  }
}
