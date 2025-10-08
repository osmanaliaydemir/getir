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

    try {
      // Load .env file based on environment
      final envFile = environment == prod
          ? '.env.prod'
          : environment == staging
          ? '.env.staging'
          : '.env.dev';

      await dotenv.load(fileName: envFile);
    } catch (e) {
      // If .env file not found, use default values
      print('⚠️  Warning: .env file not found, using default values');
      print('   Create a .env.dev file in the root directory');
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
    return dotenv.get(_apiBaseUrlKey, fallback: _getDefaultApiBaseUrl());
  }

  /// Get API timeout (milliseconds)
  static int get apiTimeout {
    return int.tryParse(dotenv.get(_apiTimeoutKey, fallback: '30000')) ?? 30000;
  }

  /// Get API key
  static String get apiKey {
    return dotenv.get(_apiKeyKey, fallback: 'dev_api_key_12345');
  }

  /// Get encryption key for sensitive data
  static String get encryptionKey {
    return dotenv.get(
      _encryptionKeyKey,
      fallback: '32_char_encryption_key_for_dev_12345678',
    );
  }

  /// Check if SSL pinning is enabled
  static bool get enableSslPinning {
    final value = dotenv.get(_enableSslPinningKey, fallback: 'false');
    return value.toLowerCase() == 'true';
  }

  /// Check if debug mode is enabled
  static bool get debugMode {
    final value = dotenv.get(_debugModeKey, fallback: 'true');
    return value.toLowerCase() == 'true';
  }

  /// Get Google Maps API key
  static String get googleMapsApiKey {
    return dotenv.get(
      _googleMapsApiKeyKey,
      fallback: 'your_google_maps_api_key',
    );
  }

  /// Get default API base URL based on environment
  static String _getDefaultApiBaseUrl() {
    switch (_currentEnvironment) {
      case prod:
        return 'http://ajilgo.runasp.net';
      case staging:
        return 'http://ajilgo.runasp.net';
      case dev:
      default:
        return 'http://ajilgo.runasp.net'; // Backend URL
    }
  }

  /// Print current configuration (debug only)
  static void printConfig() {
    if (!debugMode) return;

    print('🔧 Environment Configuration:');
    print('   Environment: $_currentEnvironment');
    print('   API Base URL: $apiBaseUrl');
    print('   API Timeout: ${apiTimeout}ms');
    print('   SSL Pinning: $enableSslPinning');
    print('   Debug Mode: $debugMode');
    print('   Google Maps: ${googleMapsApiKey.substring(0, 10)}...');
  }
}
