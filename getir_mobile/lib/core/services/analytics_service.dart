import 'package:flutter/foundation.dart';

class AnalyticsService {
  static final AnalyticsService _instance = AnalyticsService._internal();
  factory AnalyticsService() => _instance;
  AnalyticsService._internal();

  Future<void> initialize() async {
    if (kDebugMode) {
      debugPrint('AnalyticsService initialized');
    }
  }

  Future<void> logScreenView({required String screenName}) async {
    if (kDebugMode) {
      debugPrint('[Analytics] screen_view: $screenName');
    }
  }

  Future<void> logEvent(
    String name, {
    Map<String, Object?> parameters = const {},
  }) async {
    if (kDebugMode) {
      debugPrint(
        '[Analytics] $name ${parameters.isEmpty ? '' : parameters.toString()}',
      );
    }
  }
}
