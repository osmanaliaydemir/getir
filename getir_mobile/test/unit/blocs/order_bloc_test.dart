import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:getir_mobile/presentation/bloc/order/order_bloc.dart';
import 'package:getir_mobile/domain/services/order_service.dart';
import 'package:getir_mobile/domain/entities/order.dart';
import 'package:getir_mobile/data/datasources/order_datasource.dart';
import 'package:getir_mobile/core/services/analytics_service.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([OrderService, AnalyticsService])
import 'order_bloc_test.mocks.dart';

void main() {
  late OrderBloc orderBloc;
  late MockOrderService mockOrderService;
  late MockAnalyticsService mockAnalytics;

  setUp(() {
    mockOrderService = MockOrderService();
    mockAnalytics = MockAnalyticsService();
    orderBloc = OrderBloc(mockOrderService, mockAnalytics);
  });

  tearDown(() {
    orderBloc.close();
  });

  group('OrderBloc -', () {
    group('LoadUserOrders', () {
      test('initial state is OrderInitial', () {
        expect(orderBloc.state, equals(OrderInitial()));
      });

      blocTest<OrderBloc, OrderState>(
        'emits [OrderLoading, OrdersLoaded] when orders are loaded',
        build: () {
          when(
            mockOrderService.getUserOrders(),
          ).thenAnswer((_) async => Result.success([MockData.testOrder]));
          return orderBloc;
        },
        act: (bloc) => bloc.add(LoadUserOrders()),
        expect: () => [
          OrderLoading(),
          OrdersLoaded([MockData.testOrder]),
        ],
        verify: (_) {
          verify(mockOrderService.getUserOrders()).called(1);
        },
      );

      blocTest<OrderBloc, OrderState>(
        'emits [OrderLoading, OrdersLoaded] with empty list',
        build: () {
          when(
            mockOrderService.getUserOrders(),
          ).thenAnswer((_) async => Result.success([]));
          return orderBloc;
        },
        act: (bloc) => bloc.add(LoadUserOrders()),
        expect: () => [OrderLoading(), const OrdersLoaded([])],
      );

      blocTest<OrderBloc, OrderState>(
        'emits [OrderLoading, OrderError] when loading fails',
        build: () {
          when(mockOrderService.getUserOrders()).thenAnswer(
            (_) async => Result.failure(
              const NetworkException(message: 'Connection failed'),
            ),
          );
          return orderBloc;
        },
        act: (bloc) => bloc.add(LoadUserOrders()),
        expect: () => [OrderLoading(), const OrderError('Connection failed')],
      );
    });

    group('LoadOrderById', () {
      const String testOrderId = 'order-123';

      blocTest<OrderBloc, OrderState>(
        'emits [OrderLoading, OrderLoaded] when order is loaded',
        build: () {
          when(
            mockOrderService.getOrderById(testOrderId),
          ).thenAnswer((_) async => Result.success(MockData.testOrder));
          return orderBloc;
        },
        act: (bloc) => bloc.add(const LoadOrderById(testOrderId)),
        expect: () => [OrderLoading(), OrderLoaded(MockData.testOrder)],
        verify: (_) {
          verify(mockOrderService.getOrderById(testOrderId)).called(1);
        },
      );

      blocTest<OrderBloc, OrderState>(
        'emits [OrderLoading, OrderError] when order not found',
        build: () {
          when(mockOrderService.getOrderById(testOrderId)).thenAnswer(
            (_) async => Result.failure(
              const NotFoundException(
                message: 'Order not found',
                code: 'ORDER_NOT_FOUND',
              ),
            ),
          );
          return orderBloc;
        },
        act: (bloc) => bloc.add(const LoadOrderById(testOrderId)),
        expect: () => [OrderLoading(), const OrderError('Order not found')],
      );
    });

    group('CreateOrder', () {
      final testRequest = CreateOrderRequest(
        merchantId: 'merchant-123',
        deliveryAddressId: 'address-123',
        paymentMethod: PaymentMethod.card,
        notes: 'Test order',
        items: [
          const CreateOrderItemRequest(
            productId: 'product-123',
            quantity: 1,
            selectedVariantId: null,
            selectedOptionIds: [],
          ),
        ],
      );

      blocTest<OrderBloc, OrderState>(
        'emits [OrderLoading, OrderCreated] when order is created successfully',
        build: () {
          when(
            mockOrderService.createOrder(any),
          ).thenAnswer((_) async => Result.success(MockData.testOrder));

          when(
            mockAnalytics.logPurchase(
              orderId: anyNamed('orderId'),
              total: anyNamed('total'),
              currency: anyNamed('currency'),
              items: anyNamed('items'),
              shipping: anyNamed('shipping'),
            ),
          ).thenAnswer((_) async => Future.value());

          return orderBloc;
        },
        act: (bloc) => bloc.add(CreateOrder(testRequest)),
        expect: () => [OrderLoading(), OrderCreated(MockData.testOrder)],
        verify: (_) {
          verify(mockOrderService.createOrder(testRequest)).called(1);
          verify(
            mockAnalytics.logPurchase(
              orderId: MockData.testOrder.id,
              total: MockData.testOrder.totalAmount,
              currency: 'TRY',
              items: anyNamed('items'),
              shipping: MockData.testOrder.deliveryFee,
            ),
          ).called(1);
        },
      );

      blocTest<OrderBloc, OrderState>(
        'emits [OrderLoading, OrderError] and logs error when creation fails',
        build: () {
          when(mockOrderService.createOrder(any)).thenAnswer(
            (_) async => Result.failure(
              const ValidationException(
                message: 'Cart is empty',
                code: 'EMPTY_CART',
              ),
            ),
          );

          when(
            mockAnalytics.logError(
              error: anyNamed('error'),
              reason: anyNamed('reason'),
              stackTrace: anyNamed('stackTrace'),
              fatal: anyNamed('fatal'),
            ),
          ).thenAnswer((_) async => Future.value());

          return orderBloc;
        },
        act: (bloc) => bloc.add(CreateOrder(testRequest)),
        expect: () => [OrderLoading(), const OrderError('Cart is empty')],
        verify: (_) {
          verify(
            mockAnalytics.logError(
              error: anyNamed('error'),
              reason: 'Order creation failed',
              stackTrace: anyNamed('stackTrace'),
              fatal: anyNamed('fatal'),
            ),
          ).called(1);
        },
      );
    });

    group('CancelOrder', () {
      const String testOrderId = 'order-123';
      final cancelledOrder = Order(
        id: testOrderId,
        userId: 'test-user-123',
        merchantId: 'merchant-123',
        merchantName: 'Test Market',
        merchantLogoUrl: null,
        deliveryAddressId: 'address-123',
        deliveryAddress: 'Test Address',
        deliveryLatitude: 40.9923,
        deliveryLongitude: 29.0287,
        status: OrderStatus.cancelled,
        paymentStatus: PaymentStatus.refunded,
        paymentMethod: PaymentMethod.card,
        subtotal: 29.99,
        deliveryFee: 9.99,
        discountAmount: 0.0,
        totalAmount: 39.98,
        couponCode: null,
        notes: null,
        estimatedDeliveryTime: DateTime(2025, 10, 7, 12, 30),
        createdAt: DateTime(2025, 10, 7, 12, 0),
        updatedAt: DateTime(2025, 10, 7, 12, 5),
        items: [MockData.testOrderItem],
        statusHistory: [MockData.testOrderStatusHistory],
      );

      blocTest<OrderBloc, OrderState>(
        'emits [OrderLoading, OrderCancelled] when order is cancelled',
        build: () {
          when(
            mockOrderService.cancelOrder(testOrderId),
          ).thenAnswer((_) async => Result.success(cancelledOrder));

          when(
            mockAnalytics.logOrderCancelled(
              orderId: anyNamed('orderId'),
              value: anyNamed('value'),
              reason: anyNamed('reason'),
            ),
          ).thenAnswer((_) async => Future.value());

          return orderBloc;
        },
        act: (bloc) => bloc.add(const CancelOrder(testOrderId)),
        expect: () => [OrderLoading(), OrderCancelled(cancelledOrder)],
        verify: (_) {
          verify(mockOrderService.cancelOrder(testOrderId)).called(1);
          verify(
            mockAnalytics.logOrderCancelled(
              orderId: cancelledOrder.id,
              value: cancelledOrder.totalAmount,
              reason: 'user_cancelled',
            ),
          ).called(1);
        },
      );

      blocTest<OrderBloc, OrderState>(
        'emits [OrderLoading, OrderError] when cancellation fails',
        build: () {
          when(mockOrderService.cancelOrder(testOrderId)).thenAnswer(
            (_) async => Result.failure(
              const ValidationException(
                message: 'Order cannot be cancelled',
                code: 'CANNOT_CANCEL',
              ),
            ),
          );

          when(
            mockAnalytics.logError(
              error: anyNamed('error'),
              reason: anyNamed('reason'),
              stackTrace: anyNamed('stackTrace'),
              fatal: anyNamed('fatal'),
            ),
          ).thenAnswer((_) async => Future.value());

          return orderBloc;
        },
        act: (bloc) => bloc.add(const CancelOrder(testOrderId)),
        expect: () => [
          OrderLoading(),
          const OrderError('Order cannot be cancelled'),
        ],
      );
    });

    group('ProcessPayment', () {
      final testPaymentRequest = CreatePaymentRequest(
        orderId: 'order-123',
        amount: 39.98,
        paymentMethod: PaymentMethod.card,
        paymentDetails: {'cardToken': 'test-card-token-123'},
      );

      final testPaymentResult = PaymentResult(
        id: 'payment-123',
        orderId: 'order-123',
        status: PaymentStatus.paid,
        message: 'Payment successful',
        transactionId: 'txn-123',
        createdAt: DateTime(2025, 10, 7, 12, 0),
      );

      blocTest<OrderBloc, OrderState>(
        'emits [OrderLoading, PaymentProcessed] when payment succeeds',
        build: () {
          when(
            mockOrderService.processPayment(any),
          ).thenAnswer((_) async => Result.success(testPaymentResult));

          when(
            mockAnalytics.logAddPaymentInfo(
              paymentType: anyNamed('paymentType'),
              value: anyNamed('value'),
              currency: anyNamed('currency'),
            ),
          ).thenAnswer((_) async => Future.value());

          return orderBloc;
        },
        act: (bloc) => bloc.add(ProcessPayment(testPaymentRequest)),
        expect: () => [OrderLoading(), PaymentProcessed(testPaymentResult)],
        verify: (_) {
          verify(mockOrderService.processPayment(testPaymentRequest)).called(1);
          verify(
            mockAnalytics.logAddPaymentInfo(
              paymentType: PaymentMethod.card.value,
              value: testPaymentRequest.amount,
              currency: 'TRY',
            ),
          ).called(1);
        },
      );

      blocTest<OrderBloc, OrderState>(
        'emits [OrderLoading, OrderError] when payment fails',
        build: () {
          when(mockOrderService.processPayment(any)).thenAnswer(
            (_) async => Result.failure(
              const ServerException(
                message: 'Payment gateway error',
                code: 'PAYMENT_ERROR',
              ),
            ),
          );

          when(
            mockAnalytics.logError(
              error: anyNamed('error'),
              reason: anyNamed('reason'),
              stackTrace: anyNamed('stackTrace'),
              fatal: anyNamed('fatal'),
            ),
          ).thenAnswer((_) async => Future.value());

          return orderBloc;
        },
        act: (bloc) => bloc.add(ProcessPayment(testPaymentRequest)),
        expect: () => [
          OrderLoading(),
          const OrderError('Payment gateway error'),
        ],
      );
    });

    group('GetPaymentStatus', () {
      const String testPaymentId = 'payment-123';
      final testPaymentResult = PaymentResult(
        id: testPaymentId,
        orderId: 'order-123',
        status: PaymentStatus.paid,
        message: 'Payment successful',
        transactionId: 'txn-123',
        createdAt: DateTime(2025, 10, 7, 12, 0),
      );

      blocTest<OrderBloc, OrderState>(
        'emits [OrderLoading, PaymentStatusLoaded] when status is retrieved',
        build: () {
          when(
            mockOrderService.getPaymentStatus(testPaymentId),
          ).thenAnswer((_) async => Result.success(testPaymentResult));
          return orderBloc;
        },
        act: (bloc) => bloc.add(const GetPaymentStatus(testPaymentId)),
        expect: () => [OrderLoading(), PaymentStatusLoaded(testPaymentResult)],
        verify: (_) {
          verify(mockOrderService.getPaymentStatus(testPaymentId)).called(1);
        },
      );

      blocTest<OrderBloc, OrderState>(
        'emits [OrderLoading, OrderError] when status retrieval fails',
        build: () {
          when(mockOrderService.getPaymentStatus(testPaymentId)).thenAnswer(
            (_) async => Result.failure(
              const NotFoundException(
                message: 'Payment not found',
                code: 'PAYMENT_NOT_FOUND',
              ),
            ),
          );
          return orderBloc;
        },
        act: (bloc) => bloc.add(const GetPaymentStatus(testPaymentId)),
        expect: () => [OrderLoading(), const OrderError('Payment not found')],
      );
    });
  });
}
