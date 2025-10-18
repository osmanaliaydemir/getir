import 'package:dio/dio.dart';
import '../../domain/entities/notification_preferences.dart';
import '../../core/network/api_response_parser.dart';

abstract class NotificationPreferencesDataSource {
  Future<NotificationPreferences> getNotificationPreferences();
  Future<NotificationPreferences> updateNotificationPreferences(
    NotificationPreferences preferences,
  );
  Future<NotificationPreferences> resetToDefaults();
}

class NotificationPreferencesDataSourceImpl
    implements NotificationPreferencesDataSource {
  final Dio _dio;
  NotificationPreferencesDataSourceImpl(this._dio);

  @override
  Future<NotificationPreferences> getNotificationPreferences() async {
    final response = await _dio.get('/api/v1/user/notification-preferences');

    print('📡 [NotificationPreferences] GET Response: ${response.data}');

    final result = ApiResponseParser.parse<NotificationPreferences>(
      responseData: response.data,
      fromJson: (json) => NotificationPreferences.fromJson(json),
      endpointName: 'getNotificationPreferences',
    );

    print('✅ [NotificationPreferences] Parsed successfully');
    return result;
  }

  @override
  Future<NotificationPreferences> updateNotificationPreferences(
    NotificationPreferences preferences,
  ) async {
    final updateData = preferences.toJson();

    print('📤 [NotificationPreferences] PUT Request: $updateData');

    final response = await _dio.put(
      '/api/v1/user/notification-preferences',
      data: updateData,
    );

    print('📡 [NotificationPreferences] PUT Response: ${response.data}');

    final result = ApiResponseParser.parse<NotificationPreferences>(
      responseData: response.data,
      fromJson: (json) => NotificationPreferences.fromJson(json),
      endpointName: 'updateNotificationPreferences',
    );

    print('✅ [NotificationPreferences] Update successful');
    return result;
  }

  @override
  Future<NotificationPreferences> resetToDefaults() async {
    final response = await _dio.post(
      '/api/v1/user/notification-preferences/reset',
    );

    return ApiResponseParser.parse<NotificationPreferences>(
      responseData: response.data,
      fromJson: (json) => NotificationPreferences.fromJson(json),
      endpointName: 'resetToDefaults',
    );
  }
}
