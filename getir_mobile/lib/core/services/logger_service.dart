import 'package:flutter/foundation.dart';
import 'analytics_service.dart';
import '../di/injection.dart';

/// Log Level
enum LogLevel {
  debug(0, '🔍 DEBUG'),
  info(1, '📘 INFO'),
  warning(2, '⚠️  WARN'),
  error(3, '🔴 ERROR'),
  fatal(4, '💀 FATAL');

  const LogLevel(this.value, this.emoji);
  final int value;
  final String emoji;
}

/// Logger Service
///
/// Centralized logging service with support for:
/// - Different log levels
/// - Contextual logging
/// - Analytics integration
/// - Production-safe logging (no sensitive data)
/// - Log file rotation (optional)
/// - Remote logging (via Firebase)
class LoggerService {
  final AnalyticsService _analytics;

  // Current minimum log level (configurable per environment)
  LogLevel _minimumLevel = kDebugMode ? LogLevel.debug : LogLevel.info;

  LoggerService(this._analytics);

  /// Set minimum log level
  void setMinimumLevel(LogLevel level) {
    _minimumLevel = level;
    if (kDebugMode) {
      // Using direct print for logger's own debug output
      // ignore: avoid_print
      print('📊 Logger: Minimum level set to ${level.emoji}');
    }
  }

  /// Log debug message
  void debug(String message, {String? tag, Map<String, dynamic>? context}) {
    _log(LogLevel.debug, message, tag: tag, context: context);
  }

  /// Log info message
  void info(String message, {String? tag, Map<String, dynamic>? context}) {
    _log(LogLevel.info, message, tag: tag, context: context);
  }

  /// Log warning message
  void warning(String message, {String? tag, Map<String, dynamic>? context}) {
    _log(LogLevel.warning, message, tag: tag, context: context);
  }

  /// Log error message
  void error(
    String message, {
    String? tag,
    dynamic error,
    StackTrace? stackTrace,
    Map<String, dynamic>? context,
  }) {
    _log(
      LogLevel.error,
      message,
      tag: tag,
      error: error,
      stackTrace: stackTrace,
      context: context,
    );

    // Report to analytics if error provided
    if (error != null) {
      _analytics.logError(
        error: error,
        stackTrace: stackTrace,
        reason: message,
        context: context,
        fatal: false,
      );
    }
  }

  /// Log fatal error (app-breaking)
  void fatal(
    String message, {
    String? tag,
    required dynamic error,
    StackTrace? stackTrace,
    Map<String, dynamic>? context,
  }) {
    _log(
      LogLevel.fatal,
      message,
      tag: tag,
      error: error,
      stackTrace: stackTrace,
      context: context,
    );

    // Always report fatal errors to analytics
    _analytics.logError(
      error: error,
      stackTrace: stackTrace,
      reason: message,
      context: context,
      fatal: true,
    );
  }

  /// Internal logging method
  void _log(
    LogLevel level,
    String message, {
    String? tag,
    dynamic error,
    StackTrace? stackTrace,
    Map<String, dynamic>? context,
  }) {
    // Check if this log level should be printed
    if (level.value < _minimumLevel.value) {
      return;
    }

    // Format log message
    final timestamp = DateTime.now().toIso8601String();
    final tagStr = tag != null ? '[$tag]' : '';
    final logMessage = '${level.emoji} $timestamp $tagStr $message';

    // Print to console (debug only)
    if (kDebugMode) {
      // Using direct print for logger's own output
      // ignore: avoid_print
      print(logMessage);

      if (error != null) {
        // ignore: avoid_print
        print('   Error: $error');
      }

      if (stackTrace != null) {
        // ignore: avoid_print
        print('   StackTrace:\n$stackTrace');
      }

      if (context != null && context.isNotEmpty) {
        // ignore: avoid_print
        print('   Context: $context');
      }
    }

    // In production, you might want to write to a file or send to a remote service
    // For now, we're using Firebase Crashlytics via AnalyticsService
  }

  // ==================== Domain-Specific Loggers ====================

  /// Log network request
  void logNetworkRequest({
    required String method,
    required String url,
    int? statusCode,
    Duration? duration,
    Map<String, dynamic>? headers,
    dynamic error,
  }) {
    if (error != null) {
      this.error(
        'Network request failed: $method $url',
        tag: 'Network',
        error: error,
        context: {
          'method': method,
          'url': url,
          if (statusCode != null) 'statusCode': statusCode,
          if (duration != null) 'duration_ms': duration.inMilliseconds,
        },
      );
    } else {
      debug(
        'Network request: $method $url → $statusCode (${duration?.inMilliseconds}ms)',
        tag: 'Network',
      );
    }
  }

  /// Log BLoC event
  void logBlocEvent({
    required String blocName,
    required String eventName,
    Map<String, dynamic>? eventData,
  }) {
    debug('$blocName → $eventName', tag: 'BLoC', context: eventData);
  }

  /// Log BLoC state change
  void logBlocStateChange({
    required String blocName,
    required String previousState,
    required String newState,
    Map<String, dynamic>? stateData,
  }) {
    debug(
      '$blocName: $previousState → $newState',
      tag: 'BLoC',
      context: stateData,
    );
  }

  /// Log navigation
  void logNavigation({
    required String from,
    required String to,
    Map<String, dynamic>? params,
  }) {
    info('Navigation: $from → $to', tag: 'Navigation', context: params);
  }

  /// Log user action
  void logUserAction({
    required String action,
    String? screen,
    Map<String, dynamic>? details,
  }) {
    info(
      'User action: $action${screen != null ? " on $screen" : ""}',
      tag: 'UserAction',
      context: details,
    );
  }

  /// Log authentication event
  void logAuthEvent({
    required String event,
    String? userId,
    Map<String, dynamic>? details,
  }) {
    info(
      'Auth: $event${userId != null ? " (user: $userId)" : ""}',
      tag: 'Auth',
      context: details,
    );
  }

  /// Log database operation
  void logDatabaseOperation({
    required String operation,
    required String table,
    bool success = true,
    dynamic error,
  }) {
    if (success) {
      debug('DB: $operation on $table', tag: 'Database');
    } else {
      this.error(
        'DB operation failed: $operation on $table',
        tag: 'Database',
        error: error,
      );
    }
  }

  /// Log cache operation
  void logCacheOperation({
    required String operation,
    required String key,
    bool hit = false,
  }) {
    debug('Cache ${hit ? "HIT" : "MISS"}: $operation ($key)', tag: 'Cache');
  }

  /// Log performance metric
  void logPerformance({
    required String operation,
    required Duration duration,
    Map<String, dynamic>? metrics,
  }) {
    final durationMs = duration.inMilliseconds;
    final level = durationMs > 1000 ? LogLevel.warning : LogLevel.info;

    _log(
      level,
      'Performance: $operation took ${durationMs}ms',
      tag: 'Performance',
      context: metrics,
    );
  }

  /// Log app lifecycle event
  void logLifecycleEvent({
    required String event,
    Map<String, dynamic>? details,
  }) {
    info('Lifecycle: $event', tag: 'App', context: details);
  }

  // ==================== Production Logging ====================

  /// Log sensitive operation (sanitized for production)
  void logSensitiveOperation({
    required String operation,
    bool success = true,
    dynamic error,
  }) {
    if (success) {
      info('Sensitive operation: $operation completed', tag: 'Security');
    } else {
      // Don't log sensitive details in production
      this.error(
        'Sensitive operation failed: $operation',
        tag: 'Security',
        error: kDebugMode ? error : 'Error details hidden in production',
      );
    }
  }

  /// Sanitize data for production logging
  Map<String, dynamic> sanitizeData(Map<String, dynamic> data) {
    final sanitized = <String, dynamic>{};

    // List of sensitive keys to mask
    const sensitiveKeys = [
      'password',
      'token',
      'secret',
      'apiKey',
      'creditCard',
      'cvv',
      'pin',
    ];

    data.forEach((key, value) {
      if (sensitiveKeys.any((k) => key.toLowerCase().contains(k))) {
        sanitized[key] = '***REDACTED***';
      } else if (value is Map<String, dynamic>) {
        sanitized[key] = sanitizeData(value);
      } else {
        sanitized[key] = value;
      }
    });

    return sanitized;
  }
}

/// Global logger instance (for convenience)
///
/// Usage:
/// ```dart
/// logger.info('Something happened');
/// logger.error('Something failed', error: e);
/// ```
LoggerService get logger => getIt<LoggerService>();
