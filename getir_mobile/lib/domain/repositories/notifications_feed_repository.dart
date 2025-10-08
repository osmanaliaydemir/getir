import '../../core/errors/result.dart';
import '../entities/notification.dart';

abstract class NotificationsFeedRepository {
  Future<Result<List<AppNotification>>> getNotifications({
    int page,
    int pageSize,
  });
  Future<Result<void>> markAsRead(String notificationId);
}
