part of 'orders_bloc.dart';

abstract class OrdersEvent extends Equatable {
  @override
  List<Object?> get props => [];
}

class LoadUserOrders extends OrdersEvent {}

class LoadOrderDetails extends OrdersEvent {
  final String orderId;

  LoadOrderDetails(this.orderId);

  @override
  List<Object?> get props => [orderId];
}
