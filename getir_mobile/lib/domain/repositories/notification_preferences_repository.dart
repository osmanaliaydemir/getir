import '../../core/errors/result.dart';
import '../entities/notification_preferences.dart';

abstract class INotificationPreferencesRepository {
  Future<Result<NotificationPreferences>> getNotificationPreferences();
  Future<Result<NotificationPreferences>> updateNotificationPreferences(
    NotificationPreferences preferences,
  );
  Future<Result<NotificationPreferences>> resetToDefaults();
}
