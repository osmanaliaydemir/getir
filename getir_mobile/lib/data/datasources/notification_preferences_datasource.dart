import 'package:dio/dio.dart';
import '../../domain/entities/notification_preferences.dart';

abstract class NotificationPreferencesDataSource {
  Future<NotificationPreferences> getPreferences();
  Future<NotificationPreferences> updatePreferences(
    NotificationPreferences preferences,
  );
}

class NotificationPreferencesDataSourceImpl
    implements NotificationPreferencesDataSource {
  final Dio _dio;
  NotificationPreferencesDataSourceImpl(this._dio);

  @override
  Future<NotificationPreferences> getPreferences() async {
    final response = await _dio.get('/api/v1/notifications/preferences');
    final data = response.data['data'] ?? response.data;
    return NotificationPreferences.fromJson(data as Map<String, dynamic>);
  }

  @override
  Future<NotificationPreferences> updatePreferences(
    NotificationPreferences preferences,
  ) async {
    final response = await _dio.put(
      '/api/v1/notifications/preferences',
      data: preferences.toJson(),
    );
    final data = response.data['data'] ?? response.data;
    return NotificationPreferences.fromJson(data as Map<String, dynamic>);
  }
}
