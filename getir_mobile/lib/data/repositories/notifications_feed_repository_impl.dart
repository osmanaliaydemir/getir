import '../../domain/entities/notification.dart';
import '../../domain/repositories/notifications_feed_repository.dart';
import '../datasources/notifications_feed_datasource.dart';

class NotificationsFeedRepositoryImpl implements NotificationsFeedRepository {
  final NotificationsFeedDataSource _ds;
  NotificationsFeedRepositoryImpl(this._ds);

  @override
  Future<List<AppNotification>> getNotifications({
    int page = 1,
    int pageSize = 20,
  }) async {
    final dtos = await _ds.getNotifications(page: page, pageSize: pageSize);
    return dtos.map((e) => e.toDomain()).toList();
  }

  @override
  Future<void> markAsRead(String notificationId) {
    return _ds.markAsRead(notificationId);
  }
}
