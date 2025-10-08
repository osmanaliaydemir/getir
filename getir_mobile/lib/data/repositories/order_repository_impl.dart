import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../domain/entities/order.dart';
import '../../domain/repositories/order_repository.dart';
import '../datasources/order_datasource.dart';

class OrderRepositoryImpl implements IOrderRepository {
  final IOrderDataSource _dataSource;

  OrderRepositoryImpl(this._dataSource);

  @override
  Future<Result<Order>> createOrder(CreateOrderRequest request) async {
    try {
      final order = await _dataSource.createOrder(request);
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
  Future<Result<Order>> getOrderById(String orderId) async {
    try {
      final order = await _dataSource.getOrderById(orderId);
      return Result.success(order);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get order: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<Order>> cancelOrder(String orderId) async {
    try {
      final order = await _dataSource.cancelOrder(orderId);
      return Result.success(order);
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
  Future<Result<PaymentResult>> processPayment(
    CreatePaymentRequest request,
  ) async {
    try {
      final paymentResult = await _dataSource.processPayment(request);
      return Result.success(paymentResult);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to process payment: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<PaymentResult>> getPaymentStatus(String paymentId) async {
    try {
      final paymentResult = await _dataSource.getPaymentStatus(paymentId);
      return Result.success(paymentResult);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get payment status: ${e.toString()}'),
      );
    }
  }
}
