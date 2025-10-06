import '../entities/order.dart';
import '../../data/datasources/order_datasource.dart';

abstract class IOrderRepository {
  Future<Order> createOrder(CreateOrderRequest request);
  Future<List<Order>> getUserOrders();
  Future<Order> getOrderById(String orderId);
  Future<Order> cancelOrder(String orderId);
  Future<PaymentResult> processPayment(CreatePaymentRequest request);
  Future<PaymentResult> getPaymentStatus(String paymentId);
}
