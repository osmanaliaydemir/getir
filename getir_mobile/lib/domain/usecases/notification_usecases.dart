import '../../core/errors/result.dart';
import '../entities/notification_preferences.dart';
import '../repositories/notification_repository.dart';

/// Get Notification Preferences Use Case
class GetNotificationPreferencesUseCase {
  final NotificationRepository repository;

  GetNotificationPreferencesUseCase(this.repository);

  Future<Result<NotificationPreferences>> call() async {
    return await repository.getPreferences();
  }
}

/// Update Notification Preferences Use Case
class UpdateNotificationPreferencesUseCase {
  final NotificationRepository repository;

  UpdateNotificationPreferencesUseCase(this.repository);

  Future<Result<NotificationPreferences>> call(
    NotificationPreferences preferences,
  ) async {
    return await repository.updatePreferences(preferences);
  }
}
