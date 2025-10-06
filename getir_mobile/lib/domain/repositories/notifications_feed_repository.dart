import '../entities/notification.dart';

abstract class NotificationsFeedRepository {
  Future<List<AppNotification>> getNotifications({int page, int pageSize});
  Future<void> markAsRead(String notificationId);
}
