import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
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
    try {
      final items = await repository.getNotifications(
        page: event.page,
        pageSize: event.pageSize,
      );
      emit(NotificationsFeedLoaded(items));
    } catch (e) {
      emit(NotificationsFeedError(e.toString()));
    }
  }

  Future<void> _onMarkRead(
    MarkNotificationRead event,
    Emitter<NotificationsFeedState> emit,
  ) async {
    try {
      await repository.markAsRead(event.notificationId);
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
    } catch (e) {
      emit(NotificationsFeedError(e.toString()));
    }
  }
}
