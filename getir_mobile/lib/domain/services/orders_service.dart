import '../../core/errors/result.dart';
import '../entities/order.dart';
import '../repositories/orders_repository.dart';

/// Orders Service
///
/// Centralized service for order operations.
class OrdersService {
  final IOrdersRepository _repository;

  const OrdersService(this._repository);

  Future<Result<List<Order>>> getUserOrders({
    int page = 1,
    int limit = 20,
  }) async {
    return await _repository.getUserOrders(page: page, limit: limit);
  }

  Future<Result<Order>> getOrderDetails(String orderId) async {
    return await _repository.getOrderDetails(orderId);
  }

  Future<Result<Order>> createOrder({
    required String merchantId,
    required String deliveryAddressId,
    required List<Map<String, dynamic>> items,
    required String paymentMethod,
    String? couponCode,
    String? notes,
  }) async {
    return await _repository.createOrder(
      merchantId: merchantId,
      deliveryAddressId: deliveryAddressId,
      items: items,
      paymentMethod: paymentMethod,
      couponCode: couponCode,
      notes: notes,
    );
  }

  Future<Result<void>> cancelOrder(String orderId) async {
    return await _repository.cancelOrder(orderId);
  }

  Future<Result<Order>> reorder(String orderId) async {
    return await _repository.reorder(orderId);
  }
}
