import 'package:equatable/equatable.dart';

/// Represents the state of notification badge
class NotificationBadgeState extends Equatable {
  final int unreadCount;

  const NotificationBadgeState({
    this.unreadCount = 0,
  });

  /// Returns true if there are unread notifications
  bool get hasUnread => unreadCount > 0;

  /// Returns badge text for display
  /// - Returns '99+' if count > 99
  /// - Returns count as string if count > 0
  /// - Returns empty string if count = 0
  String get badgeText {
    if (unreadCount > 99) return '99+';
    if (unreadCount > 0) return unreadCount.toString();
    return '';
  }

  @override
  List<Object?> get props => [unreadCount];

  @override
  String toString() => 'NotificationBadgeState(unreadCount: $unreadCount)';
}

