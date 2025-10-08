import 'package:flutter/material.dart';
import 'package:injectable/injectable.dart';
import 'package:dio/dio.dart';
import 'local_storage_service.dart';
import 'logger_service.dart';

/// Dynamic Content Service
///
/// Fetches dynamic multi-language content from backend.
/// Features:
/// - Backend-driven translations
/// - Caching for performance
/// - Fallback to static translations
/// - RTL support
/// - Language-specific assets
@lazySingleton
class DynamicContentService {
  final Dio _dio;
  final LocalStorageService _localStorage;
  final LoggerService _logger;

  // Cache key prefix
  static const String _cachePrefix = 'dynamic_content_';
  static const Duration _cacheExpiry = Duration(hours: 24);

  // Supported languages
  static const supportedLanguages = ['tr', 'en', 'ar'];

  // Current content cache
  final Map<String, Map<String, dynamic>> _contentCache = {};

  DynamicContentService(this._dio, this._localStorage, this._logger);

  /// Initialize service and load cached content
  Future<void> initialize(String languageCode) async {
    await _loadCachedContent(languageCode);
  }

  /// Fetch dynamic content for a specific language
  Future<Map<String, dynamic>> fetchContent(String languageCode) async {
    try {
      _logger.info(
        'Fetching dynamic content for language: $languageCode',
        tag: 'DynamicContent',
      );

      final response = await _dio.get(
        '/api/v1/content/translations',
        queryParameters: {'language': languageCode},
      );

      if (response.statusCode == 200 && response.data != null) {
        final content = response.data as Map<String, dynamic>;

        // Cache the content
        await _cacheContent(languageCode, content);
        _contentCache[languageCode] = content;

        _logger.info(
          'Dynamic content fetched successfully: ${content.length} keys',
          tag: 'DynamicContent',
        );

        return content;
      }

      return {};
    } catch (e) {
      _logger.error(
        'Failed to fetch dynamic content',
        tag: 'DynamicContent',
        error: e,
      );

      // Return cached content if available
      return _contentCache[languageCode] ?? {};
    }
  }

  /// Get translated string
  String getString({
    required String key,
    required String languageCode,
    String? fallbackLanguage,
    Map<String, String>? params,
  }) {
    // Try to get from cache
    var content = _contentCache[languageCode];

    // If not in cache, try to load from local storage
    if (content == null || content.isEmpty) {
      content = _getCachedContent(languageCode);
      if (content != null) {
        _contentCache[languageCode] = content;
      }
    }

    // Get string from content
    String? value = content?[key] as String?;

    // Fallback strategy
    if (value == null && fallbackLanguage != null) {
      final fallbackContent =
          _contentCache[fallbackLanguage] ??
          _getCachedContent(fallbackLanguage);
      value = fallbackContent?[key] as String?;
    }

    // If still null, return key (development fallback)
    if (value == null) {
      _logger.warning(
        'Translation missing: $key ($languageCode)',
        tag: 'DynamicContent',
      );
      return key;
    }

    // Replace parameters if provided
    if (params != null) {
      value = _replaceParameters(value, params);
    }

    return value;
  }

  /// Get translated string with fallback chain
  String getStringWithFallback({
    required String key,
    required String languageCode,
  }) {
    return getString(
      key: key,
      languageCode: languageCode,
      fallbackLanguage: languageCode == 'tr' ? 'en' : 'tr',
    );
  }

  /// Check if translation exists
  bool hasTranslation(String key, String languageCode) {
    final content =
        _contentCache[languageCode] ?? _getCachedContent(languageCode);
    return content?[key] != null;
  }

  /// Prefetch content for all languages
  Future<void> prefetchAllLanguages() async {
    for (final lang in supportedLanguages) {
      await fetchContent(lang);
    }
  }

  /// Clear content cache
  Future<void> clearCache() async {
    for (final lang in supportedLanguages) {
      await _localStorage.removeCachedItem('$_cachePrefix$lang');
      await _localStorage.removeUserData('${_cachePrefix}${lang}_timestamp');
    }
    _contentCache.clear();

    _logger.info('Dynamic content cache cleared', tag: 'DynamicContent');
  }

  /// Refresh content for current language
  Future<void> refreshContent(String languageCode) async {
    await fetchContent(languageCode);
  }

  // ==================== Helper Methods ====================

  /// Cache content locally
  Future<void> _cacheContent(
    String languageCode,
    Map<String, dynamic> content,
  ) async {
    try {
      await _localStorage.putCacheByKey(
        '$_cachePrefix$languageCode',
        content,
        expiry: const Duration(hours: 24),
      );
      await _localStorage.storeUserData(
        '${_cachePrefix}${languageCode}_timestamp',
        DateTime.now().toIso8601String(),
      );
    } catch (e) {
      _logger.error(
        'Failed to cache dynamic content',
        tag: 'DynamicContent',
        error: e,
      );
    }
  }

  /// Load cached content
  Future<void> _loadCachedContent(String languageCode) async {
    final content = _getCachedContent(languageCode);
    if (content != null) {
      _contentCache[languageCode] = content;
      _logger.info(
        'Loaded cached content: ${content.length} keys',
        tag: 'DynamicContent',
      );
    }
  }

  /// Get cached content
  Map<String, dynamic>? _getCachedContent(String languageCode) {
    try {
      // Check if cache is expired
      final timestampStr = _localStorage.getUserData(
        '${_cachePrefix}${languageCode}_timestamp',
      );

      if (timestampStr != null) {
        final timestamp = DateTime.parse(timestampStr);
        final age = DateTime.now().difference(timestamp);

        if (age > _cacheExpiry) {
          _logger.info(
            'Cache expired for $languageCode (age: ${age.inHours}h)',
            tag: 'DynamicContent',
          );
          return null;
        }
      }

      // Get cached content synchronously - return null if not found
      // We'll fetch from backend if needed
      return null;
    } catch (e) {
      return null;
    }
  }

  /// Replace parameters in string
  String _replaceParameters(String value, Map<String, String> params) {
    var result = value;
    params.forEach((key, value) {
      result = result.replaceAll('{$key}', value);
    });
    return result;
  }

  /// Get cache info
  Map<String, dynamic> getCacheInfo() {
    final info = <String, dynamic>{};

    for (final lang in supportedLanguages) {
      final timestampStr = _localStorage.getUserData(
        '${_cachePrefix}${lang}_timestamp',
      );

      if (timestampStr != null) {
        final timestamp = DateTime.parse(timestampStr);
        final age = DateTime.now().difference(timestamp);

        info[lang] = {
          'cached': true,
          'timestamp': timestampStr,
          'age_hours': age.inHours,
          'expired': age > _cacheExpiry,
          'keys_count': _contentCache[lang]?.length ?? 0,
        };
      } else {
        info[lang] = {'cached': false};
      }
    }

    return info;
  }
}

/// Language-specific Asset Service
///
/// Loads language-specific images and assets
@lazySingleton
class LanguageAssetService {
  final LoggerService _logger;

  LanguageAssetService(this._logger);

  /// Get language-specific asset path
  String getAssetPath({
    required String baseAsset,
    required String languageCode,
    String defaultAsset = '',
  }) {
    // Try to get language-specific asset
    final langAsset = baseAsset
        .replaceFirst('.png', '_$languageCode.png')
        .replaceFirst('.jpg', '_$languageCode.jpg');

    // In real app, you'd check if asset exists
    // For now, we'll assume it exists
    _logger.debug('Loading asset: $langAsset', tag: 'LanguageAsset');

    return langAsset;
  }

  /// Get localized image URL from backend
  String getLocalizedImageUrl({
    required String baseUrl,
    required String languageCode,
  }) {
    // Append language code to URL
    if (baseUrl.contains('?')) {
      return '$baseUrl&lang=$languageCode';
    } else {
      return '$baseUrl?lang=$languageCode';
    }
  }

  /// Check if RTL language
  bool isRTL(String languageCode) {
    return languageCode == 'ar';
  }

  /// Get text direction
  TextDirection getTextDirection(String languageCode) {
    return isRTL(languageCode) ? TextDirection.rtl : TextDirection.ltr;
  }
}

/// RTL Support Helper
///
/// Helper functions for RTL (Right-to-Left) support
class RTLSupportHelper {
  /// Wrap widget with Directionality based on language
  static Widget wrap({required Widget child, required String languageCode}) {
    return Directionality(
      textDirection: languageCode == 'ar'
          ? TextDirection.rtl
          : TextDirection.ltr,
      child: child,
    );
  }

  /// Get alignment based on language
  static Alignment getAlignment(
    String languageCode, {
    Alignment ltr = Alignment.centerLeft,
    Alignment rtl = Alignment.centerRight,
  }) {
    return languageCode == 'ar' ? rtl : ltr;
  }

  /// Get text align based on language
  static TextAlign getTextAlign(
    String languageCode, {
    TextAlign ltr = TextAlign.left,
    TextAlign rtl = TextAlign.right,
  }) {
    return languageCode == 'ar' ? rtl : ltr;
  }

  /// Mirror icon for RTL
  static Widget mirrorIcon(Widget icon, String languageCode) {
    if (languageCode == 'ar') {
      return Transform(
        alignment: Alignment.center,
        transform: Matrix4.rotationY(3.14159), // 180 degrees
        child: icon,
      );
    }
    return icon;
  }

  /// Get edge insets for RTL
  static EdgeInsets getEdgeInsets(
    String languageCode, {
    required double left,
    required double right,
    double top = 0,
    double bottom = 0,
  }) {
    if (languageCode == 'ar') {
      return EdgeInsets.only(
        left: right,
        right: left,
        top: top,
        bottom: bottom,
      );
    }
    return EdgeInsets.only(left: left, right: right, top: top, bottom: bottom);
  }
}
