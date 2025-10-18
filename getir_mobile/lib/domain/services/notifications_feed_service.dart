import '../../core/errors/result.dart';
import '../entities/notification.dart';
import '../repositories/notifications_feed_repository.dart';

class NotificationsFeedService {
  final INotificationsFeedRepository _repository;

  const NotificationsFeedService(this._repository);

  Future<Result<List<AppNotification>>> getNotifications({
    int page = 1,
    int pageSize = 20,
  }) async {
    return await _repository.getNotifications(page: page, pageSize: pageSize);
  }

  Future<Result<void>> markAsRead(String notificationId) async {
    return await _repository.markAsRead(notificationId);
  }
}
