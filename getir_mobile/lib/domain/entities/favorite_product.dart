import 'package:equatable/equatable.dart';

class FavoriteProduct extends Equatable {
  final String id;
  final String name;
  final String? description;
  final double price;
  final String? imageUrl;
  final String categoryName;
  final String merchantName;
  final DateTime addedAt;

  const FavoriteProduct({
    required this.id,
    required this.name,
    this.description,
    required this.price,
    this.imageUrl,
    required this.categoryName,
    required this.merchantName,
    required this.addedAt,
  });

  factory FavoriteProduct.fromJson(Map<String, dynamic> json) {
    return FavoriteProduct(
      id: json['id'] as String,
      name: json['name'] as String,
      description: json['description'] as String?,
      price: (json['price'] as num).toDouble(),
      imageUrl: json['imageUrl'] as String?,
      categoryName: json['categoryName'] as String,
      merchantName: json['merchantName'] as String,
      addedAt: DateTime.parse(json['addedAt'] as String),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'description': description,
      'price': price,
      'imageUrl': imageUrl,
      'categoryName': categoryName,
      'merchantName': merchantName,
      'addedAt': addedAt.toIso8601String(),
    };
  }

  @override
  List<Object?> get props => [
    id,
    name,
    description,
    price,
    imageUrl,
    categoryName,
    merchantName,
    addedAt,
  ];
}
