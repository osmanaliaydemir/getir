import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:getir_mobile/domain/services/cart_service.dart';
import 'package:getir_mobile/domain/repositories/cart_repository.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([CartRepository])
import 'cart_service_test.mocks.dart';

void main() {
  late CartService cartService;
  late MockCartRepository mockRepository;

  setUp(() {
    mockRepository = MockCartRepository();
    cartService = CartService(mockRepository);
  });

  group('CartService -', () {
    group('getCart', () {
      test('returns cart when repository succeeds', () async {
        // Arrange
        when(
          mockRepository.getCart(),
        ).thenAnswer((_) async => Result.success(MockData.testCart));

        // Act
        final result = await cartService.getCart();

        // Assert
        expect(result.isSuccess, isTrue);
        expect(result.data.id, equals('cart-123'));
        expect(result.data.items.length, equals(1));
        verify(mockRepository.getCart()).called(1);
      });

      test('returns empty cart', () async {
        // Arrange
        when(
          mockRepository.getCart(),
        ).thenAnswer((_) async => Result.success(MockData.emptyCart));

        // Act
        final result = await cartService.getCart();

        // Assert
        expect(result.isSuccess, isTrue);
        expect(result.data.items, isEmpty);
      });

      test('propagates error from repository', () async {
        // Arrange
        when(mockRepository.getCart()).thenAnswer(
          (_) async => Result.failure(
            const NetworkException(message: 'Connection failed'),
          ),
        );

        // Act
        final result = await cartService.getCart();

        // Assert
        expect(result.isFailure, isTrue);
        expect(result.exception, isA<NetworkException>());
      });
    });

    group('addToCart', () {
      const String testProductId = 'product-123';

      test('validates quantity must be greater than zero', () async {
        // Act
        final result = await cartService.addToCart(
          productId: testProductId,
          quantity: 0,
        );

        // Assert
        expect(result.isFailure, isTrue);
        expect(result.exception, isA<ValidationException>());
      });

      test('validates quantity must be positive', () async {
        // Act
        final result = await cartService.addToCart(
          productId: testProductId,
          quantity: -1,
        );

        // Assert
        expect(result.isFailure, isTrue);
      });

      test('adds item when validation passes', () async {
        // Arrange
        when(
          mockRepository.addToCart(
            productId: testProductId,
            quantity: 2,
            variantId: null,
            optionIds: null,
          ),
        ).thenAnswer((_) async => Result.success(MockData.testCartItem));

        // Act
        final result = await cartService.addToCart(
          productId: testProductId,
          quantity: 2,
        );

        // Assert
        expect(result.isSuccess, isTrue);
      });
    });

    group('updateCartItem', () {
      const String testItemId = 'cart-item-123';

      test('validates quantity must be greater than zero', () async {
        // Act
        final result = await cartService.updateCartItem(
          cartItemId: testItemId,
          quantity: 0,
        );

        // Assert
        expect(result.isFailure, isTrue);
        expect(result.exception, isA<ValidationException>());
      });

      test('updates item when validation passes', () async {
        // Arrange
        when(
          mockRepository.updateCartItem(itemId: testItemId, quantity: 3),
        ).thenAnswer((_) async => Result.success(MockData.testCartItem));

        // Act
        final result = await cartService.updateCartItem(
          cartItemId: testItemId,
          quantity: 3,
        );

        // Assert
        expect(result.isSuccess, isTrue);
      });
    });

    group('applyCoupon', () {
      const String testCouponCode = 'SAVE10';

      test('validates coupon code is not empty', () async {
        // Act
        final result = await cartService.applyCoupon('');

        // Assert
        expect(result.isFailure, isTrue);
        expect(result.exception, isA<ValidationException>());
      });

      test('validates coupon code is not whitespace', () async {
        // Act
        final result = await cartService.applyCoupon('   ');

        // Assert
        expect(result.isFailure, isTrue);
      });

      test('trims whitespace from coupon code', () async {
        // Arrange
        final discountedCart = MockData.testCart.copyWith(
          couponCode: testCouponCode,
          discountAmount: 10.0,
        );
        when(
          mockRepository.applyCoupon('SAVE10'),
        ).thenAnswer((_) async => Result.success(discountedCart));

        // Act
        final result = await cartService.applyCoupon('  SAVE10  ');

        // Assert
        expect(result.isSuccess, isTrue);
        verify(mockRepository.applyCoupon('SAVE10')).called(1);
      });
    });
  });
}
