part of 'orders_bloc.dart';

abstract class OrdersState extends Equatable {
  @override
  List<Object?> get props => [];
}

class OrdersInitial extends OrdersState {}

class OrdersLoading extends OrdersState {}

class OrdersLoaded extends OrdersState {
  final List<Order> orders;
  final PaginationModel<Order>? pagination;

  OrdersLoaded(this.orders, {this.pagination});

  @override
  List<Object?> get props => [orders, pagination];

  bool get hasPagination => pagination != null;
  bool get canLoadMore => pagination?.hasNextPage ?? false;
}

class OrderDetailsLoaded extends OrdersState {
  final Order order;
  OrderDetailsLoaded(this.order);

  @override
  List<Object?> get props => [order];
}

class OrdersError extends OrdersState {
  final String message;
  OrdersError(this.message);

  @override
  List<Object?> get props => [message];
}
