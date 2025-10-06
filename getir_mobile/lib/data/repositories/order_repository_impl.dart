import '../../domain/entities/order.dart';
import '../../domain/repositories/order_repository.dart';
import '../datasources/order_datasource.dart';

class OrderRepositoryImpl implements IOrderRepository {
  final IOrderDataSource _dataSource;

  OrderRepositoryImpl(this._dataSource);

  @override
  Future<Order> createOrder(CreateOrderRequest request) async {
    return await _dataSource.createOrder(request);
  }

  @override
  Future<List<Order>> getUserOrders() async {
    return await _dataSource.getUserOrders();
  }

  @override
  Future<Order> getOrderById(String orderId) async {
    return await _dataSource.getOrderById(orderId);
  }

  @override
  Future<Order> cancelOrder(String orderId) async {
    return await _dataSource.cancelOrder(orderId);
  }

  @override
  Future<PaymentResult> processPayment(CreatePaymentRequest request) async {
    return await _dataSource.processPayment(request);
  }

  @override
  Future<PaymentResult> getPaymentStatus(String paymentId) async {
    return await _dataSource.getPaymentStatus(paymentId);
  }
}
