import '../../domain/entities/address.dart';
import '../../domain/repositories/address_repository.dart';
import '../datasources/address_datasource.dart';

class AddressRepositoryImpl implements IAddressRepository {
  final IAddressDataSource _dataSource;

  AddressRepositoryImpl(this._dataSource);

  @override
  Future<List<UserAddress>> getUserAddresses() async {
    return await _dataSource.getUserAddresses();
  }

  @override
  Future<UserAddress> getAddressById(String addressId) async {
    return await _dataSource.getAddressById(addressId);
  }

  @override
  Future<UserAddress> createAddress(CreateAddressRequest request) async {
    return await _dataSource.createAddress(request);
  }

  @override
  Future<UserAddress> updateAddress(String addressId, UpdateAddressRequest request) async {
    return await _dataSource.updateAddress(addressId, request);
  }

  @override
  Future<void> deleteAddress(String addressId) async {
    return await _dataSource.deleteAddress(addressId);
  }

  @override
  Future<UserAddress> setDefaultAddress(String addressId) async {
    return await _dataSource.setDefaultAddress(addressId);
  }
}
