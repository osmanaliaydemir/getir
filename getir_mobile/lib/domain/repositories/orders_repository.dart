import '../../core/errors/result.dart';
import '../entities/order.dart';

abstract class IOrdersRepository {
  Future<Result<List<Order>>> getUserOrders({
    int page = 1,
    int limit = 20,
  });
  Future<Result<Order>> getOrderDetails(String orderId);
  Future<Result<Order>> createOrder({
    required String merchantId,
    required String deliveryAddressId,
    required List<Map<String, dynamic>> items,
    required String paymentMethod,
    String? couponCode,
    String? notes,
  });
  Future<Result<void>> cancelOrder(String orderId);
  Future<Result<Order>> reorder(String orderId);
}
