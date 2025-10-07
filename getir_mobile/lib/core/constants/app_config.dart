import 'package:flutter/foundation.dart';

class AppConfig {
  static const bool _isDebug = kDebugMode;
  static const bool _isRelease = kReleaseMode;
  static const bool _isProfile = kProfileMode;

  // Environment
  static bool get isDebug => _isDebug;
  static bool get isRelease => _isRelease;
  static bool get isProfile => _isProfile;

  // API Configuration
  static String get apiBaseUrl {
    return 'http://ajilgo.runasp.net/api/v1'; // All environments use same URL
  }

  // SignalR Hub URLs
  static String get signalRUrl {
    return 'http://ajilgo.runasp.net/hubs'; // All environments use same URL
  }

  // Firebase Configuration
  static bool get enableFirebase => true;
  static bool get enableCrashlytics => isRelease;
  static bool get enableAnalytics => true;
  static bool get enablePerformanceMonitoring => isRelease;

  // Logging
  static bool get enableLogging => isDebug;
  static bool get enableNetworkLogging => isDebug;
  static bool get enableBlocLogging => isDebug;

  // Caching
  static bool get enableCaching => true;
  static bool get enableImageCaching => true;
  static bool get enableApiCaching => isRelease;

  // Features
  static bool get enableOfflineMode => true;
  static bool get enablePushNotifications => true;
  static bool get enableLocationServices => true;
  static bool get enableBiometricAuth => isRelease;

  // Development Features
  static bool get enableDeveloperOptions => isDebug;
  static bool get enableDebugOverlay => isDebug;
  static bool get enablePerformanceOverlay => isDebug;

  // App Store Configuration
  static String get appStoreId => '1234567890'; // iOS App Store ID
  static String get playStoreId => 'com.getir.mobile'; // Android Package Name

  // Social Media
  static String get instagramUrl => 'https://instagram.com/getir';
  static String get twitterUrl => 'https://twitter.com/getir';
  static String get facebookUrl => 'https://facebook.com/getir';
  static String get linkedinUrl => 'https://linkedin.com/company/getir';

  // Support
  static String get supportEmail => 'support@getir.com';
  static String get supportPhone => '+90 850 284 47 47';
  static String get supportWebsite => 'https://getir.com/support';

  // Legal
  static String get privacyPolicyUrl => 'https://getir.com/privacy';
  static String get termsOfServiceUrl => 'https://getir.com/terms';
  static String get cookiePolicyUrl => 'https://getir.com/cookies';

  // App Information
  static String get appName => 'Getir Mobile';
  static String get appDescription =>
      'Getir Clone - Flutter Mobile Application';
  static String get appVersion => '1.0.0';
  static int get appBuildNumber => 1;

  // Debug Information
  static Map<String, dynamic> get debugInfo => {
    'isDebug': isDebug,
    'isRelease': isRelease,
    'isProfile': isProfile,
    'apiBaseUrl': apiBaseUrl,
    'signalRUrl': signalRUrl,
    'enableLogging': enableLogging,
    'enableCaching': enableCaching,
    'enableOfflineMode': enableOfflineMode,
  };
}
