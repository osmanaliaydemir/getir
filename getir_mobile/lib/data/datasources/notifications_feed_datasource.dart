import 'package:dio/dio.dart';
import '../models/notification_dto.dart';

abstract class NotificationsFeedDataSource {
  Future<List<AppNotificationDto>> getNotifications({int page, int pageSize});
  Future<void> markAsRead(String notificationId);
}

class NotificationsFeedDataSourceImpl implements NotificationsFeedDataSource {
  final Dio _dio;
  NotificationsFeedDataSourceImpl(this._dio);

  @override
  Future<List<AppNotificationDto>> getNotifications({
    int page = 1,
    int pageSize = 20,
  }) async {
    final response = await _dio.get(
      '/api/v1/notifications',
      queryParameters: {'page': page, 'pageSize': pageSize},
    );
    final data = response.data['data'] ?? response.data;
    final List list = (data is Map && data['items'] is List)
        ? data['items']
        : (data as List);
    return list
        .map((e) => AppNotificationDto.fromJson(Map<String, dynamic>.from(e)))
        .toList();
  }

  @override
  Future<void> markAsRead(String notificationId) async {
    await _dio.put('/api/v1/notifications/$notificationId/read');
  }
}
