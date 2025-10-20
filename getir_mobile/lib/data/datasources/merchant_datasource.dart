import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import '../../domain/entities/merchant.dart';
import '../../domain/entities/service_category_type.dart';

abstract class MerchantDataSource {
  Future<List<Merchant>> getMerchants({
    int page = 1,
    int limit = 20,
    String? search,
    String? category,
    double? latitude,
    double? longitude,
    double? radius,
  });

  Future<Merchant> getMerchantById(String id);
  Future<List<Merchant>> searchMerchants(
    String query, {
    int page = 1,
    int limit = 20,
  });
  Future<List<Merchant>> getNearbyMerchants({
    required double latitude,
    required double longitude,
    double radius = 5.0,
  });

  Future<List<Merchant>> getNearbyMerchantsByCategory({
    required double latitude,
    required double longitude,
    required int categoryType,
    double radius = 5.0,
  });
}

class MerchantDataSourceImpl implements MerchantDataSource {
  final Dio _dio;

  MerchantDataSourceImpl(this._dio);

  @override
  Future<List<Merchant>> getMerchants({
    int page = 1,
    int limit = 20,
    String? search,
    String? category,
    double? latitude,
    double? longitude,
    double? radius,
  }) async {
    try {
      final queryParams = <String, dynamic>{'page': page, 'pageSize': limit};

      if (search != null) queryParams['search'] = search;
      if (category != null) queryParams['category'] = category;
      if (latitude != null) queryParams['latitude'] = latitude;
      if (longitude != null) queryParams['longitude'] = longitude;
      if (radius != null) queryParams['radius'] = radius;

      final response = await _dio.get(
        '/api/v1/merchant',
        queryParameters: queryParams,
      );

      // Response unwrap edilmiş
      final dynamic responseData = response.data;
      final List<dynamic> data = responseData is List
          ? responseData as List<dynamic>
          : (responseData is Map<String, dynamic> &&
                    responseData['items'] != null
                ? responseData['items'] as List<dynamic>
                : []);

      return data.map((json) => _merchantFromJson(json)).toList();
    } catch (e) {
      throw Exception('Failed to fetch merchants: $e');
    }
  }

  @override
  Future<Merchant> getMerchantById(String id) async {
    try {
      final response = await _dio.get('/api/v1/merchant/$id');
      return _merchantFromJson(response.data);
    } catch (e) {
      throw Exception('Failed to fetch merchant: $e');
    }
  }

  @override
  Future<List<Merchant>> searchMerchants(
    String query, {
    int page = 1,
    int limit = 20,
  }) async {
    try {
      final response = await _dio.get(
        '/api/v1/search/merchants',
        queryParameters: {'query': query, 'page': page, 'pageSize': limit},
      );

      // Response unwrap edilmiş
      final dynamic responseData = response.data;
      final List<dynamic> items = responseData is List
          ? responseData as List<dynamic>
          : (responseData is Map<String, dynamic> &&
                    responseData['items'] != null
                ? responseData['items'] as List<dynamic>
                : []);

      return items.map((json) => _merchantFromJson(json)).toList();
    } catch (e) {
      throw Exception('Failed to search merchants: $e');
    }
  }

  @override
  Future<List<Merchant>> getNearbyMerchants({
    required double latitude,
    required double longitude,
    double radius = 5.0,
  }) async {
    try {
      final response = await _dio.get(
        '/api/v1/geo/merchants/nearby',
        queryParameters: {
          'latitude': latitude,
          'longitude': longitude,
          'radius': radius,
        },
      );

      // Response zaten interceptor'da unwrap edilmiş - direkt array olmalı
      debugPrint(
        '🔍 [MerchantDataSource] getNearbyMerchants response type: ${response.data.runtimeType}',
      );

      final List<dynamic> data = response.data is List
          ? response.data as List<dynamic>
          : [];

      debugPrint('✅ [MerchantDataSource] Parsed ${data.length} merchants');
      return data.map((json) => _merchantFromJson(json)).toList();
    } catch (e) {
      throw Exception('Failed to fetch nearby merchants: $e');
    }
  }

  @override
  Future<List<Merchant>> getNearbyMerchantsByCategory({
    required double latitude,
    required double longitude,
    required int categoryType,
    double radius = 5.0,
  }) async {
    try {
      final response = await _dio.get(
        '/api/v1/geo/merchants/nearby',
        queryParameters: {
          'latitude': latitude,
          'longitude': longitude,
          'categoryType': categoryType,
          'radius': radius,
        },
      );

      // Response zaten interceptor'da unwrap edilmiş - direkt array olmalı
      debugPrint(
        '🔍 [MerchantDataSource] getNearbyMerchantsByCategory response type: ${response.data.runtimeType}',
      );
      debugPrint('   CategoryType: $categoryType');

      final List<dynamic> data = response.data is List
          ? response.data as List<dynamic>
          : [];

      debugPrint(
        '✅ [MerchantDataSource] Parsed ${data.length} merchants for category $categoryType',
      );
      return data.map((json) => _merchantFromJson(json)).toList();
    } catch (e) {
      throw Exception('Failed to fetch nearby merchants by category: $e');
    }
  }

  Merchant _merchantFromJson(Map<String, dynamic> json) {
    return Merchant(
      id: json['id']?.toString() ?? '',
      name: json['name'] ?? '',
      description: json['description'] ?? '',
      logoUrl: json['logoUrl'] ?? '',
      coverImageUrl: json['coverImageUrl'] ?? '',
      rating: (json['rating'] ?? 0.0).toDouble(),
      reviewCount: json['reviewCount'] ?? 0,
      deliveryFee: (json['deliveryFee'] ?? 0.0).toDouble(),
      estimatedDeliveryTime: json['estimatedDeliveryTime'] ?? 30,
      distance: (json['distance'] ?? 0.0).toDouble(),
      isOpen: json['isOpen'] ?? false,
      address: json['address'] ?? '',
      phoneNumber: json['phoneNumber'] ?? '',
      categories: List<String>.from(json['categories'] ?? []),
      workingHours: Map<String, String>.from(json['workingHours'] ?? {}),
      minimumOrderAmount: (json['minimumOrderAmount'] ?? 0.0).toDouble(),
      isDeliveryAvailable: json['isDeliveryAvailable'] ?? true,
      isPickupAvailable: json['isPickupAvailable'] ?? false,
      categoryType: json['categoryType'] != null
          ? ServiceCategoryType.fromInt(json['categoryType'] as int)
          : null,
    );
  }
}
