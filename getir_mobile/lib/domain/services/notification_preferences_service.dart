import '../../core/errors/result.dart';
import '../entities/notification_preferences.dart';
import '../repositories/notification_preferences_repository.dart';

/// Notification Preferences Service
///
/// Centralized service for notification preferences operations.
class NotificationPreferencesService {
  final INotificationPreferencesRepository _repository;

  const NotificationPreferencesService(this._repository);

  Future<Result<NotificationPreferences>> getNotificationPreferences() async {
    return await _repository.getNotificationPreferences();
  }

  Future<Result<NotificationPreferences>> updateNotificationPreferences(
    NotificationPreferences preferences,
  ) async {
    return await _repository.updateNotificationPreferences(preferences);
  }

  Future<Result<NotificationPreferences>> resetToDefaults() async {
    return await _repository.resetToDefaults();
  }
}
