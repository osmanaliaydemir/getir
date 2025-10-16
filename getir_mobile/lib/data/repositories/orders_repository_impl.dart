import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../domain/entities/order.dart';
import '../../domain/repositories/orders_repository.dart';
import '../datasources/orders_datasource.dart';

class OrdersRepositoryImpl implements IOrdersRepository {
  final OrdersDataSource _dataSource;

  OrdersRepositoryImpl(this._dataSource);

  @override
  Future<Result<List<Order>>> getUserOrders() async {
    try {
      final orders = await _dataSource.getUserOrders();
      return Result.success(orders);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get user orders: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<Order>> getOrderDetails(String orderId) async {
    try {
      final order = await _dataSource.getOrderDetails(orderId);
      return Result.success(order);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get order details: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<Order>> createOrder({
    required String merchantId,
    required String deliveryAddressId,
    required List<Map<String, dynamic>> items,
    required String paymentMethod,
    String? couponCode,
    String? notes,
  }) async {
    try {
      final order = await _dataSource.createOrder(
        merchantId: merchantId,
        deliveryAddressId: deliveryAddressId,
        items: items,
        paymentMethod: paymentMethod,
        couponCode: couponCode,
        notes: notes,
      );
      return Result.success(order);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to create order: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<void>> cancelOrder(String orderId) async {
    try {
      await _dataSource.cancelOrder(orderId);
      return Result.success(null);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to cancel order: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<Order>> reorder(String orderId) async {
    try {
      final order = await _dataSource.reorder(orderId);
      return Result.success(order);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to reorder: ${e.toString()}'),
      );
    }
  }
}
