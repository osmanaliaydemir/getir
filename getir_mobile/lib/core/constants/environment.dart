enum Environment { development, staging, production }

class EnvironmentConfig {
  static Environment _environment = Environment.development;

  static Environment get current => _environment;

  static void setEnvironment(Environment environment) {
    _environment = environment;
  }

  // API URLs based on environment
  static String get apiBaseUrl {
    switch (_environment) {
      case Environment.development:
        return 'http://ajilgo.runasp.net/api/v1';
      case Environment.staging:
        return 'http://ajilgo.runasp.net/api/v1';
      case Environment.production:
        return 'http://ajilgo.runasp.net/api/v1';
    }
  }

  // SignalR Hub URLs
  static String get signalRUrl {
    switch (_environment) {
      case Environment.development:
        return 'http://ajilgo.runasp.net/hubs';
      case Environment.staging:
        return 'http://ajilgo.runasp.net/hubs';
      case Environment.production:
        return 'http://ajilgo.runasp.net/hubs';
    }
  }

  // Firebase Project IDs
  static String get firebaseProjectId {
    switch (_environment) {
      case Environment.development:
        return 'getir-mobile-dev';
      case Environment.staging:
        return 'getir-mobile-staging';
      case Environment.production:
        return 'getir-mobile-prod';
    }
  }

  // App Configuration
  static bool get enableLogging {
    switch (_environment) {
      case Environment.development:
        return true;
      case Environment.staging:
        return true;
      case Environment.production:
        return false;
    }
  }

  static bool get enableCrashlytics {
    switch (_environment) {
      case Environment.development:
        return false;
      case Environment.staging:
        return true;
      case Environment.production:
        return true;
    }
  }

  static bool get enableAnalytics {
    switch (_environment) {
      case Environment.development:
        return false;
      case Environment.staging:
        return true;
      case Environment.production:
        return true;
    }
  }

  // Security
  static bool get enableCertificatePinning {
    switch (_environment) {
      case Environment.development:
        return false;
      case Environment.staging:
        return true;
      case Environment.production:
        return true;
    }
  }

  // Feature Flags
  static bool get enableExperimentalFeatures {
    switch (_environment) {
      case Environment.development:
        return true;
      case Environment.staging:
        return true;
      case Environment.production:
        return false;
    }
  }

  // Debug Information
  static Map<String, dynamic> get debugInfo => {
    'environment': _environment.name,
    'apiBaseUrl': apiBaseUrl,
    'signalRUrl': signalRUrl,
    'firebaseProjectId': firebaseProjectId,
    'enableLogging': enableLogging,
    'enableCrashlytics': enableCrashlytics,
    'enableAnalytics': enableAnalytics,
    'enableCertificatePinning': enableCertificatePinning,
    'enableExperimentalFeatures': enableExperimentalFeatures,
  };
}
