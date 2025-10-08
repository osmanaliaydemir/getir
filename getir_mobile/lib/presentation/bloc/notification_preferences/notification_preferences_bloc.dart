import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/entities/notification_preferences.dart';
import '../../../domain/services/notification_service.dart';

part 'notification_preferences_event.dart';
part 'notification_preferences_state.dart';

class NotificationPreferencesBloc
    extends Bloc<NotificationPreferencesEvent, NotificationPreferencesState> {
  final NotificationService _notificationService;

  NotificationPreferencesBloc(this._notificationService)
    : super(NotificationPreferencesInitial()) {
    on<LoadNotificationPreferences>((event, emit) async {
      emit(NotificationPreferencesLoading());

      final result = await _notificationService.getPreferences();

      result.when(
        success: (prefs) => emit(NotificationPreferencesLoaded(prefs)),
        failure: (exception) {
          final message = _getErrorMessage(exception);
          emit(NotificationPreferencesError(message));
        },
      );
    });

    on<UpdateNotificationPreferencesEvent>((event, emit) async {
      final result = await _notificationService.updatePreferences(
        event.preferences,
      );

      result.when(
        success: (updated) => emit(NotificationPreferencesLoaded(updated)),
        failure: (exception) {
          final message = _getErrorMessage(exception);
          emit(NotificationPreferencesError(message));
        },
      );
    });
  }

  /// Extract user-friendly error message from exception
  String _getErrorMessage(Exception exception) {
    if (exception is AppException) {
      return exception.message;
    }
    return 'An unexpected error occurred';
  }
}
