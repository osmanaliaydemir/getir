import '../entities/order.dart';
import '../repositories/order_repository.dart';
import '../../data/datasources/order_datasource.dart';

class CreateOrderUseCase {
  final IOrderRepository _repository;

  CreateOrderUseCase(this._repository);

  Future<Order> call(CreateOrderRequest request) async {
    return await _repository.createOrder(request);
  }
}

class GetUserOrdersUseCase {
  final IOrderRepository _repository;

  GetUserOrdersUseCase(this._repository);

  Future<List<Order>> call() async {
    return await _repository.getUserOrders();
  }
}

class GetOrderByIdUseCase {
  final IOrderRepository _repository;

  GetOrderByIdUseCase(this._repository);

  Future<Order> call(String orderId) async {
    return await _repository.getOrderById(orderId);
  }
}

class CancelOrderUseCase {
  final IOrderRepository _repository;

  CancelOrderUseCase(this._repository);

  Future<Order> call(String orderId) async {
    return await _repository.cancelOrder(orderId);
  }
}

class ProcessPaymentUseCase {
  final IOrderRepository _repository;

  ProcessPaymentUseCase(this._repository);

  Future<PaymentResult> call(CreatePaymentRequest request) async {
    return await _repository.processPayment(request);
  }
}

class GetPaymentStatusUseCase {
  final IOrderRepository _repository;

  GetPaymentStatusUseCase(this._repository);

  Future<PaymentResult> call(String paymentId) async {
    return await _repository.getPaymentStatus(paymentId);
  }
}
