import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'local_storage_service.dart';
import '../errors/app_exceptions.dart';

/// Advanced API caching service with intelligent cache management
class ApiCacheService {
  static final ApiCacheService _instance = ApiCacheService._internal();
  factory ApiCacheService() => _instance;
  ApiCacheService._internal();

  final LocalStorageService _localStorage = LocalStorageService();
  final Map<String, CacheConfig> _cacheConfigs = {};

  // Default cache configurations (durations set directly per-endpoint below)

  /// Initialize cache service with default configurations
  Future<void> initialize() async {
    // Configure cache for different types of data
    _configureCacheConfigs();

    // Clean expired cache on startup
    await _localStorage.clearExpiredCache();

    if (kDebugMode) {
      print('API Cache Service initialized');
    }
  }

  /// Configure cache settings for different endpoints
  void _configureCacheConfigs() {
    // Product data - cache for 30 minutes
    _cacheConfigs['/api/v1/products'] = CacheConfig(
      duration: const Duration(minutes: 30),
      priority: CachePriority.high,
      maxSize: 100,
    );

    // Merchant data - cache for 1 hour
    _cacheConfigs['/api/v1/merchants'] = CacheConfig(
      duration: const Duration(hours: 1),
      priority: CachePriority.high,
      maxSize: 50,
    );

    // User profile - cache for 15 minutes
    _cacheConfigs['/api/v1/user/profile'] = CacheConfig(
      duration: const Duration(minutes: 15),
      priority: CachePriority.medium,
      maxSize: 1,
    );

    // Cart data - cache for 5 minutes
    _cacheConfigs['/api/v1/cart'] = CacheConfig(
      duration: const Duration(minutes: 5),
      priority: CachePriority.high,
      maxSize: 1,
    );

    // Order history - cache for 1 hour
    _cacheConfigs['/api/v1/orders'] = CacheConfig(
      duration: const Duration(hours: 1),
      priority: CachePriority.medium,
      maxSize: 20,
    );

    // Categories - cache for 2 hours
    _cacheConfigs['/api/v1/categories'] = CacheConfig(
      duration: const Duration(hours: 2),
      priority: CachePriority.high,
      maxSize: 30,
    );

    // Search results - cache for 10 minutes
    _cacheConfigs['/api/v1/search'] = CacheConfig(
      duration: const Duration(minutes: 10),
      priority: CachePriority.low,
      maxSize: 50,
    );
  }

  /// Get cached API response
  Future<Map<String, dynamic>?> getCachedResponse(String endpoint) async {
    try {
      final config = _getCacheConfig(endpoint);
      if (config == null) return null;

      final cached = await _localStorage.getCachedApiResponse(endpoint);
      if (cached == null) return null;

      // Check if cache is still valid
      if (_isCacheValid(cached, config)) {
        _updateCacheAccess(endpoint);
        return cached;
      } else {
        // Remove expired cache
        await _removeCachedResponse(endpoint);
        return null;
      }
    } catch (e) {
      if (kDebugMode) {
        print('Error getting cached response: $e');
      }
      return null;
    }
  }

  /// Get cached response by explicit cache key
  Future<Map<String, dynamic>?> _getCachedByKey(
    String cacheKey,
    String endpoint,
  ) async {
    try {
      final config = _getCacheConfig(endpoint);
      if (config == null) return null;

      final cached = await _localStorage.getCacheByKey(cacheKey);
      if (cached == null) return null;

      if (_isCacheValid(cached, config)) {
        _updateCacheAccess(cacheKey);
        return cached;
      } else {
        await _removeCachedResponse(cacheKey);
        return null;
      }
    } catch (e) {
      if (kDebugMode) {
        print('Error getting cached by key: $e');
      }
      return null;
    }
  }

  /// Cache API response
  Future<void> cacheResponse(
    String endpoint,
    Map<String, dynamic> data, {
    Duration? customDuration,
  }) async {
    try {
      final config = _getCacheConfig(endpoint);
      if (config == null) return;

      final duration = customDuration ?? config.duration;

      // Check cache size limits
      await _enforceCacheSizeLimit(endpoint, config);

      await _localStorage.cacheApiResponse(endpoint, data, expiry: duration);

      if (kDebugMode) {
        print('Cached response for $endpoint (${duration.inMinutes}min)');
      }
    } catch (e) {
      if (kDebugMode) {
        print('Error caching response: $e');
      }
    }
  }

  /// Cache API response by explicit cache key
  Future<void> _cacheByKey(
    String cacheKey,
    String endpoint,
    Map<String, dynamic> data, {
    Duration? customDuration,
  }) async {
    try {
      final config = _getCacheConfig(endpoint);
      if (config == null) return;

      final duration = customDuration ?? config.duration;
      await _enforceCacheSizeLimit(cacheKey, config);
      await _localStorage.putCacheByKey(cacheKey, data, expiry: duration);
      if (kDebugMode) {
        print('Cached response for key=$cacheKey (${duration.inMinutes}min)');
      }
    } catch (e) {
      if (kDebugMode) {
        print('Error caching by key: $e');
      }
    }
  }

  /// Make API call with caching
  Future<Map<String, dynamic>> getWithCache(
    String endpoint, {
    Map<String, dynamic>? queryParameters,
    Map<String, dynamic>? headers,
    bool forceRefresh = false,
    Duration? ttlOverride,
  }) async {
    try {
      final cacheKey = buildCacheKey(
        endpoint,
        queryParameters: queryParameters,
      );
      // Try to get from cache first (unless force refresh)
      if (!forceRefresh) {
        final cached = await _getCachedByKey(cacheKey, endpoint);
        if (cached != null) {
          return cached;
        }
      }

      // Make API call
      final dio = Dio();
      final response = await dio.get(
        endpoint,
        queryParameters: queryParameters,
        options: Options(headers: headers),
      );

      if (response.statusCode == 200) {
        final data = Map<String, dynamic>.from(response.data);

        // Cache the response
        await _cacheByKey(
          cacheKey,
          endpoint,
          data,
          customDuration: ttlOverride,
        );

        return data;
      } else {
        throw ApiException(
          message: 'API call failed with status ${response.statusCode}',
          statusCode: response.statusCode,
        );
      }
    } on DioException catch (e) {
      // If network error, try to return cached data
      if (e.type == DioExceptionType.connectionError) {
        final fallbackKey = buildCacheKey(
          endpoint,
          queryParameters: queryParameters,
        );
        final cached = await _getCachedByKey(fallbackKey, endpoint);
        if (cached != null) {
          if (kDebugMode) {
            print('Using cached data due to network error');
          }
          return cached;
        }
      }

      throw ExceptionFactory.fromDioError(e);
    } catch (e) {
      // Try to return cached data as fallback
      final fallbackKey = buildCacheKey(
        endpoint,
        queryParameters: queryParameters,
      );
      final cached = await _getCachedByKey(fallbackKey, endpoint);
      if (cached != null) {
        if (kDebugMode) {
          print('Using cached data as fallback');
        }
        return cached;
      }

      rethrow;
    }
  }

  /// Post with caching (for responses that should be cached)
  Future<Map<String, dynamic>> postWithCache(
    String endpoint,
    Map<String, dynamic> data, {
    Map<String, dynamic>? headers,
    bool shouldCacheResponse = false,
  }) async {
    try {
      final dio = Dio();
      final response = await dio.post(
        endpoint,
        data: data,
        options: Options(headers: headers),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        final responseData = Map<String, dynamic>.from(response.data);

        // Cache response if requested
        if (shouldCacheResponse) {
          await cacheResponse(endpoint, responseData);
        }

        return responseData;
      } else {
        throw ApiException(
          message: 'API call failed with status ${response.statusCode}',
          statusCode: response.statusCode,
        );
      }
    } on DioException catch (e) {
      throw ExceptionFactory.fromDioError(e);
    }
  }

  /// Remove cached response
  Future<void> _removeCachedResponse(String endpoint) async {
    try {
      await _localStorage.removeCachedItem(endpoint);
    } catch (e) {
      if (kDebugMode) {
        print('Error removing cached response: $e');
      }
    }
  }

  /// Check if cache is valid
  bool _isCacheValid(Map<String, dynamic> cached, CacheConfig config) {
    // Prefer explicit expiry if present
    final explicitExpiryMs = cached['expiry'] as int?;
    final timestamp = cached['timestamp'] as int?;
    if (timestamp == null) return false;
    final nowMs = DateTime.now().millisecondsSinceEpoch;
    if (explicitExpiryMs != null) {
      return nowMs - timestamp < explicitExpiryMs;
    }
    final cacheTime = DateTime.fromMillisecondsSinceEpoch(timestamp);
    final age = DateTime.now().difference(cacheTime);
    return age < config.duration;
  }

  /// Update cache access time for LRU
  void _updateCacheAccess(String endpoint) {
    // This would be implemented with a more sophisticated LRU system
    // For now, we'll just log the access
    if (kDebugMode) {
      print('Cache accessed: $endpoint');
    }
  }

  /// Enforce cache size limits
  Future<void> _enforceCacheSizeLimit(
    String endpoint,
    CacheConfig config,
  ) async {
    final currentSize = _localStorage.getCacheSize();
    if (currentSize > config.maxSize) {
      await _localStorage.clearExpiredCache();
      await _localStorage.trimCacheToSize(config.maxSize);
    }
  }

  /// Get cache configuration for endpoint
  CacheConfig? _getCacheConfig(String endpoint) {
    // Find matching config by endpoint pattern
    for (final pattern in _cacheConfigs.keys) {
      if (endpoint.contains(pattern)) {
        return _cacheConfigs[pattern];
      }
    }
    return null;
  }

  /// Generate cache key
  String _generateCacheKey(String endpoint) {
    return endpoint.replaceAll(RegExp(r'[^a-zA-Z0-9]'), '_');
  }

  /// Public: Build canonical cache key from endpoint and query params
  String buildCacheKey(
    String endpoint, {
    Map<String, dynamic>? queryParameters,
  }) {
    if (queryParameters == null || queryParameters.isEmpty) {
      return _generateCacheKey(endpoint);
    }
    final sortedKeys = queryParameters.keys.toList()..sort();
    final buffer = StringBuffer(endpoint);
    buffer.write('?');
    for (var i = 0; i < sortedKeys.length; i++) {
      final key = sortedKeys[i];
      final value = queryParameters[key];
      buffer.write('$key=$value');
      if (i < sortedKeys.length - 1) buffer.write('&');
    }
    return _generateCacheKey(buffer.toString());
  }

  // Standardized cache key helpers
  String keyForProductList({
    String? merchantId,
    String? categoryId,
    int? page,
    String? search,
    String? sort,
  }) {
    final params = <String, dynamic>{};
    if (merchantId != null) params['merchantId'] = merchantId;
    if (categoryId != null) params['categoryId'] = categoryId;
    if (page != null) params['page'] = page;
    if (search != null && search.isNotEmpty) params['q'] = search;
    if (sort != null && sort.isNotEmpty) params['sort'] = sort;
    return buildCacheKey('/api/v1/products', queryParameters: params);
  }

  String keyForProductDetail(String productId) {
    return buildCacheKey('/api/v1/products/$productId');
  }

  String keyForNearbyMerchants({
    required double latitude,
    required double longitude,
    int? radiusMeters,
  }) {
    final params = <String, dynamic>{'lat': latitude, 'lng': longitude};
    if (radiusMeters != null) params['radius'] = radiusMeters;
    return buildCacheKey(
      '/api/v1/geo/merchants/nearby',
      queryParameters: params,
    );
  }

  /// Clear all cache
  Future<void> clearAllCache() async {
    await _localStorage.clearAllCache();
  }

  /// Clear cache for specific endpoint
  Future<void> clearCacheForEndpoint(String endpoint) async {
    await _removeCachedResponse(endpoint);
  }

  /// Get cache statistics
  Map<String, dynamic> getCacheStats() {
    return {
      'total_cached_items': _localStorage.getCacheSize(),
      'cache_configs': _cacheConfigs.length,
      'memory_usage': _localStorage.getCacheSize() * 1024, // Approximate
    };
  }

  /// Preload important data
  Future<void> preloadImportantData() async {
    final importantEndpoints = [
      '/api/v1/categories',
      '/api/v1/merchants',
      '/api/v1/products?featured=true',
    ];

    for (final endpoint in importantEndpoints) {
      try {
        await getWithCache(endpoint);
        if (kDebugMode) {
          print('Preloaded: $endpoint');
        }
      } catch (e) {
        if (kDebugMode) {
          print('Failed to preload $endpoint: $e');
        }
      }
    }
  }
}

/// Cache configuration for different endpoints
class CacheConfig {
  final Duration duration;
  final CachePriority priority;
  final int maxSize;

  const CacheConfig({
    required this.duration,
    required this.priority,
    required this.maxSize,
  });
}

/// Cache priority levels
enum CachePriority { low, medium, high }
