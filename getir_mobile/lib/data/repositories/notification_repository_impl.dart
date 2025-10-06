import '../../domain/entities/notification_preferences.dart';
import '../../domain/repositories/notification_repository.dart';
import '../datasources/notification_preferences_datasource.dart';

class NotificationRepositoryImpl implements NotificationRepository {
  final NotificationPreferencesDataSource _ds;
  NotificationRepositoryImpl(this._ds);

  @override
  Future<NotificationPreferences> getPreferences() => _ds.getPreferences();

  @override
  Future<NotificationPreferences> updatePreferences(
    NotificationPreferences preferences,
  ) => _ds.updatePreferences(preferences);
}
