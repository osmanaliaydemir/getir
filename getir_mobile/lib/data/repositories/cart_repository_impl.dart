import '../../domain/entities/cart.dart';
import '../../domain/repositories/cart_repository.dart';
import '../datasources/cart_datasource.dart';

class CartRepositoryImpl implements CartRepository {
  final CartDataSource _dataSource;

  CartRepositoryImpl(this._dataSource);

  @override
  Future<Cart> getCart() async {
    return await _dataSource.getCart();
  }

  @override
  Future<CartItem> addToCart({
    required String productId,
    required int quantity,
    String? variantId,
    List<String>? optionIds,
  }) async {
    return await _dataSource.addToCart(
      productId: productId,
      quantity: quantity,
      variantId: variantId,
      optionIds: optionIds,
    );
  }

  @override
  Future<CartItem> updateCartItem({
    required String itemId,
    required int quantity,
  }) async {
    return await _dataSource.updateCartItem(
      itemId: itemId,
      quantity: quantity,
    );
  }

  @override
  Future<void> removeFromCart(String itemId) async {
    return await _dataSource.removeFromCart(itemId);
  }

  @override
  Future<void> clearCart() async {
    return await _dataSource.clearCart();
  }

  @override
  Future<Cart> applyCoupon(String couponCode) async {
    return await _dataSource.applyCoupon(couponCode);
  }

  @override
  Future<Cart> removeCoupon() async {
    return await _dataSource.removeCoupon();
  }
}
