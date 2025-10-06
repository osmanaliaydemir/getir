import '../entities/cart.dart';

abstract class CartRepository {
  Future<Cart> getCart();
  Future<CartItem> addToCart({
    required String productId,
    required int quantity,
    String? variantId,
    List<String>? optionIds,
  });
  Future<CartItem> updateCartItem({
    required String itemId,
    required int quantity,
  });
  Future<void> removeFromCart(String itemId);
  Future<void> clearCart();
  Future<Cart> applyCoupon(String couponCode);
  Future<Cart> removeCoupon();
}
