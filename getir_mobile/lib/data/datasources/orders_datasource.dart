import 'package:dio/dio.dart';
import '../../domain/entities/order.dart';

abstract class OrdersDataSource {
  Future<List<Order>> getUserOrders({int page = 1, int limit = 20});
  Future<Order> getOrderDetails(String orderId);
  Future<Order> createOrder({
    required String merchantId,
    required String deliveryAddressId,
    required List<Map<String, dynamic>> items,
    required String paymentMethod,
    String? couponCode,
    String? notes,
  });
  Future<void> cancelOrder(String orderId);
  Future<Order> reorder(String orderId);
}

class OrdersDataSourceImpl implements OrdersDataSource {
  final Dio _dio;
  OrdersDataSourceImpl(this._dio);

  @override
  Future<List<Order>> getUserOrders({int page = 1, int limit = 20}) async {
    final response = await _dio.get(
      '/api/v1/user/orders',
      queryParameters: {'page': page, 'pageSize': limit},
    );

    // Handle ApiResponse<PagedResult> format
    final responseData = response.data;
    if (responseData is Map<String, dynamic>) {
      if (responseData['success'] == true && responseData['value'] != null) {
        final pagedResult = responseData['value'] as Map<String, dynamic>;
        final ordersList = pagedResult['items'] as List;
        return ordersList
            .map((json) => Order.fromJson(json as Map<String, dynamic>))
            .toList();
      }
    }

    // Fallback for direct data
    if (responseData is List) {
      return responseData
          .map((json) => Order.fromJson(json as Map<String, dynamic>))
          .toList();
    }

    throw Exception('Invalid response format');
  }

  @override
  Future<Order> getOrderDetails(String orderId) async {
    final response = await _dio.get('/api/v1/user/orders/$orderId');

    // Handle ApiResponse format
    final responseData = response.data;
    if (responseData is Map<String, dynamic>) {
      if (responseData['success'] == true && responseData['value'] != null) {
        return Order.fromJson(responseData['value'] as Map<String, dynamic>);
      }
    }

    // Fallback for direct data
    return Order.fromJson(responseData as Map<String, dynamic>);
  }

  @override
  Future<Order> createOrder({
    required String merchantId,
    required String deliveryAddressId,
    required List<Map<String, dynamic>> items,
    required String paymentMethod,
    String? couponCode,
    String? notes,
  }) async {
    final response = await _dio.post(
      '/api/v1/orders',
      data: {
        'merchantId': merchantId,
        'deliveryAddressId': deliveryAddressId,
        'items': items,
        'paymentMethod': paymentMethod,
        'couponCode': couponCode,
        'notes': notes,
      },
    );

    // Handle ApiResponse format
    final responseData = response.data;
    if (responseData is Map<String, dynamic>) {
      if (responseData['success'] == true && responseData['value'] != null) {
        return Order.fromJson(responseData['value'] as Map<String, dynamic>);
      }
    }

    // Fallback for direct data
    return Order.fromJson(responseData as Map<String, dynamic>);
  }

  @override
  Future<void> cancelOrder(String orderId) async {
    await _dio.post('/api/v1/user/orders/$orderId/cancel');
  }

  @override
  Future<Order> reorder(String orderId) async {
    final response = await _dio.post('/api/v1/user/orders/$orderId/reorder');

    // Handle ApiResponse format
    final responseData = response.data;
    if (responseData is Map<String, dynamic>) {
      if (responseData['success'] == true && responseData['value'] != null) {
        return Order.fromJson(responseData['value'] as Map<String, dynamic>);
      }
    }

    // Fallback for direct data
    return Order.fromJson(responseData as Map<String, dynamic>);
  }
}
