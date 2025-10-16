import '../../core/errors/result.dart';
import '../entities/notification_preferences.dart';
import '../repositories/notification_repository.dart';

/// Notification Service
///
/// Centralized service for notification preferences.
/// Replaces 2 separate UseCase classes.
class NotificationService {
  final INotificationRepository _repository;

  const NotificationService(this._repository);

  Future<Result<NotificationPreferences>> getPreferences() async {
    return await _repository.getPreferences();
  }

  Future<Result<NotificationPreferences>> updatePreferences(
    NotificationPreferences preferences,
  ) async {
    return await _repository.updatePreferences(preferences);
  }
}
