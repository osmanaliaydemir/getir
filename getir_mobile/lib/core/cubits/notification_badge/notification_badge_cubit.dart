import 'package:flutter_bloc/flutter_bloc.dart';
import 'notification_badge_state.dart';

/// Cubit to manage notification badge count
class NotificationBadgeCubit extends Cubit<NotificationBadgeState> {
  NotificationBadgeCubit() : super(const NotificationBadgeState());

  /// Updates the unread count to a specific value
  void updateUnreadCount(int count) {
    if (state.unreadCount != count) {
      emit(NotificationBadgeState(unreadCount: count));
    }
  }

  /// Increments the unread count by 1
  void increment() {
    emit(NotificationBadgeState(unreadCount: state.unreadCount + 1));
  }

  /// Decrements the unread count by 1 (minimum 0)
  void decrement() {
    if (state.unreadCount > 0) {
      emit(NotificationBadgeState(unreadCount: state.unreadCount - 1));
    }
  }

  /// Resets the unread count to 0
  void reset() {
    if (state.unreadCount > 0) {
      emit(const NotificationBadgeState(unreadCount: 0));
    }
  }
}
