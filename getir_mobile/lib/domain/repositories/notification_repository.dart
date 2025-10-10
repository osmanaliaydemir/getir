import '../../core/errors/result.dart';
import '../entities/notification_preferences.dart';

abstract class NotificationRepository {
  Future<Result<NotificationPreferences>> getPreferences();
  Future<Result<NotificationPreferences>> updatePreferences(
    NotificationPreferences preferences,
  );
}
