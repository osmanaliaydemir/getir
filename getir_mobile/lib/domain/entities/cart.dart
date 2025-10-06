import 'package:equatable/equatable.dart';

class Cart extends Equatable {
  final String id;
  final String userId;
  final List<CartItem> items;
  final double subtotal;
  final double deliveryFee;
  final double total;
  final String? couponCode;
  final double? discountAmount;
  final DateTime createdAt;
  final DateTime updatedAt;

  const Cart({
    required this.id,
    required this.userId,
    required this.items,
    required this.subtotal,
    required this.deliveryFee,
    required this.total,
    this.couponCode,
    this.discountAmount,
    required this.createdAt,
    required this.updatedAt,
  });

  @override
  List<Object?> get props => [
        id,
        userId,
        items,
        subtotal,
        deliveryFee,
        total,
        couponCode,
        discountAmount,
        createdAt,
        updatedAt,
      ];

  int get itemCount => items.fold(0, (sum, item) => sum + item.quantity);

  Cart copyWith({
    String? id,
    String? userId,
    List<CartItem>? items,
    double? subtotal,
    double? deliveryFee,
    double? total,
    String? couponCode,
    double? discountAmount,
    DateTime? createdAt,
    DateTime? updatedAt,
  }) {
    return Cart(
      id: id ?? this.id,
      userId: userId ?? this.userId,
      items: items ?? this.items,
      subtotal: subtotal ?? this.subtotal,
      deliveryFee: deliveryFee ?? this.deliveryFee,
      total: total ?? this.total,
      couponCode: couponCode ?? this.couponCode,
      discountAmount: discountAmount ?? this.discountAmount,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
    );
  }
}

class CartItem extends Equatable {
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

  const CartItem({
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

  @override
  List<Object?> get props => [
        id,
        productId,
        productName,
        productImageUrl,
        unitPrice,
        quantity,
        totalPrice,
        selectedVariantId,
        selectedVariantName,
        selectedOptionIds,
        selectedOptionNames,
        merchantId,
        merchantName,
        addedAt,
      ];

  CartItem copyWith({
    String? id,
    String? productId,
    String? productName,
    String? productImageUrl,
    double? unitPrice,
    int? quantity,
    double? totalPrice,
    String? selectedVariantId,
    String? selectedVariantName,
    List<String>? selectedOptionIds,
    List<String>? selectedOptionNames,
    String? merchantId,
    String? merchantName,
    DateTime? addedAt,
  }) {
    return CartItem(
      id: id ?? this.id,
      productId: productId ?? this.productId,
      productName: productName ?? this.productName,
      productImageUrl: productImageUrl ?? this.productImageUrl,
      unitPrice: unitPrice ?? this.unitPrice,
      quantity: quantity ?? this.quantity,
      totalPrice: totalPrice ?? this.totalPrice,
      selectedVariantId: selectedVariantId ?? this.selectedVariantId,
      selectedVariantName: selectedVariantName ?? this.selectedVariantName,
      selectedOptionIds: selectedOptionIds ?? this.selectedOptionIds,
      selectedOptionNames: selectedOptionNames ?? this.selectedOptionNames,
      merchantId: merchantId ?? this.merchantId,
      merchantName: merchantName ?? this.merchantName,
      addedAt: addedAt ?? this.addedAt,
    );
  }
}
