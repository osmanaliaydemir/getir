import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../domain/entities/address.dart';
import '../../domain/repositories/address_repository.dart';
import '../datasources/address_datasource.dart';

class AddressRepositoryImpl implements IAddressRepository {
  final IAddressDataSource _dataSource;

  AddressRepositoryImpl(this._dataSource);

  @override
  Future<Result<List<UserAddress>>> getUserAddresses() async {
    try {
      final addresses = await _dataSource.getUserAddresses();
      return Result.success(addresses);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get addresses: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<UserAddress>> getAddressById(String addressId) async {
    try {
      final address = await _dataSource.getAddressById(addressId);
      return Result.success(address);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get address: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<UserAddress>> createAddress(
    CreateAddressRequest request,
  ) async {
    try {
      final address = await _dataSource.createAddress(request);
      return Result.success(address);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to create address: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<UserAddress>> updateAddress(
    String addressId,
    UpdateAddressRequest request,
  ) async {
    try {
      final address = await _dataSource.updateAddress(addressId, request);
      return Result.success(address);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to update address: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<void>> deleteAddress(String addressId) async {
    try {
      await _dataSource.deleteAddress(addressId);
      return Result.success(null);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to delete address: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<UserAddress>> setDefaultAddress(String addressId) async {
    try {
      final address = await _dataSource.setDefaultAddress(addressId);
      return Result.success(address);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to set default address: ${e.toString()}'),
      );
    }
  }
}
