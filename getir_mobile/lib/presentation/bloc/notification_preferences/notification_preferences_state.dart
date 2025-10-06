part of 'notification_preferences_bloc.dart';

abstract class NotificationPreferencesState extends Equatable {
  @override
  List<Object?> get props => [];
}

class NotificationPreferencesInitial extends NotificationPreferencesState {}

class NotificationPreferencesLoading extends NotificationPreferencesState {}

class NotificationPreferencesLoaded extends NotificationPreferencesState {
  final NotificationPreferences preferences;
  NotificationPreferencesLoaded(this.preferences);

  @override
  List<Object?> get props => [preferences];
}

class NotificationPreferencesError extends NotificationPreferencesState {
  final String message;
  NotificationPreferencesError(this.message);

  @override
  List<Object?> get props => [message];
}
