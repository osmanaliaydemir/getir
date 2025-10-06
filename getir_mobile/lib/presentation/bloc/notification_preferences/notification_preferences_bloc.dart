import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../domain/entities/notification_preferences.dart';
import '../../../domain/usecases/notification_usecases.dart';

part 'notification_preferences_event.dart';
part 'notification_preferences_state.dart';

class NotificationPreferencesBloc
    extends Bloc<NotificationPreferencesEvent, NotificationPreferencesState> {
  final GetNotificationPreferencesUseCase getUseCase;
  final UpdateNotificationPreferencesUseCase updateUseCase;

  NotificationPreferencesBloc({
    required this.getUseCase,
    required this.updateUseCase,
  }) : super(NotificationPreferencesInitial()) {
    on<LoadNotificationPreferences>((event, emit) async {
      emit(NotificationPreferencesLoading());
      try {
        final prefs = await getUseCase();
        emit(NotificationPreferencesLoaded(prefs));
      } catch (e) {
        emit(NotificationPreferencesError(e.toString()));
      }
    });

    on<UpdateNotificationPreferencesEvent>((event, emit) async {
      try {
        final updated = await updateUseCase(event.preferences);
        emit(NotificationPreferencesLoaded(updated));
      } catch (e) {
        emit(NotificationPreferencesError(e.toString()));
      }
    });
  }
}
