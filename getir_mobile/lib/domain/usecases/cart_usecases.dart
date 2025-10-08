import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../entities/cart.dart';
import '../repositories/cart_repository.dart';

/// Get Cart Use Case
class GetCartUseCase {
  final CartRepository _repository;

  GetCartUseCase(this._repository);

  Future<Result<Cart>> call() async {
    return await _repository.getCart();
  }
}

/// Add To Cart Use Case
class AddToCartUseCase {
  final CartRepository _repository;

  AddToCartUseCase(this._repository);

  Future<Result<CartItem>> call({
    required String productId,
    required int quantity,
    String? variantId,
    List<String>? optionIds,
  }) async {
    // Validate quantity
    if (quantity <= 0) {
      return Result.failure(
        const ValidationException(
          message: 'Quantity must be greater than 0',
          code: 'INVALID_QUANTITY',
        ),
      );
    }

    return await _repository.addToCart(
      productId: productId,
      quantity: quantity,
      variantId: variantId,
      optionIds: optionIds,
    );
  }
}

/// Update Cart Item Use Case
class UpdateCartItemUseCase {
  final CartRepository _repository;

  UpdateCartItemUseCase(this._repository);

  Future<Result<CartItem>> call({
    required String itemId,
    required int quantity,
  }) async {
    // Validate quantity
    if (quantity <= 0) {
      return Result.failure(
        const ValidationException(
          message: 'Quantity must be greater than 0',
          code: 'INVALID_QUANTITY',
        ),
      );
    }

    return await _repository.updateCartItem(itemId: itemId, quantity: quantity);
  }
}

/// Remove From Cart Use Case
class RemoveFromCartUseCase {
  final CartRepository _repository;

  RemoveFromCartUseCase(this._repository);

  Future<Result<void>> call(String itemId) async {
    if (itemId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Item ID cannot be empty',
          code: 'EMPTY_ITEM_ID',
        ),
      );
    }

    return await _repository.removeFromCart(itemId);
  }
}

/// Clear Cart Use Case
class ClearCartUseCase {
  final CartRepository _repository;

  ClearCartUseCase(this._repository);

  Future<Result<void>> call() async {
    return await _repository.clearCart();
  }
}

/// Apply Coupon Use Case
class ApplyCouponUseCase {
  final CartRepository _repository;

  ApplyCouponUseCase(this._repository);

  Future<Result<Cart>> call(String couponCode) async {
    if (couponCode.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Coupon code cannot be empty',
          code: 'EMPTY_COUPON_CODE',
        ),
      );
    }

    return await _repository.applyCoupon(couponCode);
  }
}

/// Remove Coupon Use Case
class RemoveCouponUseCase {
  final CartRepository _repository;

  RemoveCouponUseCase(this._repository);

  Future<Result<Cart>> call() async {
    return await _repository.removeCoupon();
  }
}
