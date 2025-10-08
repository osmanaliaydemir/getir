import 'package:dio/dio.dart';
import '../../domain/entities/address.dart';

abstract class IAddressDataSource {
  Future<List<UserAddress>> getUserAddresses();
  Future<UserAddress> getAddressById(String addressId);
  Future<UserAddress> createAddress(CreateAddressRequest request);
  Future<UserAddress> updateAddress(
    String addressId,
    UpdateAddressRequest request,
  );
  Future<void> deleteAddress(String addressId);
  Future<UserAddress> setDefaultAddress(String addressId);
}

class AddressDataSourceImpl implements IAddressDataSource {
  final Dio _dio;

  AddressDataSourceImpl(this._dio);

  @override
  Future<List<UserAddress>> getUserAddresses() async {
    try {
      final response = await _dio.get('/api/v1/User/addresses');

      if (response.statusCode == 200) {
        final List<dynamic> data = response.data['data'] ?? response.data;
        return data.map((json) => UserAddress.fromJson(json)).toList();
      } else {
        throw Exception('Failed to load addresses: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error: $e');
    }
  }

  @override
  Future<UserAddress> getAddressById(String addressId) async {
    try {
      final response = await _dio.get('/api/v1/User/addresses/$addressId');

      if (response.statusCode == 200) {
        final data = response.data['data'] ?? response.data;
        return UserAddress.fromJson(data);
      } else {
        throw Exception('Failed to load address: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error: $e');
    }
  }

  @override
  Future<UserAddress> createAddress(CreateAddressRequest request) async {
    try {
      final response = await _dio.post(
        '/api/v1/User/addresses',
        data: request.toJson(),
      );

      if (response.statusCode == 201 || response.statusCode == 200) {
        final data = response.data['data'] ?? response.data;
        return UserAddress.fromJson(data);
      } else {
        throw Exception('Failed to create address: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error: $e');
    }
  }

  @override
  Future<UserAddress> updateAddress(
    String addressId,
    UpdateAddressRequest request,
  ) async {
    try {
      final response = await _dio.put(
        '/api/v1/User/addresses/$addressId',
        data: request.toJson(),
      );

      if (response.statusCode == 200) {
        final data = response.data['data'] ?? response.data;
        return UserAddress.fromJson(data);
      } else {
        throw Exception('Failed to update address: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error: $e');
    }
  }

  @override
  Future<void> deleteAddress(String addressId) async {
    try {
      final response = await _dio.delete('/api/v1/User/addresses/$addressId');

      if (response.statusCode != 200 && response.statusCode != 204) {
        throw Exception('Failed to delete address: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error: $e');
    }
  }

  @override
  Future<UserAddress> setDefaultAddress(String addressId) async {
    try {
      final response = await _dio.put(
        '/api/v1/User/addresses/$addressId/set-default',
      );

      if (response.statusCode == 200) {
        final data = response.data['data'] ?? response.data;
        return UserAddress.fromJson(data);
      } else {
        throw Exception(
          'Failed to set default address: ${response.statusCode}',
        );
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error: $e');
    }
  }

  Exception _handleDioError(DioException e) {
    switch (e.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
        return Exception(
          'Bağlantı zaman aşımı. Lütfen internet bağlantınızı kontrol edin.',
        );
      case DioExceptionType.badResponse:
        final statusCode = e.response?.statusCode;
        final message = e.response?.data?['message'] ?? 'Sunucu hatası';
        return Exception('Hata $statusCode: $message');
      case DioExceptionType.cancel:
        return Exception('İstek iptal edildi');
      case DioExceptionType.connectionError:
        return Exception(
          'Bağlantı hatası. Lütfen internet bağlantınızı kontrol edin.',
        );
      default:
        return Exception('Beklenmeyen hata: ${e.message}');
    }
  }
}

class CreateAddressRequest {
  final String title;
  final String fullAddress;
  final String? buildingNumber;
  final String? floor;
  final String? apartment;
  final String? landmark;
  final double latitude;
  final double longitude;
  final AddressType type;
  final bool isDefault;

  const CreateAddressRequest({
    required this.title,
    required this.fullAddress,
    this.buildingNumber,
    this.floor,
    this.apartment,
    this.landmark,
    required this.latitude,
    required this.longitude,
    required this.type,
    required this.isDefault,
  });

  Map<String, dynamic> toJson() {
    return {
      'title': title,
      'fullAddress': fullAddress,
      'buildingNumber': buildingNumber,
      'floor': floor,
      'apartment': apartment,
      'landmark': landmark,
      'latitude': latitude,
      'longitude': longitude,
      'type': type.value,
      'isDefault': isDefault,
    };
  }
}

class UpdateAddressRequest {
  final String? title;
  final String? fullAddress;
  final String? buildingNumber;
  final String? floor;
  final String? apartment;
  final String? landmark;
  final double? latitude;
  final double? longitude;
  final AddressType? type;
  final bool? isDefault;

  const UpdateAddressRequest({
    this.title,
    this.fullAddress,
    this.buildingNumber,
    this.floor,
    this.apartment,
    this.landmark,
    this.latitude,
    this.longitude,
    this.type,
    this.isDefault,
  });

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = {};

    if (title != null) data['title'] = title;
    if (fullAddress != null) data['fullAddress'] = fullAddress;
    if (buildingNumber != null) data['buildingNumber'] = buildingNumber;
    if (floor != null) data['floor'] = floor;
    if (apartment != null) data['apartment'] = apartment;
    if (landmark != null) data['landmark'] = landmark;
    if (latitude != null) data['latitude'] = latitude;
    if (longitude != null) data['longitude'] = longitude;
    if (type != null) data['type'] = type!.value;
    if (isDefault != null) data['isDefault'] = isDefault;

    return data;
  }
}
