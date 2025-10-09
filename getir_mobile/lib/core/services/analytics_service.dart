import 'package:firebase_analytics/firebase_analytics.dart';
import 'package:firebase_crashlytics/firebase_crashlytics.dart';
import 'package:firebase_performance/firebase_performance.dart';
import 'package:flutter/foundation.dart';

/// Analytics Service
/// 
/// Centralized analytics, error tracking, and performance monitoring service
/// using Firebase Analytics, Crashlytics, and Performance Monitoring.
/// 
/// Features:
/// - Screen view tracking (automatic)
/// - User action tracking (clicks, interactions)
/// - Conversion funnel tracking (browse → cart → purchase)
/// - Error tracking with context
/// - Performance monitoring for critical operations
/// - User properties and demographics
class AnalyticsService {
  final FirebaseAnalytics _analytics;
  final FirebaseCrashlytics _crashlytics;
  final FirebasePerformance _performance;

  AnalyticsService(
    this._analytics,
    this._crashlytics,
    this._performance,
  );

  /// Get analytics observer for automatic screen tracking
  FirebaseAnalyticsObserver get observer {
    return FirebaseAnalyticsObserver(analytics: _analytics);
  }

  // ==================== Screen Tracking ====================

  /// Log screen view
  /// 
  /// Automatically called by FirebaseAnalyticsObserver,
  /// but can be called manually for custom screens.
  Future<void> logScreenView({
    required String screenName,
    String? screenClass,
    Map<String, dynamic>? parameters,
  }) async {
    // Screen view is already logged by Firebase Analytics
    // No additional debug logging needed

    await _analytics.logScreenView(
      screenName: screenName,
      screenClass: screenClass ?? screenName,
    );

    if (parameters != null) {
      await _analytics.logEvent(
        name: 'screen_view_details',
        parameters: parameters,
      );
    }
  }

  // ==================== User Actions ====================

  /// Log button click
  Future<void> logButtonClick({
    required String buttonName,
    String? screenName,
    Map<String, dynamic>? parameters,
  }) async {
    await _logEvent(
      'button_click',
      parameters: {
        'button_name': buttonName,
        if (screenName != null) 'screen_name': screenName,
        ...?parameters,
      },
    );
  }

  /// Log search action
  Future<void> logSearch({
    required String searchTerm,
    String? searchType,
    int? resultCount,
  }) async {
    await _analytics.logSearch(
      searchTerm: searchTerm,
      numberOfNights: null,
      numberOfRooms: null,
      numberOfPassengers: null,
      origin: null,
      destination: null,
      startDate: null,
      endDate: null,
      travelClass: null,
    );

    await _logEvent(
      'search_details',
      parameters: {
        'search_term': searchTerm,
        if (searchType != null) 'search_type': searchType,
        if (resultCount != null) 'result_count': resultCount,
      },
    );
  }

  /// Log product view
  Future<void> logProductView({
    required String productId,
    required String productName,
    String? category,
    double? price,
    String? currency,
  }) async {
    await _analytics.logViewItem(
      currency: currency ?? 'TRY',
      value: price,
      items: [
        AnalyticsEventItem(
          itemId: productId,
          itemName: productName,
          itemCategory: category,
          price: price,
        ),
      ],
    );
  }

  /// Log add to cart
  Future<void> logAddToCart({
    required String productId,
    required String productName,
    required double price,
    String? category,
    int quantity = 1,
    String? currency,
  }) async {
    await _analytics.logAddToCart(
      currency: currency ?? 'TRY',
      value: price * quantity,
      items: [
        AnalyticsEventItem(
          itemId: productId,
          itemName: productName,
          itemCategory: category,
          price: price,
          quantity: quantity,
        ),
      ],
    );
  }

  /// Log remove from cart
  Future<void> logRemoveFromCart({
    required String productId,
    required String productName,
    required double price,
    int quantity = 1,
  }) async {
    await _analytics.logRemoveFromCart(
      currency: 'TRY',
      value: price * quantity,
      items: [
        AnalyticsEventItem(
          itemId: productId,
          itemName: productName,
          price: price,
          quantity: quantity,
        ),
      ],
    );
  }

  /// Log add to favorites
  Future<void> logAddToFavorites({
    required String itemId,
    required String itemName,
    String? itemType,
  }) async {
    await _logEvent(
      'add_to_favorites',
      parameters: {
        'item_id': itemId,
        'item_name': itemName,
        if (itemType != null) 'item_type': itemType,
      },
    );
  }

  // ==================== Conversion Funnel ====================

  /// Log begin checkout
  Future<void> logBeginCheckout({
    required double value,
    required String currency,
    List<AnalyticsEventItem>? items,
    String? coupon,
  }) async {
    await _analytics.logBeginCheckout(
      value: value,
      currency: currency,
      items: items,
      coupon: coupon,
    );
  }

  /// Log add payment info
  Future<void> logAddPaymentInfo({
    required String paymentType,
    required double value,
    String? currency,
  }) async {
    await _analytics.logAddPaymentInfo(
      currency: currency ?? 'TRY',
      value: value,
      paymentType: paymentType,
    );
  }

  /// Log purchase
  Future<void> logPurchase({
    required String orderId,
    required double total,
    required String currency,
    List<AnalyticsEventItem>? items,
    double? tax,
    double? shipping,
    String? coupon,
  }) async {
    await _analytics.logPurchase(
      currency: currency,
      value: total,
      transactionId: orderId,
      tax: tax,
      shipping: shipping,
      items: items,
      coupon: coupon,
    );
  }

  /// Log order cancelled
  Future<void> logOrderCancelled({
    required String orderId,
    required double value,
    String? reason,
  }) async {
    await _logEvent(
      'order_cancelled',
      parameters: {
        'order_id': orderId,
        'value': value,
        if (reason != null) 'reason': reason,
      },
    );
  }

  // ==================== Authentication ====================

  /// Log login
  Future<void> logLogin({
    String? method,
  }) async {
    await _analytics.logLogin(loginMethod: method ?? 'email');
  }

  /// Log sign up
  Future<void> logSignUp({
    String? method,
  }) async {
    await _analytics.logSignUp(signUpMethod: method ?? 'email');
  }

  /// Log logout
  Future<void> logLogout() async {
    await _logEvent('logout');
  }

  // ==================== Error Tracking ====================

  /// Log error with context
  Future<void> logError({
    required dynamic error,
    StackTrace? stackTrace,
    String? reason,
    Map<String, dynamic>? context,
    bool fatal = false,
  }) async {
    // Error logging is already handled by logger service
    // No additional debug logging needed

    // Log to Crashlytics
    await _crashlytics.recordError(
      error,
      stackTrace,
      reason: reason,
      fatal: fatal,
      information: context?.entries.map((e) => '${e.key}: ${e.value}').toList() ?? [],
    );

    // Log to Analytics
    await _logEvent(
      'app_error',
      parameters: {
        'error_type': error.runtimeType.toString(),
        'error_message': error.toString(),
        if (reason != null) 'reason': reason,
        'fatal': fatal,
        ...?context,
      },
    );
  }

  /// Set user context for error tracking
  Future<void> setErrorContext({
    String? userId,
    String? screenName,
    Map<String, dynamic>? customKeys,
  }) async {
    if (userId != null) {
      await _crashlytics.setUserIdentifier(userId);
    }

    if (customKeys != null) {
      for (final entry in customKeys.entries) {
        await _crashlytics.setCustomKey(entry.key, entry.value);
      }
    }
  }

  // ==================== Performance Monitoring ====================

  /// Start a performance trace
  Future<Trace> startTrace(String traceName) async {
    final trace = _performance.newTrace(traceName);
    await trace.start();
    return trace;
  }

  /// Stop a performance trace
  Future<void> stopTrace(Trace trace) async {
    await trace.stop();
  }

  /// Measure performance of an operation
  Future<T> measurePerformance<T>({
    required String traceName,
    required Future<T> Function() operation,
    Map<String, String>? attributes,
  }) async {
    final trace = await startTrace(traceName);

    if (attributes != null) {
      for (final entry in attributes.entries) {
        trace.putAttribute(entry.key, entry.value);
      }
    }

    try {
      final result = await operation();
      await stopTrace(trace);
      return result;
    } catch (e) {
      await stopTrace(trace);
      rethrow;
    }
  }

  // ==================== User Properties ====================

  /// Set user ID
  Future<void> setUserId(String? userId) async {
    await _analytics.setUserId(id: userId);
    if (userId != null) {
      await _crashlytics.setUserIdentifier(userId);
    }
  }

  /// Set user property
  Future<void> setUserProperty({
    required String name,
    required String? value,
  }) async {
    await _analytics.setUserProperty(name: name, value: value);
  }

  /// Set user demographics
  Future<void> setUserDemographics({
    int? age,
    String? gender,
    String? interests,
  }) async {
    if (age != null) {
      await setUserProperty(name: 'age_group', value: _getAgeGroup(age));
    }
    if (gender != null) {
      await setUserProperty(name: 'gender', value: gender);
    }
    if (interests != null) {
      await setUserProperty(name: 'interests', value: interests);
    }
  }

  // ==================== Custom Events ====================

  /// Log custom event
  Future<void> logCustomEvent({
    required String eventName,
    Map<String, dynamic>? parameters,
  }) async {
    await _logEvent(eventName, parameters: parameters);
  }

  // ==================== Helper Methods ====================

  /// Internal event logging with debug output
  Future<void> _logEvent(
    String eventName, {
    Map<String, dynamic>? parameters,
  }) async {
    // Custom events are already logged by Firebase Analytics
    // No additional debug logging needed

    await _analytics.logEvent(
      name: eventName,
      parameters: parameters,
    );
  }

  /// Get age group from age
  String _getAgeGroup(int age) {
    if (age < 18) return '0-17';
    if (age < 25) return '18-24';
    if (age < 35) return '25-34';
    if (age < 45) return '35-44';
    if (age < 55) return '45-54';
    if (age < 65) return '55-64';
    return '65+';
  }

  // ==================== Batch Operations ====================

  /// Set analytics collection enabled
  Future<void> setAnalyticsEnabled(bool enabled) async {
    await _analytics.setAnalyticsCollectionEnabled(enabled);
  }

  /// Set crashlytics collection enabled
  Future<void> setCrashlyticsEnabled(bool enabled) async {
    await _crashlytics.setCrashlyticsCollectionEnabled(enabled);
  }

  /// Reset analytics data
  Future<void> resetAnalyticsData() async {
    await _analytics.resetAnalyticsData();
  }
}