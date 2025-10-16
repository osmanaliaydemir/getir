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
    
    // Provide more specific error messages based on exception type
    final errorString = exception.toString().toLowerCase();
    
    if (errorString.contains('network') || errorString.contains('connection')) {
      return 'İnternet bağlantınızı kontrol edin ve tekrar deneyin.';
    } else if (errorString.contains('timeout')) {
      return 'İşlem zaman aşımına uğradı. Lütfen tekrar deneyin.';
    } else if (errorString.contains('unauthorized') || errorString.contains('401')) {
      return 'Bu işlemi gerçekleştirmek için giriş yapmanız gerekiyor.';
    } else if (errorString.contains('forbidden') || errorString.contains('403')) {
      return 'Bu işlem için yetkiniz bulunmuyor.';
    } else if (errorString.contains('not found') || errorString.contains('404')) {
      return 'İstenen içerik bulunamadı.';
    } else if (errorString.contains('server') || errorString.contains('500')) {
      return 'Sunucu ile bağlantı kurulamadı. Lütfen daha sonra tekrar deneyin.';
    }
    
    return 'Siparişleriniz yüklenirken bir hata oluştu. Lütfen tekrar deneyin.';
  }
}
