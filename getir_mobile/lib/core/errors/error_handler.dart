import 'package:flutter/material.dart';
import 'package:dio/dio.dart';
import 'app_exceptions.dart';
import '../localization/app_localizations.dart';
import '../services/global_keys_service.dart';
import '../services/logger_service.dart';

/// Centralized error handler for the application
class ErrorHandler {
  static final ErrorHandler _instance = ErrorHandler._internal();
  factory ErrorHandler() => _instance;
  ErrorHandler._internal();

  /// Handle and log errors
  static void handleError(
    dynamic error,
    StackTrace? stackTrace, {
    String? context,
    Map<String, dynamic>? additionalData,
  }) {
    // Log error with context
    logger.error(
      context ?? 'Unhandled error',
      tag: 'ErrorHandler',
      error: error,
      stackTrace: stackTrace,
      context: additionalData,
    );

    // Log to crash reporting service (Firebase Crashlytics, etc.)
    _logToCrashReporting(error, stackTrace, context, additionalData);
  }

  /// Get user-friendly error message
  static String getUserFriendlyMessage(dynamic error, BuildContext context) {
    final l10n = AppLocalizations.of(context);

    if (error is AppException) {
      return _getLocalizedMessage(error, l10n);
    }

    if (error is DioException) {
      final appException = ExceptionFactory.fromDioError(error);
      return _getLocalizedMessage(appException, l10n);
    }

    // Generic error message
    return l10n.somethingWentWrong;
  }

  /// Get localized error message
  static String _getLocalizedMessage(
    AppException error,
    AppLocalizations l10n,
  ) {
    switch (error.runtimeType) {
      case NoInternetException:
        return l10n.noInternetConnection;
      case TimeoutException:
        return l10n.requestTimeout;
      case UnauthorizedException:
        return l10n.unauthorizedAccess;
      case ForbiddenException:
        return l10n.accessForbidden;
      case NotFoundException:
        return l10n.resourceNotFound;
      case ServerException:
        return l10n.serverError;
      case ValidationException:
        return error.message; // Use the specific validation message
      case InsufficientFundsException:
        return l10n.insufficientFunds;
      case ProductUnavailableException:
        return l10n.productUnavailable;
      case OrderLimitExceededException:
        return l10n.orderLimitExceeded;
      case StorageException:
        return l10n.storageError;
      case CacheException:
        return l10n.cacheError;
      default:
        return error.message.isNotEmpty
            ? error.message
            : l10n.somethingWentWrong;
    }
  }

  /// Show error snackbar
  static void showErrorSnackBar(
    BuildContext context,
    dynamic error, {
    Duration duration = const Duration(seconds: 4),
    VoidCallback? onRetry,
  }) {
    final message = getUserFriendlyMessage(error, context);

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Row(
          children: [
            const Icon(Icons.error_outline, color: Colors.white, size: 20),
            const SizedBox(width: 8),
            Expanded(child: Text(message)),
          ],
        ),
        backgroundColor: Colors.red[600],
        duration: duration,
        action: onRetry != null
            ? SnackBarAction(
                label: 'Retry',
                textColor: Colors.white,
                onPressed: onRetry,
              )
            : null,
      ),
    );
  }

  /// Show global error snackbar without BuildContext (uses scaffoldMessengerKey)
  static void showGlobalError(
    dynamic error, {
    Duration duration = const Duration(seconds: 4),
  }) {
    final navigatorContext = GlobalKeysService.navigatorKey.currentContext;
    if (navigatorContext == null) return;
    final message = getUserFriendlyMessage(error, navigatorContext);
    final messenger = GlobalKeysService.scaffoldMessengerKey.currentState;
    messenger?.showSnackBar(
      SnackBar(
        content: Row(
          children: [
            const Icon(Icons.error_outline, color: Colors.white, size: 20),
            const SizedBox(width: 8),
            Expanded(child: Text(message)),
          ],
        ),
        duration: duration,
        backgroundColor: Colors.red[600],
      ),
    );
  }

  /// Show error dialog
  static void showErrorDialog(
    BuildContext context,
    dynamic error, {
    String? title,
    VoidCallback? onRetry,
    VoidCallback? onDismiss,
  }) {
    final message = getUserFriendlyMessage(error, context);
    final l10n = AppLocalizations.of(context);

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Row(
          children: [
            const Icon(Icons.error_outline, color: Colors.red, size: 24),
            const SizedBox(width: 8),
            Text(title ?? l10n.error),
          ],
        ),
        content: Text(message),
        actions: [
          if (onDismiss != null)
            TextButton(
              onPressed: () {
                Navigator.of(context).pop();
                onDismiss();
              },
              child: Text(l10n.dismiss),
            ),
          if (onRetry != null)
            ElevatedButton(
              onPressed: () {
                Navigator.of(context).pop();
                onRetry();
              },
              child: Text(l10n.retry),
            ),
          if (onRetry == null && onDismiss == null)
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: Text(l10n.ok),
            ),
        ],
      ),
    );
  }

  /// Show network error with retry option
  static void showNetworkError(BuildContext context, VoidCallback onRetry) {
    final l10n = AppLocalizations.of(context);

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Row(
          children: [
            const Icon(Icons.wifi_off, color: Colors.orange, size: 24),
            const SizedBox(width: 8),
            Text(l10n.noInternetConnection),
          ],
        ),
        content: Text(l10n.checkInternetConnection),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: Text(l10n.dismiss),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.of(context).pop();
              onRetry();
            },
            child: Text(l10n.retry),
          ),
        ],
      ),
    );
  }

  /// Show validation errors
  static void showValidationErrors(
    BuildContext context,
    ValidationException error,
  ) {
    final l10n = AppLocalizations.of(context);

    if (error.validationErrors != null && error.validationErrors!.isNotEmpty) {
      final errorMessages = <String>[];

      error.validationErrors!.forEach((field, errors) {
        errorMessages.addAll(errors);
      });

      showDialog(
        context: context,
        builder: (context) => AlertDialog(
          title: Text(l10n.validationError),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: errorMessages
                .map(
                  (message) => Padding(
                    padding: const EdgeInsets.only(bottom: 4),
                    child: Text('• $message'),
                  ),
                )
                .toList(),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: Text(l10n.ok),
            ),
          ],
        ),
      );
    } else {
      showErrorDialog(context, error);
    }
  }

  /// Log to crash reporting service
  static void _logToCrashReporting(
    dynamic error,
    StackTrace? stackTrace,
    String? context,
    Map<String, dynamic>? additionalData,
  ) {
    // TODO: Implement Firebase Crashlytics or other crash reporting
    // FirebaseCrashlytics.instance.recordError(error, stackTrace, context: context);
  }

  /// Check if error is retryable
  static bool isRetryable(dynamic error) {
    if (error is NoInternetException) return true;
    if (error is TimeoutException) return true;
    if (error is ServerException) return true;
    if (error is NetworkException) return true;
    return false;
  }

  /// Get retry delay based on error type
  static Duration getRetryDelay(dynamic error, int attempt) {
    if (error is TimeoutException) {
      return Duration(seconds: 2 * attempt);
    }
    if (error is ServerException) {
      return Duration(seconds: 5 * attempt);
    }
    return Duration(seconds: attempt);
  }
}
