import '../../../domain/entities/cart.dart';

class CartDto {
  final String id;
  final String userId;
  final String merchantId;
  final String merchantName;
  final List<CartItemDto> items;
  final double subtotal;
  final double deliveryFee;
  final double total;
  final String? couponCode;
  final double? discountAmount;
  final DateTime createdAt;
  final DateTime updatedAt;

  const CartDto({
    required this.id,
    required this.userId,
    required this.merchantId,
    required this.merchantName,
    required this.items,
    required this.subtotal,
    required this.deliveryFee,
    required this.total,
    this.couponCode,
    this.discountAmount,
    required this.createdAt,
    required this.updatedAt,
  });

  factory CartDto.fromJson(Map<String, dynamic> json) {
    return CartDto(
      id: (json['id'] ?? '').toString(),
      userId: (json['userId'] ?? '').toString(),
      merchantId: (json['merchantId'] ?? '').toString(),
      merchantName: (json['merchantName'] ?? '').toString(),
      items:
          (json['items'] as List<dynamic>?)
              ?.map((e) => CartItemDto.fromJson(e as Map<String, dynamic>))
              .toList() ??
          const <CartItemDto>[],
      subtotal: json['subtotal'] is num
          ? (json['subtotal'] as num).toDouble()
          : 0.0,
      deliveryFee: json['deliveryFee'] is num
          ? (json['deliveryFee'] as num).toDouble()
          : 0.0,
      total: json['total'] is num ? (json['total'] as num).toDouble() : 0.0,
      couponCode: json['couponCode'] as String?,
      discountAmount: json['discountAmount'] is num
          ? (json['discountAmount'] as num).toDouble()
          : null,
      createdAt: DateTime.parse(
        (json['createdAt'] ?? DateTime.now().toIso8601String()).toString(),
      ),
      updatedAt: DateTime.parse(
        (json['updatedAt'] ?? DateTime.now().toIso8601String()).toString(),
      ),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'userId': userId,
      'items': items.map((e) => e.toJson()).toList(),
      'subtotal': subtotal,
      'deliveryFee': deliveryFee,
      'total': total,
      'couponCode': couponCode,
      'discountAmount': discountAmount,
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt.toIso8601String(),
    };
  }

  Cart toDomain() {
    return Cart(
      id: id,
      userId: userId,
      merchantId: merchantId,
      merchantName: merchantName,
      items: items.map((e) => e.toDomain()).toList(),
      subtotal: subtotal,
      deliveryFee: deliveryFee,
      total: total,
      couponCode: couponCode,
      discountAmount: discountAmount,
      createdAt: createdAt,
      updatedAt: updatedAt,
    );
  }
}

class CartItemDto {
  final String id;
  final String productId;
  final String productName;
  final String productImageUrl;
  final double unitPrice;
  final int quantity;
  final double totalPrice;
  final String? selectedVariantId;
  final String? selectedVariantName;
  final List<String> selectedOptionIds;
  final List<String> selectedOptionNames;
  final String merchantId;
  final String merchantName;
  final DateTime addedAt;

  const CartItemDto({
    required this.id,
    required this.productId,
    required this.productName,
    required this.productImageUrl,
    required this.unitPrice,
    required this.quantity,
    required this.totalPrice,
    this.selectedVariantId,
    this.selectedVariantName,
    required this.selectedOptionIds,
    required this.selectedOptionNames,
    required this.merchantId,
    required this.merchantName,
    required this.addedAt,
  });

  factory CartItemDto.fromJson(Map<String, dynamic> json) {
    return CartItemDto(
      id: (json['id'] ?? '').toString(),
      productId: (json['productId'] ?? '').toString(),
      productName: (json['productName'] ?? '').toString(),
      productImageUrl: (json['productImageUrl'] ?? '').toString(),
      unitPrice: json['unitPrice'] is num
          ? (json['unitPrice'] as num).toDouble()
          : 0.0,
      quantity: json['quantity'] is int ? json['quantity'] as int : 0,
      totalPrice: json['totalPrice'] is num
          ? (json['totalPrice'] as num).toDouble()
          : 0.0,
      selectedVariantId: (json['selectedVariantId'] as String?),
      selectedVariantName: (json['selectedVariantName'] as String?),
      selectedOptionIds:
          (json['selectedOptionIds'] as List<dynamic>?)
              ?.map((e) => e.toString())
              .toList() ??
          const <String>[],
      selectedOptionNames:
          (json['selectedOptionNames'] as List<dynamic>?)
              ?.map((e) => e.toString())
              .toList() ??
          const <String>[],
      merchantId: (json['merchantId'] ?? '').toString(),
      merchantName: (json['merchantName'] ?? '').toString(),
      addedAt: DateTime.parse(
        (json['addedAt'] ?? DateTime.now().toIso8601String()).toString(),
      ),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'productId': productId,
      'productName': productName,
      'productImageUrl': productImageUrl,
      'unitPrice': unitPrice,
      'quantity': quantity,
      'totalPrice': totalPrice,
      'selectedVariantId': selectedVariantId,
      'selectedVariantName': selectedVariantName,
      'selectedOptionIds': selectedOptionIds,
      'selectedOptionNames': selectedOptionNames,
      'merchantId': merchantId,
      'merchantName': merchantName,
      'addedAt': addedAt.toIso8601String(),
    };
  }

  CartItem toDomain() {
    return CartItem(
      id: id,
      productId: productId,
      productName: productName,
      productImageUrl: productImageUrl,
      unitPrice: unitPrice,
      quantity: quantity,
      totalPrice: totalPrice,
      selectedVariantId: selectedVariantId,
      selectedVariantName: selectedVariantName,
      selectedOptionIds: selectedOptionIds,
      selectedOptionNames: selectedOptionNames,
      merchantId: merchantId,
      merchantName: merchantName,
      addedAt: addedAt,
    );
  }
}
