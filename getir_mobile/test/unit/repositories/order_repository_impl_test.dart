import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:dio/dio.dart';
import 'package:getir_mobile/data/repositories/order_repository_impl.dart';
import 'package:getir_mobile/data/datasources/order_datasource.dart';
import 'package:getir_mobile/domain/entities/order.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([IOrderDataSource])
import 'order_repository_impl_test.mocks.dart';

void main() {
  late OrderRepositoryImpl repository;
  late MockIOrderDataSource mockDataSource;

  setUp(() {
    mockDataSource = MockIOrderDataSource();
    repository = OrderRepositoryImpl(mockDataSource);
  });

  group('OrderRepositoryImpl -', () {
    group('createOrder', () {
      test('returns success with order when created', () async {
        // Arrange
        final request = CreateOrderRequest(
          merchantId: 'merchant-123',
          deliveryAddressId: 'address-123',
          paymentMethod: PaymentMethod.cash,
          items: [
            CreateOrderItemRequest(
              productId: 'product-123',
              quantity: 2,
              selectedOptionIds: [],
            ),
          ],
        );
        when(
          mockDataSource.createOrder(any),
        ).thenAnswer((_) async => MockData.testOrder);

        // Act
        final result = await repository.createOrder(request);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testOrder);
      });

      test('handles validation error', () async {
        // Arrange
        final request = CreateOrderRequest(
          merchantId: 'merchant-123',
          deliveryAddressId: 'address-123',
          paymentMethod: PaymentMethod.cash,
          items: [],
        );
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/order'),
          type: DioExceptionType.badResponse,
          response: Response(
            requestOptions: RequestOptions(path: '/order'),
            statusCode: 400,
            data: {'message': 'Invalid order data'},
          ),
        );
        when(mockDataSource.createOrder(any)).thenThrow(dioException);

        // Act
        final result = await repository.createOrder(request);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<ValidationException>());
      });
    });

    group('getUserOrders', () {
      test('returns success with orders list', () async {
        // Arrange
        final orders = [MockData.testOrder];
        when(mockDataSource.getUserOrders()).thenAnswer((_) async => orders);

        // Act
        final result = await repository.getUserOrders();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, orders);
      });

      test('handles network error', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/orders'),
          type: DioExceptionType.connectionError,
        );
        when(mockDataSource.getUserOrders()).thenThrow(dioException);

        // Act
        final result = await repository.getUserOrders();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NoInternetException>());
      });
    });

    group('getOrderById', () {
      test('returns success with order when found', () async {
        // Arrange
        when(
          mockDataSource.getOrderById(any),
        ).thenAnswer((_) async => MockData.testOrder);

        // Act
        final result = await repository.getOrderById('order-123');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testOrder);
      });

      test('handles 404 error', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/orders/123'),
          type: DioExceptionType.badResponse,
          response: Response(
            requestOptions: RequestOptions(path: '/orders/123'),
            statusCode: 404,
            data: {'message': 'Order not found'},
          ),
        );
        when(mockDataSource.getOrderById(any)).thenThrow(dioException);

        // Act
        final result = await repository.getOrderById('invalid-id');

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });
    });

    group('cancelOrder', () {
      test('returns success with cancelled order', () async {
        // Arrange
        when(
          mockDataSource.cancelOrder(any),
        ).thenAnswer((_) async => MockData.testOrder);

        // Act
        final result = await repository.cancelOrder('order-123');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testOrder);
      });

      test('handles cancellation error', () async {
        // Arrange
        const appException = BusinessException(
          message: 'Order cannot be cancelled',
        );
        when(mockDataSource.cancelOrder(any)).thenThrow(appException);

        // Act
        final result = await repository.cancelOrder('order-123');

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, appException);
      });
    });

    group('processPayment', () {
      test('returns success with payment result', () async {
        // Arrange
        final request = CreatePaymentRequest(
          orderId: 'order-123',
          amount: 59.99,
          paymentMethod: PaymentMethod.card,
        );
        final paymentResult = PaymentResult(
          id: 'payment-123',
          orderId: 'order-123',
          status: PaymentStatus.paid,
          createdAt: DateTime.now(),
        );
        when(
          mockDataSource.processPayment(any),
        ).thenAnswer((_) async => paymentResult);

        // Act
        final result = await repository.processPayment(request);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, paymentResult);
      });
    });

    group('getPaymentStatus', () {
      test('returns success with payment status', () async {
        // Arrange
        final paymentResult = PaymentResult(
          id: 'payment-123',
          orderId: 'order-123',
          status: PaymentStatus.paid,
          createdAt: DateTime.now(),
        );
        when(
          mockDataSource.getPaymentStatus(any),
        ).thenAnswer((_) async => paymentResult);

        // Act
        final result = await repository.getPaymentStatus('payment-123');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, paymentResult);
      });
    });
  });
}
