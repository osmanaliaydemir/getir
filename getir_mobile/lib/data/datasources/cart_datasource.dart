import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import '../../domain/entities/cart.dart';

abstract class CartDataSource {
  Future<Cart> getCart();
  Future<CartItem> addToCart({
    required String merchantId,
    required String productId,
    required int quantity,
    String? notes,
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
      final response = await _dio.get('/api/v1/cart');
      debugPrint(
        '🛒 [CartDataSource] getCart response type: ${response.data.runtimeType}',
      );

      // Response unwrap edilmiş olmalı
      final dynamic data = response.data;
      return _cartFromJson(data is Map<String, dynamic> ? data : {});
    } catch (e) {
      debugPrint('❌ [CartDataSource] getCart error: $e');
      throw Exception('Failed to fetch cart: $e');
    }
  }

  @override
  Future<CartItem> addToCart({
    required String merchantId,
    required String productId,
    required int quantity,
    String? notes,
  }) async {
    try {
      debugPrint(
        '🛒 [CartDataSource] addToCart: merchantId=$merchantId, productId=$productId, quantity=$quantity',
      );

      final response = await _dio.post(
        '/api/v1/cart/items',
        data: {
          'MerchantId': merchantId, // Backend PascalCase bekliyor
          'ProductId': productId,
          'Quantity': quantity,
          'Notes': notes,
        },
      );

      debugPrint('✅ [CartDataSource] addToCart success');
      return _cartItemFromJson(response.data);
    } catch (e) {
      debugPrint('❌ [CartDataSource] addToCart error: $e');
      throw Exception('Failed to add to cart: $e');
    }
  }

  @override
  Future<CartItem> updateCartItem({
    required String itemId,
    required int quantity,
  }) async {
    try {
      debugPrint(
        '🛒 [CartDataSource] updateCartItem: itemId=$itemId, quantity=$quantity',
      );

      final response = await _dio.put(
        '/api/v1/cart/items/$itemId',
        data: {
          'Quantity': quantity, // Backend PascalCase bekliyor
          'Notes': null, // Backend notes alanı bekliyor
        },
      );

      debugPrint('✅ [CartDataSource] updateCartItem success');
      return _cartItemFromJson(response.data);
    } catch (e) {
      debugPrint('❌ [CartDataSource] updateCartItem error: $e');
      throw Exception('Failed to update cart item: $e');
    }
  }

  @override
  Future<void> removeFromCart(String itemId) async {
    try {
      debugPrint('🛒 [CartDataSource] removeFromCart: itemId=$itemId');
      await _dio.delete('/api/v1/cart/items/$itemId');
      debugPrint('✅ [CartDataSource] removeFromCart success');
    } catch (e) {
      debugPrint('❌ [CartDataSource] removeFromCart error: $e');
      throw Exception('Failed to remove from cart: $e');
    }
  }

  @override
  Future<void> clearCart() async {
    try {
      await _dio.delete('/api/v1/cart/clear');
    } catch (e) {
      throw Exception('Failed to clear cart: $e');
    }
  }

  @override
  Future<Cart> applyCoupon(String couponCode) async {
    try {
      final response = await _dio.post(
        '/api/v1/coupon/apply',
        data: {'CouponCode': couponCode}, // Backend PascalCase bekliyor
      );
      return _cartFromJson(response.data);
    } catch (e) {
      throw Exception('Failed to apply coupon: $e');
    }
  }

  @override
  Future<Cart> removeCoupon() async {
    try {
      final response = await _dio.delete('/api/v1/coupon/remove');
      return _cartFromJson(response.data);
    } catch (e) {
      throw Exception('Failed to remove coupon: $e');
    }
  }

  Cart _cartFromJson(Map<String, dynamic> json) {
    return Cart(
      id: json['id']?.toString() ?? '',
      userId: json['userId']?.toString() ?? '',
      merchantId: json['merchantId']?.toString() ?? '',
      merchantName: json['merchantName'] ?? '',
      items:
          (json['items'] as List<dynamic>?)
              ?.map((item) => _cartItemFromJson(item))
              .toList() ??
          [],
      subtotal: (json['subTotal'] ?? json['subtotal'] ?? 0.0)
          .toDouble(), // Backend'den subTotal geliyor (PascalCase)
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
      productImageUrl:
          json['productImage'] ??
          json['productImageUrl'] ??
          '', // Backend'den productImage geliyor
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
