import 'package:dio/dio.dart';
import '../models/service_category_dto.dart';
import '../cache/service_category_cache.dart';
import '../../domain/entities/service_category_type.dart';

/// ServiceCategory için data source interface
abstract class ServiceCategoryDataSource {
  /// Tüm aktif kategorileri getir (sayfalama ile)
  Future<List<ServiceCategoryDto>> getServiceCategories({
    int page = 1,
    int pageSize = 20,
  });

  /// ID'ye göre kategori getir
  Future<ServiceCategoryDto> getServiceCategoryById(String id);

  /// Türüne göre aktif kategorileri getir
  Future<List<ServiceCategoryDto>> getActiveServiceCategoriesByType(
    ServiceCategoryType type,
  );

  /// Tüm aktif kategorileri getir (sayfalama yok - dropdown için)
  Future<List<ServiceCategoryDto>> getAllActiveCategories();
}

/// ServiceCategory data source implementasyonu
class ServiceCategoryDataSourceImpl implements ServiceCategoryDataSource {
  final Dio _dio;
  final ServiceCategoryCache _cache = ServiceCategoryCache();

  ServiceCategoryDataSourceImpl(this._dio);

  @override
  Future<List<ServiceCategoryDto>> getServiceCategories({
    int page = 1,
    int pageSize = 20,
  }) async {
    try {
      final response = await _dio.get(
        '/api/v1/ServiceCategory',
        queryParameters: {'page': page, 'pageSize': pageSize},
      );

      // Response zaten interceptor'da unwrap edilmiş
      final dynamic data = response.data;
      if (data == null) return [];

      // data = {"items": [...], "total": 9, ...}
      final List<dynamic> items = data is Map<String, dynamic>
          ? (data['items'] as List<dynamic>? ?? [])
          : (data as List<dynamic>? ?? []);

      return items.map((json) => ServiceCategoryDto.fromJson(json)).toList();
    } catch (e) {
      throw Exception('Failed to fetch service categories: $e');
    }
  }

  @override
  Future<ServiceCategoryDto> getServiceCategoryById(String id) async {
    try {
      final response = await _dio.get('/api/v1/ServiceCategory/$id');
      return ServiceCategoryDto.fromJson(response.data);
    } catch (e) {
      throw Exception('Failed to fetch service category: $e');
    }
  }

  @override
  Future<List<ServiceCategoryDto>> getActiveServiceCategoriesByType(
    ServiceCategoryType type,
  ) async {
    try {
      final response = await _dio.get(
        '/api/v1/ServiceCategory/active/by-type/${type.value}',
      );

      // Response zaten unwrap edilmiş
      final dynamic data = response.data;
      final List<dynamic> items = data is Map<String, dynamic>
          ? (data['items'] as List<dynamic>? ?? [])
          : (data as List<dynamic>? ?? []);

      return items.map((json) => ServiceCategoryDto.fromJson(json)).toList();
    } catch (e) {
      throw Exception('Failed to fetch service categories by type: $e');
    }
  }

  @override
  Future<List<ServiceCategoryDto>> getAllActiveCategories() async {
    try {
      // Önce cache'den kontrol et
      final cachedCategories = _cache.getCachedCategories();
      if (cachedCategories != null) {
        return cachedCategories;
      }

      // Cache yoksa API'den çek
      final response = await _dio.get(
        '/api/v1/ServiceCategory',
        queryParameters: {
          'page': 1,
          'pageSize': 100, // Ana sayfada göstermek için yeterli
        },
      );

      // Response zaten interceptor'da unwrap edilmiş
      final dynamic data = response.data;
      if (data == null) return [];

      // data = {"items": [...], "total": 9, ...}
      final List<dynamic> items = data is Map<String, dynamic>
          ? (data['items'] as List<dynamic>? ?? [])
          : (data as List<dynamic>? ?? []);

      // Sadece aktif olanları filtrele ve displayOrder'a göre sırala
      final categories = items
          .map((json) => ServiceCategoryDto.fromJson(json))
          .where((cat) => cat.isActive)
          .toList();

      categories.sort((a, b) => a.displayOrder.compareTo(b.displayOrder));

      // Cache'e kaydet
      _cache.cacheCategories(categories);

      return categories;
    } catch (e) {
      throw Exception('Failed to fetch all active categories: $e');
    }
  }
}
