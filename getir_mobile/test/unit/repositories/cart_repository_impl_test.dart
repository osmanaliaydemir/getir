import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:dio/dio.dart';
import 'package:getir_mobile/data/repositories/cart_repository_impl.dart';
import 'package:getir_mobile/data/datasources/cart_datasource.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([CartDataSource])
import 'cart_repository_impl_test.mocks.dart';

void main() {
  late CartRepositoryImpl repository;
  late MockCartDataSource mockDataSource;

  setUp(() {
    mockDataSource = MockCartDataSource();
    repository = CartRepositoryImpl(mockDataSource);
  });

  group('CartRepositoryImpl -', () {
    group('getCart', () {
      test('returns success with cart when datasource succeeds', () async {
        // Arrange
        when(
          mockDataSource.getCart(),
        ).thenAnswer((_) async => MockData.testCart);

        // Act
        final result = await repository.getCart();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testCart);
        verify(mockDataSource.getCart()).called(1);
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/cart'),
          type: DioExceptionType.connectionTimeout,
        );
        when(mockDataSource.getCart()).thenThrow(dioException);

        // Act
        final result = await repository.getCart();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });

      test('returns failure when AppException occurs', () async {
        // Arrange
        const appException = ApiException(message: 'Cart error');
        when(mockDataSource.getCart()).thenThrow(appException);

        // Act
        final result = await repository.getCart();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, appException);
      });

      test('wraps generic exception as ApiException', () async {
        // Arrange
        when(mockDataSource.getCart()).thenThrow(Exception('Unknown error'));

        // Act
        final result = await repository.getCart();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<ApiException>());
      });
    });

    group('addToCart', () {
      test('returns success with cart item when added', () async {
        // Arrange
        when(
          mockDataSource.addToCart(
            merchantId: anyNamed('merchantId'),
            productId: anyNamed('productId'),
            quantity: anyNamed('quantity'),
            notes: anyNamed('notes'),
          ),
        ).thenAnswer((_) async => MockData.testCartItem);

        // Act
        final result = await repository.addToCart(
          merchantId: 'merchant-123',
          productId: 'product-123',
          quantity: 2,
        );

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testCartItem);
      });

      test('handles DioException correctly', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/cart'),
          type: DioExceptionType.badResponse,
          response: Response(
            requestOptions: RequestOptions(path: '/cart'),
            statusCode: 404,
            data: {'message': 'Product not found'},
          ),
        );
        when(
          mockDataSource.addToCart(
            merchantId: anyNamed('merchantId'),
            productId: anyNamed('productId'),
            quantity: anyNamed('quantity'),
            notes: anyNamed('notes'),
          ),
        ).thenThrow(dioException);

        // Act
        final result = await repository.addToCart(
          merchantId: 'merchant-123',
          productId: 'product-123',
          quantity: 1,
        );

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });
    });

    group('updateCartItem', () {
      test('returns success when cart item updated', () async {
        // Arrange
        when(
          mockDataSource.updateCartItem(
            itemId: anyNamed('itemId'),
            quantity: anyNamed('quantity'),
          ),
        ).thenAnswer((_) async => MockData.testCartItem);

        // Act
        final result = await repository.updateCartItem(
          itemId: 'item-123',
          quantity: 3,
        );

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testCartItem);
      });
    });

    group('removeFromCart', () {
      test('returns success when item removed', () async {
        // Arrange
        when(mockDataSource.removeFromCart(any)).thenAnswer((_) async => {});

        // Act
        final result = await repository.removeFromCart('item-123');

        // Assert
        expect(result.isSuccess, true);
        verify(mockDataSource.removeFromCart('item-123')).called(1);
      });
    });

    group('clearCart', () {
      test('returns success when cart cleared', () async {
        // Arrange
        when(mockDataSource.clearCart()).thenAnswer((_) async => {});

        // Act
        final result = await repository.clearCart();

        // Assert
        expect(result.isSuccess, true);
        verify(mockDataSource.clearCart()).called(1);
      });
    });

    group('applyCoupon', () {
      test('returns success with updated cart', () async {
        // Arrange
        final cartWithCoupon = MockData.testCart.copyWith(
          couponCode: 'SAVE10',
          discountAmount: 10.0,
        );
        when(
          mockDataSource.applyCoupon(any),
        ).thenAnswer((_) async => cartWithCoupon);

        // Act
        final result = await repository.applyCoupon('SAVE10');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data!.couponCode, 'SAVE10');
      });
    });

    group('removeCoupon', () {
      test('returns success when coupon removed', () async {
        // Arrange
        when(
          mockDataSource.removeCoupon(),
        ).thenAnswer((_) async => MockData.testCart);

        // Act
        final result = await repository.removeCoupon();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testCart);
      });
    });
  });
}
