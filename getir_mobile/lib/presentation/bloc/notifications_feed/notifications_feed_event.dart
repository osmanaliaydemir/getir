part of 'notifications_feed_bloc.dart';

abstract class NotificationsFeedEvent extends Equatable {
  const NotificationsFeedEvent();
  @override
  List<Object?> get props => [];
}

class LoadNotificationsFeed extends NotificationsFeedEvent {
  final int page;
  final int pageSize;
  const LoadNotificationsFeed({this.page = 1, this.pageSize = 20});
}

class MarkNotificationRead extends NotificationsFeedEvent {
  final String notificationId;
  const MarkNotificationRead(this.notificationId);
  @override
  List<Object?> get props => [notificationId];
}
