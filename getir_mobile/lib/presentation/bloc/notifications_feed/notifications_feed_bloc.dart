import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/entities/notification.dart';
import '../../../domain/repositories/notifications_feed_repository.dart';

part 'notifications_feed_event.dart';
part 'notifications_feed_state.dart';

class NotificationsFeedBloc
    extends Bloc<NotificationsFeedEvent, NotificationsFeedState> {
  final NotificationsFeedRepository repository;

  NotificationsFeedBloc({required this.repository})
    : super(NotificationsFeedInitial()) {
    on<LoadNotificationsFeed>(_onLoad);
    on<MarkNotificationRead>(_onMarkRead);
  }

  Future<void> _onLoad(
    LoadNotificationsFeed event,
    Emitter<NotificationsFeedState> emit,
  ) async {
    emit(NotificationsFeedLoading());

    final result = await repository.getNotifications(
      page: event.page,
      pageSize: event.pageSize,
    );

    result.when(
      success: (items) => emit(NotificationsFeedLoaded(items)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(NotificationsFeedError(message));
      },
    );
  }

  Future<void> _onMarkRead(
    MarkNotificationRead event,
    Emitter<NotificationsFeedState> emit,
  ) async {
    final result = await repository.markAsRead(event.notificationId);

    result.when(
      success: (_) {
        // Optimistic update if already loaded
        final current = state;
        if (current is NotificationsFeedLoaded) {
          final updated = current.items.map((n) {
            if (n.id == event.notificationId) {
              return AppNotification(
                id: n.id,
                title: n.title,
                body: n.body,
                type: n.type,
                createdAt: n.createdAt,
                isRead: true,
                data: n.data,
              );
            }
            return n;
          }).toList();
          emit(NotificationsFeedLoaded(updated));
        }
      },
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(NotificationsFeedError(message));
      },
    );
  }

  /// Extract user-friendly error message from exception
  String _getErrorMessage(Exception exception) {
    if (exception is AppException) {
      return exception.message;
    }
    return 'An unexpected error occurred';
  }
}
