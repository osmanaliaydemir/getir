import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:getir_mobile/domain/services/order_service.dart';
import 'package:getir_mobile/domain/repositories/order_repository.dart';
import 'package:getir_mobile/domain/entities/order.dart';
import 'package:getir_mobile/data/datasources/order_datasource.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([IOrderRepository])
import 'order_service_test.mocks.dart';

void main() {
  late OrderService service;
  late MockIOrderRepository mockRepository;

  setUp(() {
    mockRepository = MockIOrderRepository();
    service = OrderService(mockRepository);
  });

  group('OrderService -', () {
    group('createOrder', () {
      test('creates order successfully', () async {
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
          mockRepository.createOrder(any),
        ).thenAnswer((_) async => Result.success(MockData.testOrder));

        // Act
        final result = await service.createOrder(request);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testOrder);
        verify(mockRepository.createOrder(request)).called(1);
      });

      test('returns failure when creation fails', () async {
        // Arrange
        final request = CreateOrderRequest(
          merchantId: 'merchant-123',
          deliveryAddressId: 'address-123',
          paymentMethod: PaymentMethod.cash,
          items: [],
        );
        const exception = ValidationException(message: 'Invalid order data');
        when(
          mockRepository.createOrder(any),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.createOrder(request);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('getUserOrders', () {
      test('returns user orders successfully', () async {
        // Arrange
        final orders = [MockData.testOrder];
        when(
          mockRepository.getUserOrders(),
        ).thenAnswer((_) async => Result.success(orders));

        // Act
        final result = await service.getUserOrders();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, orders);
        expect(result.data?.length, 1);
        verify(mockRepository.getUserOrders()).called(1);
      });

      test('returns empty list when user has no orders', () async {
        // Arrange
        when(
          mockRepository.getUserOrders(),
        ).thenAnswer((_) async => Result.success([]));

        // Act
        final result = await service.getUserOrders();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, isEmpty);
      });

      test('propagates error from repository', () async {
        // Arrange
        const exception = NetworkException(message: 'Failed to load orders');
        when(
          mockRepository.getUserOrders(),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getUserOrders();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('getOrderById', () {
      test('returns order when found', () async {
        // Arrange
        when(
          mockRepository.getOrderById(any),
        ).thenAnswer((_) async => Result.success(MockData.testOrder));

        // Act
        final result = await service.getOrderById('order-123');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testOrder);
        verify(mockRepository.getOrderById('order-123')).called(1);
      });

      test('returns failure when order not found', () async {
        // Arrange
        const exception = NotFoundException(message: 'Order not found');
        when(
          mockRepository.getOrderById(any),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getOrderById('invalid-id');

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('cancelOrder', () {
      test('cancels order successfully', () async {
        // Arrange
        final cancelledOrder = MockData.testOrder;
        when(
          mockRepository.cancelOrder(any),
        ).thenAnswer((_) async => Result.success(cancelledOrder));

        // Act
        final result = await service.cancelOrder('order-123');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, cancelledOrder);
        verify(mockRepository.cancelOrder('order-123')).called(1);
      });

      test('returns failure when cancellation not allowed', () async {
        // Arrange
        const exception = BusinessException(
          message: 'Order cannot be cancelled',
        );
        when(
          mockRepository.cancelOrder(any),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.cancelOrder('order-123');

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('processPayment', () {
      test('processes payment successfully', () async {
        // Arrange
        final request = CreatePaymentRequest(
          orderId: 'order-123',
          amount: 59.99,
          paymentMethod: PaymentMethod.card,
          paymentDetails: {'cardToken': 'tok_123'},
        );
        final paymentResult = PaymentResult(
          id: 'payment-123',
          orderId: 'order-123',
          status: PaymentStatus.paid,
          createdAt: DateTime.now(),
        );
        when(
          mockRepository.processPayment(any),
        ).thenAnswer((_) async => Result.success(paymentResult));

        // Act
        final result = await service.processPayment(request);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, paymentResult);
        verify(mockRepository.processPayment(request)).called(1);
      });

      test('returns failure when payment fails', () async {
        // Arrange
        final request = CreatePaymentRequest(
          orderId: 'order-123',
          amount: 59.99,
          paymentMethod: PaymentMethod.card,
          paymentDetails: {},
        );
        const exception = ApiException(message: 'Payment declined');
        when(
          mockRepository.processPayment(any),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.processPayment(request);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('getPaymentStatus', () {
      test('returns payment status successfully', () async {
        // Arrange
        final paymentResult = PaymentResult(
          id: 'payment-123',
          orderId: 'order-123',
          status: PaymentStatus.paid,
          createdAt: DateTime.now(),
        );
        when(
          mockRepository.getPaymentStatus(any),
        ).thenAnswer((_) async => Result.success(paymentResult));

        // Act
        final result = await service.getPaymentStatus('payment-123');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, paymentResult);
        verify(mockRepository.getPaymentStatus('payment-123')).called(1);
      });

      test('returns failure when payment not found', () async {
        // Arrange
        const exception = NotFoundException(message: 'Payment not found');
        when(
          mockRepository.getPaymentStatus(any),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getPaymentStatus('invalid-id');

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });
  });
}
