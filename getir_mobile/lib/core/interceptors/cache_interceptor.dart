import 'package:dio/dio.dart';
import 'package:dio_cache_interceptor/dio_cache_interceptor.dart';
import 'package:dio_cache_interceptor_hive_store/dio_cache_interceptor_hive_store.dart';
import 'package:path_provider/path_provider.dart';
import '../services/logger_service.dart';
import 'package:flutter/foundation.dart';

/// API Response Cache Interceptor
/// Uses Hive for persistent caching of API responses
/// Improves performance and reduces network calls
class ApiCacheInterceptor {
  static CacheOptions? _cacheOptions;
  static bool _isInitialized = false;

  /// Check if cache is initialized
  static bool get isInitialized => _isInitialized;

  /// Initialize cache with Hive store
  static Future<void> initialize() async {
    if (_isInitialized) return;

    try {
      // Get app directory for cache storage
      final appDir = await getApplicationDocumentsDirectory();
      final cachePath = '${appDir.path}/api_cache';

      // Create cache store
      final cacheStore = HiveCacheStore(
        cachePath,
        hiveBoxName: 'getir_api_cache',
      );

      // Configure cache options
      _cacheOptions = CacheOptions(
        store: cacheStore,
        policy: CachePolicy.request, // Respect cache headers from server
        hitCacheOnErrorExcept: [
          401,
          403,
        ], // Use cache on error except auth errors
        maxStale: const Duration(days: 7), // Maximum stale duration
        priority: CachePriority.normal,
        cipher: null, // Add encryption if needed
        keyBuilder: CacheOptions.defaultCacheKeyBuilder,
        allowPostMethod: false, // Don't cache POST requests
      );

      _isInitialized = true;
      logger.info('API Cache initialized successfully', tag: 'Cache');
    } catch (e) {
      logger.error('API Cache initialization failed', tag: 'Cache', error: e);
      _isInitialized = false;
    }
  }

  /// Get cache interceptor instance
  static DioCacheInterceptor get interceptor {
    if (_cacheOptions == null) {
      throw StateError(
        'ApiCacheInterceptor not initialized. Call initialize() first.',
      );
    }
    return DioCacheInterceptor(options: _cacheOptions!);
  }

  /// Create cache options for specific requests
  /// Example usage:
  /// ```dart
  /// dio.get('/api/merchants',
  ///   options: ApiCacheInterceptor.buildCacheOptions(
  ///     policy: CachePolicy.forceCache,
  ///     maxAge: Duration(hours: 1),
  ///   ).toOptions(),
  /// );
  /// ```
  static CacheOptions buildCacheOptions({
    CachePolicy? policy,
    Duration? maxAge,
    CachePriority? priority,
    bool allowPostMethod = false,
  }) {
    if (_cacheOptions == null) {
      throw StateError(
        'ApiCacheInterceptor not initialized. Call initialize() first.',
      );
    }

    return CacheOptions(
      store: _cacheOptions!.store,
      policy: policy ?? _cacheOptions!.policy,
      maxStale: maxAge ?? _cacheOptions!.maxStale,
      priority: priority ?? _cacheOptions!.priority,
      allowPostMethod: allowPostMethod,
      hitCacheOnErrorExcept: _cacheOptions!.hitCacheOnErrorExcept,
      keyBuilder: _cacheOptions!.keyBuilder,
      cipher: _cacheOptions!.cipher,
    );
  }

  /// Clear all cached responses
  static Future<void> clearCache() async {
    if (_cacheOptions?.store == null) return;

    try {
      await _cacheOptions!.store!.clean();
      logger.info('API Cache cleared successfully', tag: 'Cache');
    } catch (e) {
      logger.error('API Cache clear failed', tag: 'Cache', error: e);
    }
  }

  /// Clear specific cache entry by key
  static Future<void> clearCacheByKey(String key) async {
    if (_cacheOptions?.store == null) return;

    try {
      await _cacheOptions!.store!.delete(key);
      logger.debug(
        'API Cache key deleted',
        tag: 'Cache',
        context: {'key': key},
      );
    } catch (e) {
      logger.error(
        'API Cache delete failed',
        tag: 'Cache',
        error: e,
        context: {'key': key},
      );
    }
  }

  /// Clear expired cache entries
  static Future<void> clearExpiredCache() async {
    if (_cacheOptions?.store == null) return;

    try {
      await _cacheOptions!.store!.clean(staleOnly: true);
      logger.debug('Cleared expired cache entries', tag: 'APICache');
    } catch (e) {
      logger.error('Clear expired cache failed', tag: 'APICache', error: e);
    }
  }
}

/// Cache policy helper
/// Provides convenient methods for common cache strategies
class CachePolicyHelper {
  /// Cache first, network as fallback
  /// Best for: Static content, master data
  static CachePolicy get cacheFirst => CachePolicy.forceCache;

  /// Network first, cache as fallback
  /// Best for: Dynamic content with offline support
  static CachePolicy get networkFirst => CachePolicy.refreshForceCache;

  /// Always fetch from network
  /// Best for: Real-time data, user-specific content
  static CachePolicy get networkOnly => CachePolicy.noCache;

  /// Use cache, refresh in background
  /// Best for: Balance between speed and freshness
  static CachePolicy get cacheAndRefresh => CachePolicy.forceCache;

  /// Request decides (respects server cache headers)
  /// Best for: When server controls caching strategy
  static CachePolicy get request => CachePolicy.request;
}

/// Cacheable request mixin
/// Add this to your API data sources for easy caching
mixin CacheableRequest {
  /// Make a GET request with cache
  Future<Response<T>> getCached<T>(
    Dio dio,
    String path, {
    Map<String, dynamic>? queryParameters,
    Options? options,
    CachePolicy policy = CachePolicy.request,
    Duration maxAge = const Duration(hours: 1),
  }) async {
    final cacheOptions = ApiCacheInterceptor.buildCacheOptions(
      policy: policy,
      maxAge: maxAge,
    ).toOptions();

    return await dio.get<T>(
      path,
      queryParameters: queryParameters,
      options: options?.copyWith(extra: cacheOptions.extra) ?? cacheOptions,
    );
  }
}

/// Cache durations for different types of data
class CacheDurations {
  static const short = Duration(minutes: 5); // Real-time data
  static const medium = Duration(hours: 1); // Dynamic data
  static const long = Duration(hours: 24); // Static data
  static const veryLong = Duration(days: 7); // Master data
}
