import '../../core/errors/result.dart';
import '../entities/address.dart';
import '../../data/datasources/address_datasource.dart';

abstract class IAddressRepository {
  Future<Result<List<UserAddress>>> getUserAddresses();
  Future<Result<UserAddress>> getAddressById(String addressId);
  Future<Result<UserAddress>> createAddress(CreateAddressRequest request);
  Future<Result<UserAddress>> updateAddress(
    String addressId,
    UpdateAddressRequest request,
  );
  Future<Result<void>> deleteAddress(String addressId);
  Future<Result<UserAddress>> setDefaultAddress(String addressId);
}
