import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../core/models/pagination_model.dart';
import '../../../domain/entities/order.dart';
import '../../../domain/services/orders_service.dart';

part 'orders_event.dart';
part 'orders_state.dart';

class OrdersBloc extends Bloc<OrdersEvent, OrdersState> {
  final OrdersService _ordersService;

  OrdersBloc(this._ordersService) : super(OrdersInitial()) {
    on<LoadUserOrders>(_onLoadUserOrders);
    on<LoadOrderDetails>(_onLoadOrderDetails);
    on<LoadMoreOrders>(_onLoadMoreOrders);
    on<RefreshOrders>(_onRefreshOrders);
  }

  Future<void> _onLoadUserOrders(
    LoadUserOrders event,
    Emitter<OrdersState> emit,
  ) async {
    emit(OrdersLoading());

    final result = await _ordersService.getUserOrders();

    result.when(
      success: (orders) => emit(OrdersLoaded(orders)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(OrdersError(message));
      },
    );
  }

  Future<void> _onLoadOrderDetails(
    LoadOrderDetails event,
    Emitter<OrdersState> emit,
  ) async {
    emit(OrdersLoading());

    final result = await _ordersService.getOrderDetails(event.orderId);

    result.when(
      success: (order) => emit(OrderDetailsLoaded(order)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(OrdersError(message));
      },
    );
  }

  Future<void> _onLoadMoreOrders(
    LoadMoreOrders event,
    Emitter<OrdersState> emit,
  ) async {
    if (state is! OrdersLoaded) return;
    final currentState = state as OrdersLoaded;

    if (currentState.pagination == null ||
        !currentState.pagination!.hasNextPage ||
        currentState.pagination!.isLoading) {
      return;
    }

    final loadingPagination = currentState.pagination!.setLoading(true);
    emit(OrdersLoaded(currentState.orders, pagination: loadingPagination));

    final nextPage = currentState.pagination!.nextPage;
    final result = await _ordersService.getUserOrders(page: nextPage);

    result.when(
      success: (newOrders) {
        final updatedOrders = [...currentState.orders, ...newOrders];
        final updatedPagination = currentState.pagination!
            .addItems(newOrders)
            .copyWith(
              currentPage: nextPage,
              hasNextPage: newOrders.length >= 20,
              isLoading: false,
            );
        emit(OrdersLoaded(updatedOrders, pagination: updatedPagination));
      },
      failure: (exception) {
        final errorPagination = currentState.pagination!.setLoading(false);
        emit(OrdersLoaded(currentState.orders, pagination: errorPagination));
      },
    );
  }

  Future<void> _onRefreshOrders(
    RefreshOrders event,
    Emitter<OrdersState> emit,
  ) async {
    if (state is OrdersLoaded) {
      final currentState = state as OrdersLoaded;
      if (currentState.pagination != null) {
        final refreshingPagination = currentState.pagination!.setRefreshing(
          true,
        );
        emit(
          OrdersLoaded(currentState.orders, pagination: refreshingPagination),
        );
      }
    }

    final result = await _ordersService.getUserOrders();

    result.when(
      success: (orders) {
        final pagination = PaginationModel<Order>(
          items: orders,
          currentPage: 1,
          totalPages: 999,
          totalItems: orders.length,
          hasNextPage: orders.length >= 20,
          hasPreviousPage: false,
          isLoading: false,
          isRefreshing: false,
        );
        emit(OrdersLoaded(orders, pagination: pagination));
      },
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(OrdersError(message));
      },
    );
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
    } else if (errorString.contains('unauthorized') ||
        errorString.contains('401')) {
      return 'Bu işlemi gerçekleştirmek için giriş yapmanız gerekiyor.';
    } else if (errorString.contains('forbidden') ||
        errorString.contains('403')) {
      return 'Bu işlem için yetkiniz bulunmuyor.';
    } else if (errorString.contains('not found') ||
        errorString.contains('404')) {
      return 'İstenen içerik bulunamadı.';
    } else if (errorString.contains('server') || errorString.contains('500')) {
      return 'Sunucu ile bağlantı kurulamadı. Lütfen daha sonra tekrar deneyin.';
    }

    return 'Siparişleriniz yüklenirken bir hata oluştu. Lütfen tekrar deneyin.';
  }
}
