import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../domain/entities/cart.dart';
import '../../domain/repositories/cart_repository.dart';
import '../datasources/cart_datasource.dart';

class CartRepositoryImpl implements ICartRepository {
  final CartDataSource _dataSource;

  CartRepositoryImpl(this._dataSource);

  @override
  Future<Result<Cart>> getCart() async {
    try {
      final cart = await _dataSource.getCart();
      return Result.success(cart);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get cart: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<CartItem>> addToCart({
    required String productId,
    required int quantity,
    String? variantId,
    List<String>? optionIds,
  }) async {
    try {
      final cartItem = await _dataSource.addToCart(
        productId: productId,
        quantity: quantity,
        variantId: variantId,
        optionIds: optionIds,
      );
      return Result.success(cartItem);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to add to cart: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<CartItem>> updateCartItem({
    required String itemId,
    required int quantity,
  }) async {
    try {
      final cartItem = await _dataSource.updateCartItem(
        itemId: itemId,
        quantity: quantity,
      );
      return Result.success(cartItem);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to update cart item: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<void>> removeFromCart(String itemId) async {
    try {
      await _dataSource.removeFromCart(itemId);
      return Result.success(null);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to remove from cart: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<void>> clearCart() async {
    try {
      await _dataSource.clearCart();
      return Result.success(null);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to clear cart: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<Cart>> applyCoupon(String couponCode) async {
    try {
      final cart = await _dataSource.applyCoupon(couponCode);
      return Result.success(cart);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to apply coupon: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<Cart>> removeCoupon() async {
    try {
      final cart = await _dataSource.removeCoupon();
      return Result.success(cart);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to remove coupon: ${e.toString()}'),
      );
    }
  }
}
