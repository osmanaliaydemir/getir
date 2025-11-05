import 'package:flutter/foundation.dart';
import 'package:hive_flutter/hive_flutter.dart';
import 'package:timeago/timeago.dart' as timeago;

import '../config/environment_config.dart';
import '../di/injection.dart';
import '../services/secure_encryption_service.dart';
import '../services/local_storage_service.dart';
import '../services/network_service.dart';
import '../services/sync_service.dart';
import '../services/api_cache_service.dart';
import '../services/analytics_service.dart';
import '../services/firebase_service.dart';
import '../services/performance_service.dart';
import '../interceptors/ssl_pinning_interceptor.dart';
import '../interceptors/cache_interceptor.dart';
import '../services/logger_service.dart';

/// AppInitializer
///
/// Centralized app initialization logic extracted from main.dart
/// Handles all app setup in a testable and maintainable way
class AppInitializer {
  AppInitializer._();

  /// Initialize the complete application
  ///
  /// This method orchestrates all initialization steps in the correct order:
  /// 1. Parallel initialization of independent services
  /// 2. Dependency injection setup
  /// 3. Sequential initialization of dependent services
  /// 4. Analytics & crash reporting setup
  static Future<void> initialize({
    String environment = EnvironmentConfig.dev,
  }) async {
    try {
      debugPrint('🚀 [AppInit] Starting app initialization');

      // 🚀 PHASE 1: Parallel initialization - Independent services
      await _initializeParallelServices(environment);

      // 🔧 PHASE 2: Dependency Injection setup (MUST be before logger usage!)
      await _setupDependencyInjection();

      // 🎯 PHASE 3: Sequential initialization - Dependent services
      await _initializeSequentialServices();

      // 📊 PHASE 4: Analytics & Crash Reporting
      await _setupAnalyticsAndCrashReporting();

      logger.info('App initialization completed successfully', tag: 'AppInit');
    } catch (e, stackTrace) {
      // Use debugPrint if logger not yet initialized
      debugPrint('❌ [AppInit] FATAL: App initialization failed - $e');
      debugPrint('StackTrace: $stackTrace');

      // Try to log if logger is available
      try {
        logger.fatal(
          'App initialization failed',
          tag: 'AppInit',
          error: e,
          stackTrace: stackTrace,
        );
      } catch (_) {
        // Logger not available yet, already printed above
      }
      rethrow;
    }
  }

  /// Phase 1: Initialize independent services in parallel
  static Future<void> _initializeParallelServices(String environment) async {
    debugPrint('📦 [AppInit] Phase 1: Initializing parallel services');

    await Future.wait([
      EnvironmentConfig.initialize(environment: environment),
      Hive.initFlutter(),
      FirebaseService.initialize(),
    ]);

    // Validate production environment early (fail-fast)
    try {
      EnvironmentConfig.validateOrThrow();
    } catch (e) {
      debugPrint('❌ [AppInit] Invalid production configuration: $e');
      rethrow;
    }

    // Print config in debug mode
    if (kDebugMode) {
      EnvironmentConfig.printConfig();
    }

    debugPrint('✅ [AppInit] Phase 1 completed: Parallel services initialized');
  }

  /// Phase 2: Setup dependency injection
  static Future<void> _setupDependencyInjection() async {
    debugPrint('🔧 [AppInit] Phase 2: Setting up Dependency Injection');

    await configureDependencies();

    debugPrint(
      '✅ [AppInit] Phase 2 completed: DI configured, logger now available',
    );
  }

  /// Phase 3: Initialize services that depend on DI
  static Future<void> _initializeSequentialServices() async {
    logger.debug('Phase 3: Initializing sequential services', tag: 'AppInit');

    // Core services (order matters due to dependencies)
    await getIt<SecureEncryptionService>().initialize();
    await getIt<LocalStorageService>().initialize();

    // Security & Caching
    await SslPinningInterceptor.initialize();
    await ApiCacheInterceptor.initialize();

    // Localization
    timeago.setLocaleMessages('tr', timeago.TrMessages());

    // Network & Sync services
    await NetworkService().initialize();
    await SyncService().initialize();
    await ApiCacheService().initialize();

    logger.debug(
      'Phase 3 completed: Sequential services initialized',
      tag: 'AppInit',
    );
  }

  /// Phase 4: Setup analytics and crash reporting
  static Future<void> _setupAnalyticsAndCrashReporting() async {
    logger.debug(
      'Phase 4: Setting up Analytics & Crash Reporting',
      tag: 'AppInit',
    );

    // Mark app start for performance tracking
    AppStartupTracker().markAppStart();

    // Enable Crashlytics
    final analytics = getIt<AnalyticsService>();
    await analytics.setCrashlyticsEnabled(true);

    // Setup global error handling
    _setupGlobalErrorHandling(analytics);

    logger.debug(
      'Phase 4 completed: Analytics & Crash Reporting configured',
      tag: 'AppInit',
    );
  }

  /// Setup global error handling for uncaught exceptions
  static void _setupGlobalErrorHandling(AnalyticsService analytics) {
    FlutterError.onError = (errorDetails) {
      logger.fatal(
        'Flutter Framework Error',
        tag: 'FlutterError',
        error: errorDetails.exception,
        stackTrace: errorDetails.stack,
      );

      analytics.logError(
        error: errorDetails.exception,
        stackTrace: errorDetails.stack,
        reason: 'Flutter Framework Error',
        fatal: true,
      );
    };
  }
}
