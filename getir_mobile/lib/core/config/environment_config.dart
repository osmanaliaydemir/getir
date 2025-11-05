import 'package:flutter/foundation.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

/// Environment configuration for the app
/// Supports dev, staging, and production environments
class EnvironmentConfig {
  static const String _apiBaseUrlKey = 'API_BASE_URL';
  static const String _apiTimeoutKey = 'API_TIMEOUT';
  static const String _apiKeyKey = 'API_KEY';
  static const String _encryptionKeyKey = 'ENCRYPTION_KEY';
  static const String _enableSslPinningKey = 'ENABLE_SSL_PINNING';
  static const String _debugModeKey = 'DEBUG_MODE';
  static const String _googleMapsApiKeyKey = 'GOOGLE_MAPS_API_KEY';

  // Environment types
  static const String dev = 'dev';
  static const String staging = 'staging';
  static const String prod = 'prod';

  // Current environment (default: dev)
  static String _currentEnvironment = dev;

  /// Initialize environment from .env file
  static Future<void> initialize({String environment = dev}) async {
    _currentEnvironment = environment;

    // Load .env file based on environment
    final envFile = environment == prod
        ? '.env.prod'
        : environment == staging
        ? '.env.staging'
        : '.env.dev';

    try {
      await dotenv.load(fileName: envFile);
    } catch (e) {
      // If .env file not found, use default values (logger not available yet!)
      debugPrint(
        '⚠️ [EnvConfig] .env file not found ($envFile), using default values',
      );
      debugPrint('💡 [EnvConfig] Create a .env.dev file in the root directory');
    }
  }

  /// Validate critical production configuration and fail-fast if invalid
  static void validateOrThrow() {
    if (!isProduction) return;
    final errors = <String>[];
    if (apiBaseUrl.isEmpty || apiBaseUrl.startsWith('http') == false) {
      errors.add('API_BASE_URL is missing/invalid');
    }
    if (apiTimeout <= 0) {
      errors.add('API_TIMEOUT must be > 0');
    }
    if (apiKey.isEmpty || apiKey == 'dev_api_key_12345') {
      errors.add('API_KEY must be set for production');
    }
    if (encryptionKey.length < 32 || encryptionKey.contains('dev_')) {
      errors.add('ENCRYPTION_KEY must be a strong 32+ char key');
    }
    if (debugMode) {
      errors.add('DEBUG_MODE must be false in production');
    }
    if (enableSslPinning == false) {
      // Allow disabling only if explicitly acknowledged in pipeline
      debugPrint('⚠️ [EnvConfig] ENABLE_SSL_PINNING=false in PROD');
    }
    if (googleMapsApiKey.isEmpty ||
        googleMapsApiKey == 'your_google_maps_api_key') {
      errors.add('GOOGLE_MAPS_API_KEY must be set');
    }
    if (errors.isNotEmpty) {
      final message =
          'Production environment configuration errors:\n - ' +
          errors.join('\n - ');
      throw StateError(message);
    }
  }

  /// Get current environment
  static String get currentEnvironment => _currentEnvironment;

  /// Check if development mode
  static bool get isDevelopment => _currentEnvironment == dev;

  /// Check if staging mode
  static bool get isStaging => _currentEnvironment == staging;

  /// Check if production mode
  static bool get isProduction => _currentEnvironment == prod;

  /// Get API base URL
  static String get apiBaseUrl {
    try {
      return dotenv.get(_apiBaseUrlKey, fallback: _getDefaultApiBaseUrl());
    } catch (e) {
      return _getDefaultApiBaseUrl();
    }
  }

  /// Get API timeout (milliseconds)
  static int get apiTimeout {
    try {
      return int.tryParse(dotenv.get(_apiTimeoutKey, fallback: '30000')) ??
          30000;
    } catch (e) {
      return 30000;
    }
  }

  /// Get API key
  static String get apiKey {
    try {
      return dotenv.get(_apiKeyKey, fallback: 'dev_api_key_12345');
    } catch (e) {
      return 'dev_api_key_12345';
    }
  }

  /// Get encryption key for sensitive data
  static String get encryptionKey {
    try {
      return dotenv.get(
        _encryptionKeyKey,
        fallback: '32_char_encryption_key_for_dev_12345678',
      );
    } catch (e) {
      return '32_char_encryption_key_for_dev_12345678';
    }
  }

  /// Check if SSL pinning is enabled
  static bool get enableSslPinning {
    try {
      final value = dotenv.get(_enableSslPinningKey, fallback: 'false');
      return value.toLowerCase() == 'true';
    } catch (e) {
      return false;
    }
  }

  /// Check if debug mode is enabled
  static bool get debugMode {
    try {
      final value = dotenv.get(_debugModeKey, fallback: 'true');
      return value.toLowerCase() == 'true';
    } catch (e) {
      return true; // Default to true for development
    }
  }

  /// Get Google Maps API key
  static String get googleMapsApiKey {
    try {
      return dotenv.get(
        _googleMapsApiKeyKey,
        fallback: 'your_google_maps_api_key',
      );
    } catch (e) {
      return 'your_google_maps_api_key';
    }
  }

  /// Get default API base URL based on environment
  static String _getDefaultApiBaseUrl() {
    switch (_currentEnvironment) {
      case prod:
        return 'https://ajilgo.runasp.net';
      case staging:
        return 'https://ajilgo.runasp.net';
      case dev:
      default:
        return 'https://ajilgo.runasp.net'; // Backend URL
    }
  }

  /// Print current configuration (debug only)
  static void printConfig() {
    try {
      if (!debugMode) return;

      debugPrint('');
      debugPrint('═══════════════════════════════════════════════');
      debugPrint('📋 Environment Configuration');
      debugPrint('═══════════════════════════════════════════════');
      debugPrint('  Environment: $_currentEnvironment');
      debugPrint('  API Base URL: $apiBaseUrl');
      debugPrint('  API Timeout: ${apiTimeout}ms');
      debugPrint('  SSL Pinning: $enableSslPinning');
      debugPrint('  Debug Mode: $debugMode');
      debugPrint('  Google Maps: ${googleMapsApiKey.substring(0, 10)}...');
      debugPrint('═══════════════════════════════════════════════');
      debugPrint('');
    } catch (e) {
      // If .env not loaded, skip printing config
      debugPrint('⚠️ [EnvConfig] Cannot print config (likely .env not loaded)');
    }
  }
}
