import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:getir_mobile/presentation/bloc/cart/cart_bloc.dart';
import 'package:getir_mobile/domain/services/cart_service.dart';
import 'package:getir_mobile/domain/entities/cart.dart';
import 'package:getir_mobile/core/services/analytics_service.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([CartService, AnalyticsService])
import 'cart_bloc_test.mocks.dart';

void main() {
  late CartBloc cartBloc;
  late MockCartService mockCartService;
  late MockAnalyticsService mockAnalytics;

  setUp(() {
    mockCartService = MockCartService();
    mockAnalytics = MockAnalyticsService();
    cartBloc = CartBloc(mockCartService, mockAnalytics);
  });

  tearDown(() {
    cartBloc.close();
  });

  group('CartBloc -', () {
    group('LoadCart', () {
      test('initial state is CartInitial', () {
        expect(cartBloc.state, equals(CartInitial()));
      });

      blocTest<CartBloc, CartState>(
        'emits [CartLoading, CartLoaded] when cart is loaded successfully',
        build: () {
          when(
            mockCartService.getCart(),
          ).thenAnswer((_) async => Result.success(MockData.testCart));
          return cartBloc;
        },
        act: (bloc) => bloc.add(LoadCart()),
        expect: () => [CartLoading(), CartLoaded(MockData.testCart)],
        verify: (_) {
          verify(mockCartService.getCart()).called(1);
        },
      );

      blocTest<CartBloc, CartState>(
        'emits [CartLoading, CartLoaded] with empty cart',
        build: () {
          when(
            mockCartService.getCart(),
          ).thenAnswer((_) async => Result.success(MockData.emptyCart));
          return cartBloc;
        },
        act: (bloc) => bloc.add(LoadCart()),
        expect: () => [CartLoading(), CartLoaded(MockData.emptyCart)],
      );

      blocTest<CartBloc, CartState>(
        'emits [CartLoading, CartError] when loading fails',
        build: () {
          when(mockCartService.getCart()).thenAnswer(
            (_) async => Result.failure(
              const NetworkException(message: 'Connection failed'),
            ),
          );
          return cartBloc;
        },
        act: (bloc) => bloc.add(LoadCart()),
        expect: () => [CartLoading(), const CartError('Connection failed')],
      );
    });

    group('AddToCart', () {
      const String testProductId = 'product-123';
      const int testQuantity = 2;
      const String testProductName = 'Test Product';
      const double testPrice = 29.99;

      blocTest<CartBloc, CartState>(
        'emits [CartItemAdded, CartLoaded] when item is added successfully',
        build: () {
          when(
            mockCartService.addToCart(
              productId: testProductId,
              quantity: testQuantity,
              variantId: null,
              optionIds: null,
            ),
          ).thenAnswer((_) async => Result.success(MockData.testCartItem));

          when(
            mockCartService.getCart(),
          ).thenAnswer((_) async => Result.success(MockData.testCart));

          when(
            mockAnalytics.logAddToCart(
              productId: anyNamed('productId'),
              productName: anyNamed('productName'),
              price: anyNamed('price'),
              category: anyNamed('category'),
              quantity: anyNamed('quantity'),
            ),
          ).thenAnswer((_) async => Future.value());

          return cartBloc;
        },
        act: (bloc) => bloc.add(
          const AddToCart(
            productId: testProductId,
            quantity: testQuantity,
            productName: testProductName,
            price: testPrice,
          ),
        ),
        expect: () => [
          CartItemAdded(MockData.testCartItem),
          CartLoaded(MockData.testCart),
        ],
        verify: (_) {
          verify(
            mockCartService.addToCart(
              productId: testProductId,
              quantity: testQuantity,
              variantId: null,
              optionIds: null,
            ),
          ).called(1);
          verify(
            mockAnalytics.logAddToCart(
              productId: testProductId,
              productName: testProductName,
              price: testPrice,
              category: null,
              quantity: testQuantity,
            ),
          ).called(1);
        },
      );

      blocTest<CartBloc, CartState>(
        'adds item without analytics when product info is missing',
        build: () {
          when(
            mockCartService.addToCart(
              productId: testProductId,
              quantity: testQuantity,
              variantId: null,
              optionIds: null,
            ),
          ).thenAnswer((_) async => Result.success(MockData.testCartItem));

          when(
            mockCartService.getCart(),
          ).thenAnswer((_) async => Result.success(MockData.testCart));

          return cartBloc;
        },
        act: (bloc) => bloc.add(
          const AddToCart(productId: testProductId, quantity: testQuantity),
        ),
        expect: () => [
          CartItemAdded(MockData.testCartItem),
          CartLoaded(MockData.testCart),
        ],
        verify: (_) {
          verifyNever(
            mockAnalytics.logAddToCart(
              productId: anyNamed('productId'),
              productName: anyNamed('productName'),
              price: anyNamed('price'),
              category: anyNamed('category'),
              quantity: anyNamed('quantity'),
            ),
          );
        },
      );

      blocTest<CartBloc, CartState>(
        'emits [CartError] when add to cart fails',
        build: () {
          when(
            mockCartService.addToCart(
              productId: testProductId,
              quantity: testQuantity,
              variantId: null,
              optionIds: null,
            ),
          ).thenAnswer(
            (_) async => Result.failure(
              const ValidationException(
                message: 'Product not available',
                code: 'PRODUCT_UNAVAILABLE',
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

          return cartBloc;
        },
        act: (bloc) => bloc.add(
          const AddToCart(productId: testProductId, quantity: testQuantity),
        ),
        expect: () => [const CartError('Product not available')],
      );

      blocTest<CartBloc, CartState>(
        'adds item with variant',
        build: () {
          const String variantId = 'variant-123';

          when(
            mockCartService.addToCart(
              productId: testProductId,
              quantity: testQuantity,
              variantId: variantId,
              optionIds: null,
            ),
          ).thenAnswer((_) async => Result.success(MockData.testCartItem));

          when(
            mockCartService.getCart(),
          ).thenAnswer((_) async => Result.success(MockData.testCart));

          return cartBloc;
        },
        act: (bloc) => bloc.add(
          const AddToCart(
            productId: testProductId,
            quantity: testQuantity,
            variantId: 'variant-123',
          ),
        ),
        expect: () => [
          CartItemAdded(MockData.testCartItem),
          CartLoaded(MockData.testCart),
        ],
      );
    });

    group('UpdateCartItem', () {
      const String testItemId = 'cart-item-123';
      const int newQuantity = 3;

      blocTest<CartBloc, CartState>(
        'emits [CartItemUpdated, CartLoaded] when item is updated',
        build: () {
          when(
            mockCartService.updateCartItem(
              cartItemId: testItemId,
              quantity: newQuantity,
            ),
          ).thenAnswer((_) async => Result.success(MockData.testCartItem));

          when(
            mockCartService.getCart(),
          ).thenAnswer((_) async => Result.success(MockData.testCart));

          return cartBloc;
        },
        act: (bloc) => bloc.add(
          const UpdateCartItem(itemId: testItemId, quantity: newQuantity),
        ),
        expect: () => [
          CartItemUpdated(MockData.testCartItem),
          CartLoaded(MockData.testCart),
        ],
        verify: (_) {
          verify(
            mockCartService.updateCartItem(
              cartItemId: testItemId,
              quantity: newQuantity,
            ),
          ).called(1);
        },
      );

      blocTest<CartBloc, CartState>(
        'emits [CartError] when update fails',
        build: () {
          when(
            mockCartService.updateCartItem(
              cartItemId: testItemId,
              quantity: newQuantity,
            ),
          ).thenAnswer(
            (_) async => Result.failure(
              const NotFoundException(
                message: 'Cart item not found',
                code: 'ITEM_NOT_FOUND',
              ),
            ),
          );

          return cartBloc;
        },
        act: (bloc) => bloc.add(
          const UpdateCartItem(itemId: testItemId, quantity: newQuantity),
        ),
        expect: () => [const CartError('Cart item not found')],
      );
    });

    group('RemoveFromCart', () {
      const String testItemId = 'cart-item-123';

      blocTest<CartBloc, CartState>(
        'emits [CartItemRemoved, CartLoaded] when item is removed',
        build: () {
          when(
            mockCartService.removeFromCart(testItemId),
          ).thenAnswer((_) async => Result.success(null));

          when(
            mockCartService.getCart(),
          ).thenAnswer((_) async => Result.success(MockData.emptyCart));

          when(
            mockAnalytics.logCustomEvent(
              eventName: anyNamed('eventName'),
              parameters: anyNamed('parameters'),
            ),
          ).thenAnswer((_) async => Future.value());

          return cartBloc;
        },
        act: (bloc) => bloc.add(const RemoveFromCart(testItemId)),
        expect: () => [
          const CartItemRemoved(testItemId),
          CartLoaded(MockData.emptyCart),
        ],
        verify: (_) {
          verify(mockCartService.removeFromCart(testItemId)).called(1);
          verify(
            mockAnalytics.logCustomEvent(
              eventName: 'remove_from_cart',
              parameters: {'item_id': testItemId},
            ),
          ).called(1);
        },
      );

      blocTest<CartBloc, CartState>(
        'emits [CartError] when remove fails',
        build: () {
          when(mockCartService.removeFromCart(testItemId)).thenAnswer(
            (_) async => Result.failure(
              const NetworkException(message: 'Connection failed'),
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

          return cartBloc;
        },
        act: (bloc) => bloc.add(const RemoveFromCart(testItemId)),
        expect: () => [const CartError('Connection failed')],
      );
    });

    group('ClearCart', () {
      blocTest<CartBloc, CartState>(
        'emits [CartCleared, CartLoaded] when cart is cleared',
        build: () {
          when(
            mockCartService.clearCart(),
          ).thenAnswer((_) async => Result.success(null));

          when(
            mockCartService.getCart(),
          ).thenAnswer((_) async => Result.success(MockData.emptyCart));

          return cartBloc;
        },
        act: (bloc) => bloc.add(ClearCart()),
        expect: () => [CartCleared(), CartLoaded(MockData.emptyCart)],
        verify: (_) {
          verify(mockCartService.clearCart()).called(1);
        },
      );

      blocTest<CartBloc, CartState>(
        'emits [CartError] when clear fails',
        build: () {
          when(mockCartService.clearCart()).thenAnswer(
            (_) async => Result.failure(
              const ServerException(message: 'Server error', code: '500'),
            ),
          );

          return cartBloc;
        },
        act: (bloc) => bloc.add(ClearCart()),
        expect: () => [const CartError('Server error')],
      );
    });

    group('ApplyCoupon', () {
      const String testCouponCode = 'SAVE10';
      final Cart discountedCart = MockData.testCart.copyWith(
        couponCode: testCouponCode,
        discountAmount: 10.0,
        total: 29.98,
      );

      blocTest<CartBloc, CartState>(
        'emits [CartLoaded] when coupon is applied successfully',
        build: () {
          when(
            mockCartService.applyCoupon(testCouponCode),
          ).thenAnswer((_) async => Result.success(discountedCart));

          return cartBloc;
        },
        act: (bloc) => bloc.add(const ApplyCoupon(testCouponCode)),
        expect: () => [CartLoaded(discountedCart)],
        verify: (_) {
          verify(mockCartService.applyCoupon(testCouponCode)).called(1);
        },
      );

      blocTest<CartBloc, CartState>(
        'emits [CartError] when coupon is invalid',
        build: () {
          when(mockCartService.applyCoupon(testCouponCode)).thenAnswer(
            (_) async => Result.failure(
              const ValidationException(
                message: 'Invalid or expired coupon',
                code: 'INVALID_COUPON',
              ),
            ),
          );

          return cartBloc;
        },
        act: (bloc) => bloc.add(const ApplyCoupon(testCouponCode)),
        expect: () => [const CartError('Invalid or expired coupon')],
      );

      blocTest<CartBloc, CartState>(
        'emits [CartError] when minimum order amount not met',
        build: () {
          when(mockCartService.applyCoupon(testCouponCode)).thenAnswer(
            (_) async => Result.failure(
              const ValidationException(
                message: 'Minimum order amount not met',
                code: 'MIN_ORDER_NOT_MET',
              ),
            ),
          );

          return cartBloc;
        },
        act: (bloc) => bloc.add(const ApplyCoupon(testCouponCode)),
        expect: () => [const CartError('Minimum order amount not met')],
      );
    });

    group('RemoveCoupon', () {
      final Cart cartWithoutCoupon = MockData.testCart.copyWith(
        couponCode: null,
        discountAmount: 0.0,
      );

      blocTest<CartBloc, CartState>(
        'emits [CartLoaded] when coupon is removed successfully',
        build: () {
          when(
            mockCartService.removeCoupon(),
          ).thenAnswer((_) async => Result.success(cartWithoutCoupon));

          return cartBloc;
        },
        act: (bloc) => bloc.add(RemoveCoupon()),
        expect: () => [CartLoaded(cartWithoutCoupon)],
        verify: (_) {
          verify(mockCartService.removeCoupon()).called(1);
        },
      );

      blocTest<CartBloc, CartState>(
        'emits [CartError] when remove coupon fails',
        build: () {
          when(mockCartService.removeCoupon()).thenAnswer(
            (_) async => Result.failure(
              const NetworkException(message: 'Connection failed'),
            ),
          );

          return cartBloc;
        },
        act: (bloc) => bloc.add(RemoveCoupon()),
        expect: () => [const CartError('Connection failed')],
      );
    });

    group('MergeLocalCartAfterLogin', () {
      blocTest<CartBloc, CartState>(
        'loads server cart after login (backend is source of truth)',
        build: () {
          when(
            mockCartService.getCart(),
          ).thenAnswer((_) async => Result.success(MockData.testCart));

          return cartBloc;
        },
        act: (bloc) => bloc.add(MergeLocalCartAfterLogin()),
        expect: () => [CartLoaded(MockData.testCart)],
        verify: (_) {
          verify(mockCartService.getCart()).called(1);
        },
      );

      blocTest<CartBloc, CartState>(
        'emits [CartError] when merge fails',
        build: () {
          when(mockCartService.getCart()).thenAnswer(
            (_) async => Result.failure(
              const NetworkException(message: 'Failed to load cart'),
            ),
          );

          return cartBloc;
        },
        act: (bloc) => bloc.add(MergeLocalCartAfterLogin()),
        expect: () => [const CartError('Failed to load cart')],
      );
    });
  });
}
