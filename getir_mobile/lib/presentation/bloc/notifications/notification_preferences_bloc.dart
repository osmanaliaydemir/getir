import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/entities/notification_preferences.dart';
import '../../../domain/services/notification_preferences_service.dart';

part 'notification_preferences_event.dart';
part 'notification_preferences_state.dart';

class NotificationPreferencesBloc
    extends Bloc<NotificationPreferencesEvent, NotificationPreferencesState> {
  final NotificationPreferencesService _notificationPreferencesService;

  NotificationPreferencesBloc(this._notificationPreferencesService)
    : super(NotificationPreferencesInitial()) {
    on<LoadNotificationPreferences>((event, emit) async {
      emit(NotificationPreferencesLoading());

      final result = await _notificationPreferencesService
          .getNotificationPreferences();

      result.when(
        success: (preferences) =>
            emit(NotificationPreferencesLoaded(preferences)),
        failure: (exception) {
          final message = _getErrorMessage(exception);
          emit(NotificationPreferencesError(message));
        },
      );
    });

    on<UpdateNotificationPreference>((event, emit) async {
      final currentState = state;
      if (currentState is NotificationPreferencesLoaded) {
        // Optimistically update the UI
        final updatedPreferences = _updatePreference(
          currentState.preferences,
          event.key,
          event.value,
        );
        emit(NotificationPreferencesLoaded(updatedPreferences));

        // Update on server
        final result = await _notificationPreferencesService
            .updateNotificationPreferences(updatedPreferences);

        result.when(
          success: (preferences) =>
              emit(NotificationPreferencesUpdated(preferences)),
          failure: (exception) {
            final message = _getErrorMessage(exception);
            emit(NotificationPreferencesError(message));
            // Revert to previous state on error
            emit(NotificationPreferencesLoaded(currentState.preferences));
          },
        );
      }
    });

    on<ResetToDefaults>((event, emit) async {
      final result = await _notificationPreferencesService.resetToDefaults();

      result.when(
        success: (preferences) =>
            emit(NotificationPreferencesUpdated(preferences)),
        failure: (exception) {
          final message = _getErrorMessage(exception);
          emit(NotificationPreferencesError(message));
        },
      );
    });
  }

  NotificationPreferences _updatePreference(
    NotificationPreferences preferences,
    String key,
    bool value,
  ) {
    switch (key) {
      case 'emailEnabled':
        return preferences.copyWith(emailEnabled: value);
      case 'emailOrderUpdates':
        return preferences.copyWith(emailOrderUpdates: value);
      case 'emailPromotions':
        return preferences.copyWith(emailPromotions: value);
      case 'emailNewsletter':
        return preferences.copyWith(emailNewsletter: value);
      case 'emailSecurityAlerts':
        return preferences.copyWith(emailSecurityAlerts: value);
      case 'smsEnabled':
        return preferences.copyWith(smsEnabled: value);
      case 'smsOrderUpdates':
        return preferences.copyWith(smsOrderUpdates: value);
      case 'smsPromotions':
        return preferences.copyWith(smsPromotions: value);
      case 'smsSecurityAlerts':
        return preferences.copyWith(smsSecurityAlerts: value);
      case 'pushEnabled':
        return preferences.copyWith(pushEnabled: value);
      case 'pushOrderUpdates':
        return preferences.copyWith(pushOrderUpdates: value);
      case 'pushPromotions':
        return preferences.copyWith(pushPromotions: value);
      case 'pushMerchantUpdates':
        return preferences.copyWith(pushMerchantUpdates: value);
      case 'pushSecurityAlerts':
        return preferences.copyWith(pushSecurityAlerts: value);
      default:
        return preferences;
    }
  }

  /// Extract user-friendly error message from exception
  String _getErrorMessage(Exception exception) {
    if (exception is AppException) {
      return exception.message;
    }
    return 'An unexpected error occurred';
  }
}
