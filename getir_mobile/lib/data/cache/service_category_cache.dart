import '../models/service_category_dto.dart';

/// ServiceCategory için basit memory cache
/// Kategoriler sık değişmediği için cache'lenmesi performans artırır
class ServiceCategoryCache {
  static final ServiceCategoryCache _instance = ServiceCategoryCache._internal();
  
  factory ServiceCategoryCache() => _instance;
  
  ServiceCategoryCache._internal();

  List<ServiceCategoryDto>? _cachedCategories;
  DateTime? _cacheTime;
  
  /// Cache süresi (varsayılan: 30 dakika)
  final Duration cacheDuration = const Duration(minutes: 30);

  /// Cache'lenmiş kategorileri getir
  List<ServiceCategoryDto>? getCachedCategories() {
    if (_cachedCategories == null || _cacheTime == null) {
      return null;
    }

    final now = DateTime.now();
    final difference = now.difference(_cacheTime!);

    // Cache süresi dolmuşsa null dön
    if (difference > cacheDuration) {
      clear();
      return null;
    }

    return _cachedCategories;
  }

  /// Kategorileri cache'le
  void cacheCategories(List<ServiceCategoryDto> categories) {
    _cachedCategories = categories;
    _cacheTime = DateTime.now();
  }

  /// Cache'i temizle
  void clear() {
    _cachedCategories = null;
    _cacheTime = null;
  }

  /// Cache'in geçerli olup olmadığını kontrol et
  bool get isCacheValid {
    if (_cacheTime == null) return false;
    final now = DateTime.now();
    final difference = now.difference(_cacheTime!);
    return difference <= cacheDuration;
  }

  /// Cache'deki kategori sayısı
  int get cachedCount => _cachedCategories?.length ?? 0;
}

