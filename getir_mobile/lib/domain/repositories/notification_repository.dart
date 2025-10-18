import '../../core/errors/result.dart';
import '../entities/notification_preferences.dart';

abstract class INotificationRepository {
  Future<Result<NotificationPreferences>> getPreferences();
  Future<Result<NotificationPreferences>> updatePreferences(
    NotificationPreferences preferences,
  );
}
