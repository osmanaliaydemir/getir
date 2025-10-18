import 'package:dio/dio.dart';
import '../../domain/entities/product.dart';

abstract class ProductDataSource {
  Future<List<Product>> getProducts({
    int page = 1,
    int limit = 20,
    String? merchantId,
    String? category,
    String? search,
  });

  Future<Product> getProductById(String id);
  Future<List<Product>> getProductsByMerchant(
    String merchantId, {
    int page = 1,
    int limit = 20,
  });
  Future<List<Product>> searchProducts(
    String query, {
    int page = 1,
    int limit = 20,
  });
  Future<List<String>> getCategories();
}

class ProductDataSourceImpl implements ProductDataSource {
  final Dio _dio;

  ProductDataSourceImpl(this._dio);

  @override
  Future<List<Product>> getProducts({
    int page = 1,
    int limit = 20,
    String? merchantId,
    String? category,
    String? search,
  }) async {
    try {
      final queryParams = <String, dynamic>{'page': page, 'limit': limit};

      if (merchantId != null) queryParams['merchantId'] = merchantId;
      if (category != null) queryParams['category'] = category;
      if (search != null) queryParams['search'] = search;

      final response = await _dio.get(
        '/api/v1/product',
        queryParameters: queryParams,
      );

      final List<dynamic> data = response.data['data'] ?? response.data;
      return data.map((json) => _productFromJson(json)).toList();
    } catch (e) {
      throw Exception('Failed to fetch products: $e');
    }
  }

  @override
  Future<Product> getProductById(String id) async {
    try {
      final response = await _dio.get('/api/v1/product/$id');
      return _productFromJson(response.data);
    } catch (e) {
      throw Exception('Failed to fetch product: $e');
    }
  }

  @override
  Future<List<Product>> getProductsByMerchant(
    String merchantId, {
    int page = 1,
    int limit = 20,
  }) async {
    try {
      final response = await _dio.get(
        '/api/v1/product/merchant/$merchantId',
        queryParameters: {'page': page, 'limit': limit},
      );
      final List<dynamic> data = response.data['data'] ?? response.data;
      return data.map((json) => _productFromJson(json)).toList();
    } catch (e) {
      throw Exception('Failed to fetch merchant products: $e');
    }
  }

  @override
  Future<List<Product>> searchProducts(
    String query, {
    int page = 1,
    int limit = 20,
  }) async {
    try {
      final response = await _dio.get(
        '/api/v1/search/products',
        queryParameters: {
          'query': query,
          'pageNumber': page,
          'pageSize': limit,
        },
      );

      final data = response.data['data'];
      if (data == null) return [];

      final List<dynamic> items = data['items'] ?? data;
      return items.map((json) => _productFromJson(json)).toList();
    } catch (e) {
      throw Exception('Failed to search products: $e');
    }
  }

  @override
  Future<List<String>> getCategories() async {
    try {
      final response = await _dio.get('/api/v1/productcategory');
      final List<dynamic> data = response.data['data'] ?? response.data;
      return data.map((category) => category.toString()).toList();
    } catch (e) {
      throw Exception('Failed to fetch categories: $e');
    }
  }

  Product _productFromJson(Map<String, dynamic> json) {
    return Product(
      id: json['id']?.toString() ?? '',
      name: json['name'] ?? '',
      description: json['description'] ?? '',
      price: (json['price'] ?? 0.0).toDouble(),
      imageUrl: json['imageUrl'] ?? '',
      category: json['category'] ?? '',
      merchantId: json['merchantId']?.toString() ?? '',
      merchantName: json['merchantName'] ?? '',
      isAvailable: json['isAvailable'] ?? false,
      stockQuantity: json['stockQuantity'] ?? 0,
      variants:
          (json['variants'] as List<dynamic>?)
              ?.map((v) => _productVariantFromJson(v))
              .toList() ??
          [],
      options:
          (json['options'] as List<dynamic>?)
              ?.map((o) => _productOptionFromJson(o))
              .toList() ??
          [],
      rating: (json['rating'] ?? 0.0).toDouble(),
      reviewCount: json['reviewCount'] ?? 0,
      unit: json['unit'] ?? 'adet',
      discountPrice: json['discountPrice']?.toDouble(),
      discountPercentage: json['discountPercentage']?.toDouble(),
      tags: List<String>.from(json['tags'] ?? []),
      nutritionalInfo: Map<String, dynamic>.from(json['nutritionalInfo'] ?? {}),
      brand: json['brand'] ?? '',
      barcode: json['barcode'] ?? '',
    );
  }

  ProductVariant _productVariantFromJson(Map<String, dynamic> json) {
    return ProductVariant(
      id: json['id']?.toString() ?? '',
      name: json['name'] ?? '',
      price: (json['price'] ?? 0.0).toDouble(),
      isAvailable: json['isAvailable'] ?? false,
      stockQuantity: json['stockQuantity'] ?? 0,
    );
  }

  ProductOption _productOptionFromJson(Map<String, dynamic> json) {
    return ProductOption(
      id: json['id']?.toString() ?? '',
      name: json['name'] ?? '',
      type: json['type'] ?? 'single',
      isRequired: json['isRequired'] ?? false,
      values:
          (json['values'] as List<dynamic>?)
              ?.map((v) => _productOptionValueFromJson(v))
              .toList() ??
          [],
    );
  }

  ProductOptionValue _productOptionValueFromJson(Map<String, dynamic> json) {
    return ProductOptionValue(
      id: json['id']?.toString() ?? '',
      name: json['name'] ?? '',
      price: (json['price'] ?? 0.0).toDouble(),
      isAvailable: json['isAvailable'] ?? false,
    );
  }
}
