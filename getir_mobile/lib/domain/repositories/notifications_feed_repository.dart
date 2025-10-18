import '../../core/errors/result.dart';
import '../entities/notification.dart';

abstract class INotificationsFeedRepository {
  Future<Result<List<AppNotification>>> getNotifications({
    int page,
    int pageSize,
  });
  Future<Result<void>> markAsRead(String notificationId);
}
