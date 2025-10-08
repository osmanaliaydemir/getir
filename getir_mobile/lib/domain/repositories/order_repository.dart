import '../../core/errors/result.dart';
import '../entities/order.dart';
import '../../data/datasources/order_datasource.dart';

abstract class IOrderRepository {
  Future<Result<Order>> createOrder(CreateOrderRequest request);
  Future<Result<List<Order>>> getUserOrders();
  Future<Result<Order>> getOrderById(String orderId);
  Future<Result<Order>> cancelOrder(String orderId);
  Future<Result<PaymentResult>> processPayment(CreatePaymentRequest request);
  Future<Result<PaymentResult>> getPaymentStatus(String paymentId);
}
