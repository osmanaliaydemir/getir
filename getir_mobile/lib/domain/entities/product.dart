import 'package:equatable/equatable.dart';

class Product extends Equatable {
  final String id;
  final String name;
  final String description;
  final double price;
  final String imageUrl;
  final String category;
  final String merchantId;
  final String merchantName;
  final bool isAvailable;
  final int stockQuantity;
  final List<ProductVariant> variants;
  final List<ProductOption> options;
  final double? rating; // Nullable - ürünün henüz review'ı olmayabilir
  final int? reviewCount;
  final String unit; // kg, adet, litre, etc.
  final double? discountPrice;
  final double? discountPercentage;
  final List<String> tags;
  final Map<String, dynamic> nutritionalInfo;
  final String brand;
  final String barcode;

  const Product({
    required this.id,
    required this.name,
    required this.description,
    required this.price,
    required this.imageUrl,
    required this.category,
    required this.merchantId,
    required this.merchantName,
    required this.isAvailable,
    required this.stockQuantity,
    required this.variants,
    required this.options,
    required this.rating,
    required this.reviewCount,
    required this.unit,
    this.discountPrice,
    this.discountPercentage,
    required this.tags,
    required this.nutritionalInfo,
    required this.brand,
    required this.barcode,
  });

  @override
  List<Object?> get props => [
    id,
    name,
    description,
    price,
    imageUrl,
    category,
    merchantId,
    merchantName,
    isAvailable,
    stockQuantity,
    variants,
    options,
    rating,
    reviewCount,
    unit,
    discountPrice,
    discountPercentage,
    tags,
    nutritionalInfo,
    brand,
    barcode,
  ];

  double get finalPrice => discountPrice ?? price;

  bool get hasDiscount => discountPrice != null && discountPrice! < price;

  Product copyWith({
    String? id,
    String? name,
    String? description,
    double? price,
    String? imageUrl,
    String? category,
    String? merchantId,
    String? merchantName,
    bool? isAvailable,
    int? stockQuantity,
    List<ProductVariant>? variants,
    List<ProductOption>? options,
    double? rating,
    int? reviewCount,
    String? unit,
    double? discountPrice,
    double? discountPercentage,
    List<String>? tags,
    Map<String, dynamic>? nutritionalInfo,
    String? brand,
    String? barcode,
  }) {
    return Product(
      id: id ?? this.id,
      name: name ?? this.name,
      description: description ?? this.description,
      price: price ?? this.price,
      imageUrl: imageUrl ?? this.imageUrl,
      category: category ?? this.category,
      merchantId: merchantId ?? this.merchantId,
      merchantName: merchantName ?? this.merchantName,
      isAvailable: isAvailable ?? this.isAvailable,
      stockQuantity: stockQuantity ?? this.stockQuantity,
      variants: variants ?? this.variants,
      options: options ?? this.options,
      rating: rating ?? this.rating,
      reviewCount: reviewCount ?? this.reviewCount,
      unit: unit ?? this.unit,
      discountPrice: discountPrice ?? this.discountPrice,
      discountPercentage: discountPercentage ?? this.discountPercentage,
      tags: tags ?? this.tags,
      nutritionalInfo: nutritionalInfo ?? this.nutritionalInfo,
      brand: brand ?? this.brand,
      barcode: barcode ?? this.barcode,
    );
  }
}

class ProductVariant extends Equatable {
  final String id;
  final String name;
  final double price;
  final bool isAvailable;
  final int stockQuantity;

  const ProductVariant({
    required this.id,
    required this.name,
    required this.price,
    required this.isAvailable,
    required this.stockQuantity,
  });

  @override
  List<Object?> get props => [id, name, price, isAvailable, stockQuantity];
}

class ProductOption extends Equatable {
  final String id;
  final String name;
  final String type; // single, multiple
  final bool isRequired;
  final List<ProductOptionValue> values;

  const ProductOption({
    required this.id,
    required this.name,
    required this.type,
    required this.isRequired,
    required this.values,
  });

  @override
  List<Object?> get props => [id, name, type, isRequired, values];
}

class ProductOptionValue extends Equatable {
  final String id;
  final String name;
  final double price;
  final bool isAvailable;

  const ProductOptionValue({
    required this.id,
    required this.name,
    required this.price,
    required this.isAvailable,
  });

  @override
  List<Object?> get props => [id, name, price, isAvailable];
}
