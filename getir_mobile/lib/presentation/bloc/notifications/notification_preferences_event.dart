part of 'notification_preferences_bloc.dart';

abstract class NotificationPreferencesEvent extends Equatable {
  @override
  List<Object?> get props => [];
}

class LoadNotificationPreferences extends NotificationPreferencesEvent {}

class UpdateNotificationPreference extends NotificationPreferencesEvent {
  final String key;
  final bool value;

  UpdateNotificationPreference({required this.key, required this.value});

  @override
  List<Object?> get props => [key, value];
}

class ResetToDefaults extends NotificationPreferencesEvent {}
