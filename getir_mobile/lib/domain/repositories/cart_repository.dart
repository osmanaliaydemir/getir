import '../../core/errors/result.dart';
import '../entities/cart.dart';

abstract class ICartRepository {
  Future<Result<Cart>> getCart();
  Future<Result<CartItem>> addToCart({
    required String merchantId,
    required String productId,
    required int quantity,
  });
  Future<Result<CartItem>> updateCartItem({
    required String itemId,
    required int quantity,
  });
  Future<Result<void>> removeFromCart(String itemId);
  Future<Result<void>> clearCart();
  Future<Result<Cart>> applyCoupon(String couponCode);
  Future<Result<Cart>> removeCoupon();
}
