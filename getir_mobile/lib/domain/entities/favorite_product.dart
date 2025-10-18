import 'package:equatable/equatable.dart';

class FavoriteProduct extends Equatable {
  final String id;
  final String productId;
  final String productName;
  final String? productDescription;
  final double price;
  final String? imageUrl;
  final String merchantId;
  final String merchantName;
  final bool isAvailable;
  final DateTime addedAt;

  const FavoriteProduct({
    required this.id,
    required this.productId,
    required this.productName,
    this.productDescription,
    required this.price,
    this.imageUrl,
    required this.merchantId,
    required this.merchantName,
    required this.isAvailable,
    required this.addedAt,
  });

  factory FavoriteProduct.fromJson(Map<String, dynamic> json) {
    return FavoriteProduct(
      id: json['id'] as String,
      productId: json['productId'] as String,
      productName: json['productName'] as String,
      productDescription: json['productDescription'] as String?,
      price: (json['price'] as num).toDouble(),
      imageUrl: json['imageUrl'] as String?,
      merchantId: json['merchantId'] as String,
      merchantName: json['merchantName'] as String,
      isAvailable: json['isAvailable'] as bool? ?? true,
      addedAt: DateTime.parse(json['addedAt'] as String),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'productId': productId,
      'productName': productName,
      'productDescription': productDescription,
      'price': price,
      'imageUrl': imageUrl,
      'merchantId': merchantId,
      'merchantName': merchantName,
      'isAvailable': isAvailable,
      'addedAt': addedAt.toIso8601String(),
    };
  }

  @override
  List<Object?> get props => [
    id,
    productId,
    productName,
    productDescription,
    price,
    imageUrl,
    merchantId,
    merchantName,
    isAvailable,
    addedAt,
  ];
}
