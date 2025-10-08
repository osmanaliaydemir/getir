import 'package:dio/dio.dart';
import '../../domain/entities/cart.dart';

abstract class CartDataSource {
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

class CartDataSourceImpl implements CartDataSource {
  final Dio _dio;

  CartDataSourceImpl(this._dio);

  @override
  Future<Cart> getCart() async {
    try {
      final response = await _dio.get('/api/v1/Cart');
      return _cartFromJson(response.data);
    } catch (e) {
      throw Exception('Failed to fetch cart: $e');
    }
  }

  @override
  Future<CartItem> addToCart({
    required String productId,
    required int quantity,
    String? variantId,
    List<String>? optionIds,
  }) async {
    try {
      final response = await _dio.post(
        '/api/v1/Cart/items',
        data: {
          'productId': productId,
          'quantity': quantity,
          if (variantId != null) 'variantId': variantId,
          if (optionIds != null) 'optionIds': optionIds,
        },
      );
      return _cartItemFromJson(response.data);
    } catch (e) {
      throw Exception('Failed to add to cart: $e');
    }
  }

  @override
  Future<CartItem> updateCartItem({
    required String itemId,
    required int quantity,
  }) async {
    try {
      final response = await _dio.put(
        '/api/v1/Cart/items/$itemId',
        data: {'quantity': quantity},
      );
      return _cartItemFromJson(response.data);
    } catch (e) {
      throw Exception('Failed to update cart item: $e');
    }
  }

  @override
  Future<void> removeFromCart(String itemId) async {
    try {
      await _dio.delete('/api/v1/Cart/items/$itemId');
    } catch (e) {
      throw Exception('Failed to remove from cart: $e');
    }
  }

  @override
  Future<void> clearCart() async {
    try {
      await _dio.delete('/api/v1/Cart/clear');
    } catch (e) {
      throw Exception('Failed to clear cart: $e');
    }
  }

  @override
  Future<Cart> applyCoupon(String couponCode) async {
    try {
      final response = await _dio.post(
        '/api/v1/Coupon/apply',
        data: {'couponCode': couponCode},
      );
      return _cartFromJson(response.data);
    } catch (e) {
      throw Exception('Failed to apply coupon: $e');
    }
  }

  @override
  Future<Cart> removeCoupon() async {
    try {
      final response = await _dio.delete('/api/v1/Coupon/remove');
      return _cartFromJson(response.data);
    } catch (e) {
      throw Exception('Failed to remove coupon: $e');
    }
  }

  Cart _cartFromJson(Map<String, dynamic> json) {
    return Cart(
      id: json['id']?.toString() ?? '',
      userId: json['userId']?.toString() ?? '',
      items:
          (json['items'] as List<dynamic>?)
              ?.map((item) => _cartItemFromJson(item))
              .toList() ??
          [],
      subtotal: (json['subtotal'] ?? 0.0).toDouble(),
      deliveryFee: (json['deliveryFee'] ?? 0.0).toDouble(),
      total: (json['total'] ?? 0.0).toDouble(),
      couponCode: json['couponCode'],
      discountAmount: json['discountAmount']?.toDouble(),
      createdAt: DateTime.parse(
        json['createdAt'] ?? DateTime.now().toIso8601String(),
      ),
      updatedAt: DateTime.parse(
        json['updatedAt'] ?? DateTime.now().toIso8601String(),
      ),
    );
  }

  CartItem _cartItemFromJson(Map<String, dynamic> json) {
    return CartItem(
      id: json['id']?.toString() ?? '',
      productId: json['productId']?.toString() ?? '',
      productName: json['productName'] ?? '',
      productImageUrl: json['productImageUrl'] ?? '',
      unitPrice: (json['unitPrice'] ?? 0.0).toDouble(),
      quantity: json['quantity'] ?? 0,
      totalPrice: (json['totalPrice'] ?? 0.0).toDouble(),
      selectedVariantId: json['selectedVariantId']?.toString(),
      selectedVariantName: json['selectedVariantName'],
      selectedOptionIds: List<String>.from(json['selectedOptionIds'] ?? []),
      selectedOptionNames: List<String>.from(json['selectedOptionNames'] ?? []),
      merchantId: json['merchantId']?.toString() ?? '',
      merchantName: json['merchantName'] ?? '',
      addedAt: DateTime.parse(
        json['addedAt'] ?? DateTime.now().toIso8601String(),
      ),
    );
  }
}
