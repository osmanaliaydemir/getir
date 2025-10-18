import '../../core/errors/result.dart';
import '../entities/order.dart';
import '../repositories/order_repository.dart';
import '../../data/datasources/order_datasource.dart';

/// Order Service
///
/// Centralized service for all order-related operations.
/// Replaces 6 separate UseCase classes.
class OrderService {
  final IOrderRepository _repository;

  const OrderService(this._repository);

  Future<Result<Order>> createOrder(CreateOrderRequest request) async {
    return await _repository.createOrder(request);
  }

  Future<Result<List<Order>>> getUserOrders() async {
    return await _repository.getUserOrders();
  }

  Future<Result<Order>> getOrderById(String orderId) async {
    return await _repository.getOrderById(orderId);
  }

  Future<Result<Order>> cancelOrder(String orderId) async {
    return await _repository.cancelOrder(orderId);
  }

  Future<Result<PaymentResult>> processPayment(
    CreatePaymentRequest request,
  ) async {
    return await _repository.processPayment(request);
  }

  Future<Result<PaymentResult>> getPaymentStatus(String paymentId) async {
    return await _repository.getPaymentStatus(paymentId);
  }
}
