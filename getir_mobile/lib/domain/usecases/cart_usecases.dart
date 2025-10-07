import '../entities/cart.dart';
import '../repositories/cart_repository.dart';

/// Get Cart Use Case
///
/// Retrieves current user's shopping cart.
///
/// **Current Implementation:** Simple repository wrapper
/// **Future Enhancements:**
/// - Validate cart items (stock availability)
/// - Calculate dynamic pricing (time-based discounts)
/// - Apply automatic coupons
/// - Merge local/server cart intelligently
class GetCartUseCase {
  final CartRepository _repository;

  GetCartUseCase(this._repository);

  Future<Cart> call() async {
    return await _repository.getCart();
  }
}

/// Add To Cart Use Case
///
/// Adds a product to the shopping cart.
///
/// **Current Implementation:** Simple repository wrapper
/// **Future Enhancements:**
/// - Stock availability check before adding
/// - Maximum quantity limit validation
/// - Product price change detection
/// - Cross-sell/upsell recommendations
/// - Analytics tracking (add_to_cart event)
class AddToCartUseCase {
  final CartRepository _repository;

  AddToCartUseCase(this._repository);

  Future<CartItem> call({
    required String productId,
    required int quantity,
    String? variantId,
    List<String>? optionIds,
  }) async {
    // TODO: Add business logic
    // - Validate quantity > 0
    // - Check stock availability
    // - Validate product exists and is active

    return await _repository.addToCart(
      productId: productId,
      quantity: quantity,
      variantId: variantId,
      optionIds: optionIds,
    );
  }
}

class UpdateCartItemUseCase {
  final CartRepository _repository;

  UpdateCartItemUseCase(this._repository);

  Future<CartItem> call({required String itemId, required int quantity}) async {
    return await _repository.updateCartItem(itemId: itemId, quantity: quantity);
  }
}

class RemoveFromCartUseCase {
  final CartRepository _repository;

  RemoveFromCartUseCase(this._repository);

  Future<void> call(String itemId) async {
    return await _repository.removeFromCart(itemId);
  }
}

class ClearCartUseCase {
  final CartRepository _repository;

  ClearCartUseCase(this._repository);

  Future<void> call() async {
    return await _repository.clearCart();
  }
}

class ApplyCouponUseCase {
  final CartRepository _repository;

  ApplyCouponUseCase(this._repository);

  Future<Cart> call(String couponCode) async {
    return await _repository.applyCoupon(couponCode);
  }
}

class RemoveCouponUseCase {
  final CartRepository _repository;

  RemoveCouponUseCase(this._repository);

  Future<Cart> call() async {
    return await _repository.removeCoupon();
  }
}
