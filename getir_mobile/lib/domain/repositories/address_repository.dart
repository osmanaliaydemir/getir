import '../entities/address.dart';
import '../../data/datasources/address_datasource.dart';

abstract class IAddressRepository {
  Future<List<UserAddress>> getUserAddresses();
  Future<UserAddress> getAddressById(String addressId);
  Future<UserAddress> createAddress(CreateAddressRequest request);
  Future<UserAddress> updateAddress(String addressId, UpdateAddressRequest request);
  Future<void> deleteAddress(String addressId);
  Future<UserAddress> setDefaultAddress(String addressId);
}
