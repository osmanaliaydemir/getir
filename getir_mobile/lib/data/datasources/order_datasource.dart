import 'package:dio/dio.dart';
import '../../domain/entities/order.dart';

abstract class IOrderDataSource {
  Future<Order> createOrder(CreateOrderRequest request);
  Future<List<Order>> getUserOrders();
  Future<Order> getOrderById(String orderId);
  Future<Order> cancelOrder(String orderId);
  Future<PaymentResult> processPayment(CreatePaymentRequest request);
  Future<PaymentResult> getPaymentStatus(String paymentId);
}

class OrderDataSourceImpl implements IOrderDataSource {
  final Dio _dio;

  OrderDataSourceImpl(this._dio);

  @override
  Future<Order> createOrder(CreateOrderRequest request) async {
    try {
      final response = await _dio.post('/api/v1/order', data: request.toJson());

      if (response.statusCode == 201 || response.statusCode == 200) {
        final data = response.data['data'] ?? response.data;
        return Order.fromJson(data);
      } else {
        throw Exception('Failed to create order: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error: $e');
    }
  }

  @override
  Future<List<Order>> getUserOrders() async {
    try {
      final response = await _dio.get('/api/v1/order');

      if (response.statusCode == 200) {
        final List<dynamic> data = response.data['data'] ?? response.data;
        return data.map((json) => Order.fromJson(json)).toList();
      } else {
        throw Exception('Failed to load orders: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error: $e');
    }
  }

  @override
  Future<Order> getOrderById(String orderId) async {
    try {
      final response = await _dio.get('/api/v1/order/$orderId');

      if (response.statusCode == 200) {
        final data = response.data['data'] ?? response.data;
        return Order.fromJson(data);
      } else {
        throw Exception('Failed to load order: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error: $e');
    }
  }

  @override
  Future<Order> cancelOrder(String orderId) async {
    try {
      final response = await _dio.put('/api/v1/order/$orderId/cancel');

      if (response.statusCode == 200) {
        final data = response.data['data'] ?? response.data;
        return Order.fromJson(data);
      } else {
        throw Exception('Failed to cancel order: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error: $e');
    }
  }

  @override
  Future<PaymentResult> processPayment(CreatePaymentRequest request) async {
    try {
      final response = await _dio.post(
        '/api/v1/payment',
        data: request.toJson(),
      );

      if (response.statusCode == 201 || response.statusCode == 200) {
        final data = response.data['data'] ?? response.data;
        return PaymentResult.fromJson(data);
      } else {
        throw Exception('Failed to process payment: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error: $e');
    }
  }

  @override
  Future<PaymentResult> getPaymentStatus(String paymentId) async {
    try {
      final response = await _dio.get('/api/v1/payment/$paymentId');

      if (response.statusCode == 200) {
        final data = response.data['data'] ?? response.data;
        return PaymentResult.fromJson(data);
      } else {
        throw Exception('Failed to get payment status: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error: $e');
    }
  }

  Exception _handleDioError(DioException e) {
    switch (e.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
        return Exception(
          'Bağlantı zaman aşımı. Lütfen internet bağlantınızı kontrol edin.',
        );
      case DioExceptionType.badResponse:
        final statusCode = e.response?.statusCode;
        final message = e.response?.data?['message'] ?? 'Sunucu hatası';
        return Exception('Hata $statusCode: $message');
      case DioExceptionType.cancel:
        return Exception('İstek iptal edildi');
      case DioExceptionType.connectionError:
        return Exception(
          'Bağlantı hatası. Lütfen internet bağlantınızı kontrol edin.',
        );
      default:
        return Exception('Beklenmeyen hata: ${e.message}');
    }
  }
}

class CreateOrderRequest {
  final String merchantId;
  final String deliveryAddressId;
  final PaymentMethod paymentMethod;
  final String? couponCode;
  final String? notes;
  final List<CreateOrderItemRequest> items;

  const CreateOrderRequest({
    required this.merchantId,
    required this.deliveryAddressId,
    required this.paymentMethod,
    this.couponCode,
    this.notes,
    required this.items,
  });

  Map<String, dynamic> toJson() {
    return {
      'merchantId': merchantId,
      'deliveryAddressId': deliveryAddressId,
      'paymentMethod': paymentMethod.value,
      'couponCode': couponCode,
      'notes': notes,
      'items': items.map((item) => item.toJson()).toList(),
    };
  }
}

class CreateOrderItemRequest {
  final String productId;
  final int quantity;
  final String? selectedVariantId;
  final List<String> selectedOptionIds;

  const CreateOrderItemRequest({
    required this.productId,
    required this.quantity,
    this.selectedVariantId,
    required this.selectedOptionIds,
  });

  Map<String, dynamic> toJson() {
    return {
      'productId': productId,
      'quantity': quantity,
      'selectedVariantId': selectedVariantId,
      'selectedOptionIds': selectedOptionIds,
    };
  }
}

class CreatePaymentRequest {
  final String orderId;
  final PaymentMethod paymentMethod;
  final double amount;
  final Map<String, dynamic>? paymentDetails;

  const CreatePaymentRequest({
    required this.orderId,
    required this.paymentMethod,
    required this.amount,
    this.paymentDetails,
  });

  Map<String, dynamic> toJson() {
    return {
      'orderId': orderId,
      'paymentMethod': paymentMethod.value,
      'amount': amount,
      'paymentDetails': paymentDetails,
    };
  }
}

class PaymentResult {
  final String id;
  final String orderId;
  final PaymentStatus status;
  final String? transactionId;
  final String? message;
  final DateTime createdAt;

  const PaymentResult({
    required this.id,
    required this.orderId,
    required this.status,
    this.transactionId,
    this.message,
    required this.createdAt,
  });

  factory PaymentResult.fromJson(Map<String, dynamic> json) {
    return PaymentResult(
      id: json['id'] as String,
      orderId: json['orderId'] as String,
      status: PaymentStatus.fromString(json['status'] as String),
      transactionId: json['transactionId'] as String?,
      message: json['message'] as String?,
      createdAt: DateTime.parse(json['createdAt'] as String),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'orderId': orderId,
      'status': status.value,
      'transactionId': transactionId,
      'message': message,
      'createdAt': createdAt.toIso8601String(),
    };
  }
}
