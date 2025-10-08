import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../entities/cart.dart';
import '../repositories/cart_repository.dart';

/// Shopping Cart Service
///
/// Centralized service for all cart-related operations.
/// Replaces 7 separate UseCase classes with a single, cohesive service.
///
/// **Methods:**
/// - getCart() - Retrieve current cart
/// - addToCart() - Add item to cart
/// - updateCartItem() - Update item quantity
/// - removeFromCart() - Remove item from cart
/// - clearCart() - Clear all items
/// - applyCoupon() - Apply discount coupon
/// - removeCoupon() - Remove discount coupon
class CartService {
  final CartRepository _repository;

  const CartService(this._repository);

  /// Retrieves the current shopping cart.
  Future<Result<Cart>> getCart() async {
    return await _repository.getCart();
  }

  /// Adds a product to the cart with specified quantity.
  ///
  /// **Validation:**
  /// - Quantity must be greater than 0
  ///
  /// **Returns:**
  /// - Success: [CartItem] added to cart
  /// - Failure: [ValidationException] on invalid quantity
  Future<Result<CartItem>> addToCart({
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

  /// Updates the quantity of an existing cart item.
  ///
  /// **Validation:**
  /// - Quantity must be greater than 0
  ///
  /// **Returns:**
  /// - Success: Updated [CartItem]
  /// - Failure: [ValidationException] on invalid quantity
  /// - Failure: [NotFoundException] if item not in cart
  Future<Result<CartItem>> updateCartItem({
    required String cartItemId,
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

    return await _repository.updateCartItem(
      itemId: cartItemId,
      quantity: quantity,
    );
  }

  /// Removes an item from the cart.
  Future<Result<void>> removeFromCart(String cartItemId) async {
    return await _repository.removeFromCart(cartItemId);
  }

  /// Clears all items from the cart.
  Future<Result<void>> clearCart() async {
    return await _repository.clearCart();
  }

  /// Applies a discount coupon to the cart.
  ///
  /// **Validation:**
  /// - Coupon code must not be empty
  ///
  /// **Returns:**
  /// - Success: Updated [Cart] with discount applied
  /// - Failure: [ValidationException] on empty code
  /// - Failure: [NotFoundException] if coupon invalid/expired
  Future<Result<Cart>> applyCoupon(String couponCode) async {
    // Validate coupon code
    if (couponCode.trim().isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Coupon code cannot be empty',
          code: 'INVALID_COUPON',
        ),
      );
    }

    return await _repository.applyCoupon(couponCode.trim());
  }

  /// Removes the currently applied coupon from the cart.
  Future<Result<Cart>> removeCoupon() async {
    return await _repository.removeCoupon();
  }
}
