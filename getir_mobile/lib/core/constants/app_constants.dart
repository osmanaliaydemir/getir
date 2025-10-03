class AppConstants {
  // App Info
  static const String appName = 'Getir Mobile';
  static const String appVersion = '1.0.0';
  static const String appBuildNumber = '1';
  
  // API Configuration
  static const String baseUrl = 'https://localhost:7000'; // Backend API URL
  static const String apiVersion = 'v1';
  static const String apiBaseUrl = '$baseUrl/api/$apiVersion';
  
  // Timeouts
  static const int connectTimeout = 30000; // 30 seconds
  static const int receiveTimeout = 30000; // 30 seconds
  static const int sendTimeout = 30000; // 30 seconds
  
  // Pagination
  static const int defaultPageSize = 20;
  static const int maxPageSize = 100;
  
  // Location
  static const double defaultLatitude = 41.0082; // İstanbul coordinates
  static const double defaultLongitude = 28.9784;
  static const double defaultSearchRadius = 10.0; // 10 km
  
  // Cache
  static const int cacheMaxAge = 300; // 5 minutes
  static const int imageCacheMaxAge = 86400; // 24 hours
  
  // File Upload
  static const int maxFileSize = 10 * 1024 * 1024; // 10 MB
  static const List<String> allowedImageTypes = ['jpg', 'jpeg', 'png', 'webp'];
  
  // Notification
  static const int notificationRetryCount = 3;
  static const Duration notificationRetryDelay = Duration(seconds: 5);
  
  // Security
  static const String jwtTokenKey = 'jwt_token';
  static const String refreshTokenKey = 'refresh_token';
  static const String userDataKey = 'user_data';
  
  // SharedPreferences Keys
  static const String isFirstLaunchKey = 'is_first_launch';
  static const String lastLocationKey = 'last_location';
  static const String notificationSettingsKey = 'notification_settings';
  static const String themeModeKey = 'theme_mode';
  static const String languageKey = 'language';
}
