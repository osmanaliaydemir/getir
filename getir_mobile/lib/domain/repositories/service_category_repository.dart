import '../../core/errors/result.dart';
import '../entities/service_category.dart';
import '../entities/service_category_type.dart';

/// ServiceCategory repository interface - Domain layer
abstract class IServiceCategoryRepository {
  /// Tüm aktif kategorileri getir (sayfalama ile)
  Future<Result<List<ServiceCategory>>> getServiceCategories({
    int page = 1,
    int pageSize = 20,
  });

  /// ID'ye göre kategori getir
  Future<Result<ServiceCategory>> getServiceCategoryById(String id);

  /// Türüne göre aktif kategorileri getir
  Future<Result<List<ServiceCategory>>> getActiveServiceCategoriesByType(
    ServiceCategoryType type,
  );

  /// Tüm aktif kategorileri getir (sayfalama yok - ana sayfa için)
  Future<Result<List<ServiceCategory>>> getAllActiveCategories();
}
