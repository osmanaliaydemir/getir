import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../entities/order.dart';
import '../repositories/order_repository.dart';
import '../../data/datasources/order_datasource.dart';

/// Create Order Use Case
class CreateOrderUseCase {
  final IOrderRepository _repository;

  CreateOrderUseCase(this._repository);

  Future<Result<Order>> call(CreateOrderRequest request) async {
    return await _repository.createOrder(request);
  }
}

/// Get User Orders Use Case
class GetUserOrdersUseCase {
  final IOrderRepository _repository;

  GetUserOrdersUseCase(this._repository);

  Future<Result<List<Order>>> call() async {
    return await _repository.getUserOrders();
  }
}

/// Get Order By ID Use Case
class GetOrderByIdUseCase {
  final IOrderRepository _repository;

  GetOrderByIdUseCase(this._repository);

  Future<Result<Order>> call(String orderId) async {
    if (orderId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Order ID cannot be empty',
          code: 'EMPTY_ORDER_ID',
        ),
      );
    }

    return await _repository.getOrderById(orderId);
  }
}

/// Cancel Order Use Case
class CancelOrderUseCase {
  final IOrderRepository _repository;

  CancelOrderUseCase(this._repository);

  Future<Result<Order>> call(String orderId) async {
    if (orderId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Order ID cannot be empty',
          code: 'EMPTY_ORDER_ID',
        ),
      );
    }

    return await _repository.cancelOrder(orderId);
  }
}

/// Process Payment Use Case
class ProcessPaymentUseCase {
  final IOrderRepository _repository;

  ProcessPaymentUseCase(this._repository);

  Future<Result<PaymentResult>> call(CreatePaymentRequest request) async {
    return await _repository.processPayment(request);
  }
}

/// Get Payment Status Use Case
class GetPaymentStatusUseCase {
  final IOrderRepository _repository;

  GetPaymentStatusUseCase(this._repository);

  Future<Result<PaymentResult>> call(String paymentId) async {
    if (paymentId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Payment ID cannot be empty',
          code: 'EMPTY_PAYMENT_ID',
        ),
      );
    }

    return await _repository.getPaymentStatus(paymentId);
  }
}
