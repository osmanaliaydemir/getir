import '../entities/cart.dart';
import '../repositories/cart_repository.dart';

class GetCartUseCase {
  final CartRepository _repository;

  GetCartUseCase(this._repository);

  Future<Cart> call() async {
    return await _repository.getCart();
  }
}

class AddToCartUseCase {
  final CartRepository _repository;

  AddToCartUseCase(this._repository);

  Future<CartItem> call({
    required String productId,
    required int quantity,
    String? variantId,
    List<String>? optionIds,
  }) async {
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

  Future<CartItem> call({
    required String itemId,
    required int quantity,
  }) async {
    return await _repository.updateCartItem(
      itemId: itemId,
      quantity: quantity,
    );
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
