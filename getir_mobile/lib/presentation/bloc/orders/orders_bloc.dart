import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/entities/order.dart';
import '../../../domain/services/orders_service.dart';

part 'orders_event.dart';
part 'orders_state.dart';

class OrdersBloc extends Bloc<OrdersEvent, OrdersState> {
  final OrdersService _ordersService;

  OrdersBloc(this._ordersService) : super(OrdersInitial()) {
    on<LoadUserOrders>((event, emit) async {
      emit(OrdersLoading());

      final result = await _ordersService.getUserOrders();

      result.when(
        success: (orders) => emit(OrdersLoaded(orders)),
        failure: (exception) {
          final message = _getErrorMessage(exception);
          emit(OrdersError(message));
        },
      );
    });

    on<LoadOrderDetails>((event, emit) async {
      emit(OrdersLoading());

      final result = await _ordersService.getOrderDetails(event.orderId);

      result.when(
        success: (order) => emit(OrderDetailsLoaded(order)),
        failure: (exception) {
          final message = _getErrorMessage(exception);
          emit(OrdersError(message));
        },
      );
    });
  }

  /// Extract user-friendly error message from exception
  String _getErrorMessage(Exception exception) {
    if (exception is AppException) {
      return exception.message;
    }
    return 'An unexpected error occurred';
  }
}
