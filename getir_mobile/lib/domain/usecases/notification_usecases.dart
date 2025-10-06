import '../entities/notification_preferences.dart';
import '../repositories/notification_repository.dart';

class GetNotificationPreferencesUseCase {
  final NotificationRepository repository;
  GetNotificationPreferencesUseCase(this.repository);
  Future<NotificationPreferences> call() => repository.getPreferences();
}

class UpdateNotificationPreferencesUseCase {
  final NotificationRepository repository;
  UpdateNotificationPreferencesUseCase(this.repository);
  Future<NotificationPreferences> call(NotificationPreferences preferences) =>
      repository.updatePreferences(preferences);
}
