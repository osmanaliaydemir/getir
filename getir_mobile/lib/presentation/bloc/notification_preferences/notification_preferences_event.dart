part of 'notification_preferences_bloc.dart';

abstract class NotificationPreferencesEvent extends Equatable {
  @override
  List<Object?> get props => [];
}

class LoadNotificationPreferences extends NotificationPreferencesEvent {}

class UpdateNotificationPreferencesEvent extends NotificationPreferencesEvent {
  final NotificationPreferences preferences;
  UpdateNotificationPreferencesEvent(this.preferences);

  @override
  List<Object?> get props => [preferences];
}
