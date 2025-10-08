import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../domain/entities/order.dart';
import '../../../domain/usecases/order_usecases.dart';
import '../../../data/datasources/order_datasource.dart';
import '../../../core/services/analytics_service.dart';
import 'package:firebase_analytics/firebase_analytics.dart';

// Events
abstract class OrderEvent extends Equatable {
  const OrderEvent();

  @override
  List<Object?> get props => [];
}

class LoadUserOrders extends OrderEvent {}

class LoadOrderById extends OrderEvent {
  final String orderId;

  const LoadOrderById(this.orderId);

  @override
  List<Object> get props => [orderId];
}

class CreateOrder extends OrderEvent {
  final CreateOrderRequest request;

  const CreateOrder(this.request);

  @override
  List<Object> get props => [request];
}

class CancelOrder extends OrderEvent {
  final String orderId;

  const CancelOrder(this.orderId);

  @override
  List<Object> get props => [orderId];
}

class ProcessPayment extends OrderEvent {
  final CreatePaymentRequest request;

  const ProcessPayment(this.request);

  @override
  List<Object> get props => [request];
}

class GetPaymentStatus extends OrderEvent {
  final String paymentId;

  const GetPaymentStatus(this.paymentId);

  @override
  List<Object> get props => [paymentId];
}

// States
abstract class OrderState extends Equatable {
  const OrderState();

  @override
  List<Object?> get props => [];
}

class OrderInitial extends OrderState {}

class OrderLoading extends OrderState {}

class OrdersLoaded extends OrderState {
  final List<Order> orders;

  const OrdersLoaded(this.orders);

  @override
  List<Object> get props => [orders];
}

class OrderLoaded extends OrderState {
  final Order order;

  const OrderLoaded(this.order);

  @override
  List<Object> get props => [order];
}

class OrderCreated extends OrderState {
  final Order order;

  const OrderCreated(this.order);

  @override
  List<Object> get props => [order];
}

class OrderCancelled extends OrderState {
  final Order order;

  const OrderCancelled(this.order);

  @override
  List<Object> get props => [order];
}

class PaymentProcessed extends OrderState {
  final PaymentResult paymentResult;

  const PaymentProcessed(this.paymentResult);

  @override
  List<Object> get props => [paymentResult];
}

class PaymentStatusLoaded extends OrderState {
  final PaymentResult paymentResult;

  const PaymentStatusLoaded(this.paymentResult);

  @override
  List<Object> get props => [paymentResult];
}

class OrderError extends OrderState {
  final String message;

  const OrderError(this.message);

  @override
  List<Object> get props => [message];
}

// BLoC
class OrderBloc extends Bloc<OrderEvent, OrderState> {
  final GetUserOrdersUseCase _getUserOrdersUseCase;
  final GetOrderByIdUseCase _getOrderByIdUseCase;
  final CreateOrderUseCase _createOrderUseCase;
  final CancelOrderUseCase _cancelOrderUseCase;
  final ProcessPaymentUseCase _processPaymentUseCase;
  final GetPaymentStatusUseCase _getPaymentStatusUseCase;
  final AnalyticsService _analytics;

  OrderBloc({
    required GetUserOrdersUseCase getUserOrdersUseCase,
    required GetOrderByIdUseCase getOrderByIdUseCase,
    required CreateOrderUseCase createOrderUseCase,
    required CancelOrderUseCase cancelOrderUseCase,
    required ProcessPaymentUseCase processPaymentUseCase,
    required GetPaymentStatusUseCase getPaymentStatusUseCase,
    required AnalyticsService analytics,
  }) : _getUserOrdersUseCase = getUserOrdersUseCase,
       _getOrderByIdUseCase = getOrderByIdUseCase,
       _createOrderUseCase = createOrderUseCase,
       _cancelOrderUseCase = cancelOrderUseCase,
       _processPaymentUseCase = processPaymentUseCase,
       _getPaymentStatusUseCase = getPaymentStatusUseCase,
       _analytics = analytics,
       super(OrderInitial()) {
    on<LoadUserOrders>(_onLoadUserOrders);
    on<LoadOrderById>(_onLoadOrderById);
    on<CreateOrder>(_onCreateOrder);
    on<CancelOrder>(_onCancelOrder);
    on<ProcessPayment>(_onProcessPayment);
    on<GetPaymentStatus>(_onGetPaymentStatus);
  }

  Future<void> _onLoadUserOrders(
    LoadUserOrders event,
    Emitter<OrderState> emit,
  ) async {
    emit(OrderLoading());
    try {
      final orders = await _getUserOrdersUseCase();
      emit(OrdersLoaded(orders));
    } catch (e) {
      emit(OrderError(e.toString()));
    }
  }

  Future<void> _onLoadOrderById(
    LoadOrderById event,
    Emitter<OrderState> emit,
  ) async {
    emit(OrderLoading());
    try {
      final order = await _getOrderByIdUseCase(event.orderId);
      emit(OrderLoaded(order));
    } catch (e) {
      emit(OrderError(e.toString()));
    }
  }

  Future<void> _onCreateOrder(
    CreateOrder event,
    Emitter<OrderState> emit,
  ) async {
    emit(OrderLoading());
    try {
      final order = await _createOrderUseCase(event.request);

      // 📊 Analytics: Track order creation (purchase)
      await _analytics.logPurchase(
        orderId: order.id,
        total: order.totalPrice,
        currency: 'TRY',
        items: order.items
            .map(
              (item) => AnalyticsEventItem(
                itemId: item.productId,
                itemName: item.productName,
                price: item.price,
                quantity: item.quantity,
              ),
            )
            .toList(),
        shipping: order.deliveryFee,
      );

      emit(OrderCreated(order));
    } catch (e) {
      emit(OrderError(e.toString()));
      await _analytics.logError(error: e, reason: 'Order creation failed');
    }
  }

  Future<void> _onCancelOrder(
    CancelOrder event,
    Emitter<OrderState> emit,
  ) async {
    emit(OrderLoading());
    try {
      final order = await _cancelOrderUseCase(event.orderId);

      // 📊 Analytics: Track order cancellation
      await _analytics.logOrderCancelled(
        orderId: order.id,
        value: order.totalPrice,
        reason: 'user_cancelled',
      );

      emit(OrderCancelled(order));
    } catch (e) {
      emit(OrderError(e.toString()));
      await _analytics.logError(error: e, reason: 'Order cancellation failed');
    }
  }

  Future<void> _onProcessPayment(
    ProcessPayment event,
    Emitter<OrderState> emit,
  ) async {
    emit(OrderLoading());
    try {
      final paymentResult = await _processPaymentUseCase(event.request);

      // 📊 Analytics: Track payment info added
      await _analytics.logAddPaymentInfo(
        paymentType: event.request.paymentMethod,
        value: event.request.amount,
        currency: 'TRY',
      );

      emit(PaymentProcessed(paymentResult));
    } catch (e) {
      emit(OrderError(e.toString()));
      await _analytics.logError(error: e, reason: 'Payment processing failed');
    }
  }

  Future<void> _onGetPaymentStatus(
    GetPaymentStatus event,
    Emitter<OrderState> emit,
  ) async {
    emit(OrderLoading());
    try {
      final paymentResult = await _getPaymentStatusUseCase(event.paymentId);
      emit(PaymentStatusLoaded(paymentResult));
    } catch (e) {
      emit(OrderError(e.toString()));
    }
  }
}
