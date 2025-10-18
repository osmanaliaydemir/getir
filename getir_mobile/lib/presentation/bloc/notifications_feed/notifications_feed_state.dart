part of 'notifications_feed_bloc.dart';

abstract class NotificationsFeedState extends Equatable {
  const NotificationsFeedState();
  @override
  List<Object?> get props => [];
}

class NotificationsFeedInitial extends NotificationsFeedState {}

class NotificationsFeedLoading extends NotificationsFeedState {}

class NotificationsFeedLoaded extends NotificationsFeedState {
  final List<AppNotification> items;
  final PaginationModel<AppNotification>? pagination;

  const NotificationsFeedLoaded(this.items, {this.pagination});

  @override
  List<Object?> get props => [items, pagination];

  bool get hasPagination => pagination != null;
  bool get canLoadMore => pagination?.hasNextPage ?? false;
}

class NotificationsFeedError extends NotificationsFeedState {
  final String message;
  const NotificationsFeedError(this.message);
  @override
  List<Object?> get props => [message];
}
