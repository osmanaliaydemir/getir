import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../core/models/pagination_model.dart';
import '../../../domain/entities/notification.dart';
import '../../../domain/services/notifications_feed_service.dart';

part 'notifications_feed_event.dart';
part 'notifications_feed_state.dart';

class NotificationsFeedBloc
    extends Bloc<NotificationsFeedEvent, NotificationsFeedState> {
  final NotificationsFeedService _service;

  NotificationsFeedBloc({required NotificationsFeedService service})
    : _service = service,
      super(NotificationsFeedInitial()) {
    on<LoadNotificationsFeed>(_onLoad);
    on<MarkNotificationRead>(_onMarkRead);
    on<LoadMoreNotifications>(_onLoadMore);
    on<RefreshNotifications>(_onRefresh);
  }

  Future<void> _onLoad(
    LoadNotificationsFeed event,
    Emitter<NotificationsFeedState> emit,
  ) async {
    emit(NotificationsFeedLoading());

    final result = await _service.getNotifications(
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
    final result = await _service.markAsRead(event.notificationId);

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

  // 🔄 ============= PAGINATION HANDLERS =============

  Future<void> _onLoadMore(
    LoadMoreNotifications event,
    Emitter<NotificationsFeedState> emit,
  ) async {
    if (state is! NotificationsFeedLoaded) return;
    final currentState = state as NotificationsFeedLoaded;

    if (currentState.pagination == null ||
        !currentState.pagination!.hasNextPage ||
        currentState.pagination!.isLoading) {
      return;
    }

    final loadingPagination = currentState.pagination!.setLoading(true);
    emit(
      NotificationsFeedLoaded(
        currentState.items,
        pagination: loadingPagination,
      ),
    );

    final nextPage = currentState.pagination!.nextPage;
    final result = await _service.getNotifications(
      page: nextPage,
      pageSize: 20,
    );

    result.when(
      success: (newItems) {
        final updatedItems = [...currentState.items, ...newItems];
        final updatedPagination = currentState.pagination!
            .addItems(newItems)
            .copyWith(
              currentPage: nextPage,
              hasNextPage: newItems.length >= 20,
              isLoading: false,
            );
        emit(
          NotificationsFeedLoaded(updatedItems, pagination: updatedPagination),
        );
      },
      failure: (exception) {
        final errorPagination = currentState.pagination!.setLoading(false);
        emit(
          NotificationsFeedLoaded(
            currentState.items,
            pagination: errorPagination,
          ),
        );
      },
    );
  }

  Future<void> _onRefresh(
    RefreshNotifications event,
    Emitter<NotificationsFeedState> emit,
  ) async {
    final result = await _service.getNotifications(page: 1, pageSize: 20);

    result.when(
      success: (items) {
        final pagination = PaginationModel<AppNotification>(
          items: items,
          currentPage: 1,
          totalPages: 999,
          totalItems: items.length,
          hasNextPage: items.length >= 20,
          hasPreviousPage: false,
          isLoading: false,
          isRefreshing: false,
        );
        emit(NotificationsFeedLoaded(items, pagination: pagination));
      },
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(NotificationsFeedError(message));
      },
    );
  }
}
