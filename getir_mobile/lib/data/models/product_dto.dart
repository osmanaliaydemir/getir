import '../../../domain/entities/product.dart';

class ProductDto {
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
  final List<ProductVariantDto> variants;
  final List<ProductOptionDto> options;
  final double rating;
  final int reviewCount;
  final String unit;
  final double? discountPrice;
  final double? discountPercentage;
  final List<String> tags;
  final Map<String, dynamic> nutritionalInfo;
  final String brand;
  final String barcode;

  const ProductDto({
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

  factory ProductDto.fromJson(Map<String, dynamic> json) {
    return ProductDto(
      id: (json['id'] ?? '').toString(),
      name: (json['name'] ?? '').toString(),
      description: (json['description'] ?? '').toString(),
      price: (json['price'] is num ? (json['price'] as num).toDouble() : 0.0),
      imageUrl: (json['imageUrl'] ?? '').toString(),
      category: (json['category'] ?? '').toString(),
      merchantId: (json['merchantId'] ?? '').toString(),
      merchantName: (json['merchantName'] ?? '').toString(),
      isAvailable: json['isAvailable'] == true,
      stockQuantity: json['stockQuantity'] is int
          ? json['stockQuantity'] as int
          : 0,
      variants:
          (json['variants'] as List<dynamic>?)
              ?.map(
                (e) => ProductVariantDto.fromJson(e as Map<String, dynamic>),
              )
              .toList() ??
          const <ProductVariantDto>[],
      options:
          (json['options'] as List<dynamic>?)
              ?.map((e) => ProductOptionDto.fromJson(e as Map<String, dynamic>))
              .toList() ??
          const <ProductOptionDto>[],
      rating: json['rating'] is num ? (json['rating'] as num).toDouble() : 0.0,
      reviewCount: json['reviewCount'] is int ? json['reviewCount'] as int : 0,
      unit: (json['unit'] ?? 'adet').toString(),
      discountPrice: json['discountPrice'] is num
          ? (json['discountPrice'] as num).toDouble()
          : null,
      discountPercentage: json['discountPercentage'] is num
          ? (json['discountPercentage'] as num).toDouble()
          : null,
      tags:
          (json['tags'] as List<dynamic>?)?.map((e) => e.toString()).toList() ??
          const <String>[],
      nutritionalInfo: json['nutritionalInfo'] is Map<String, dynamic>
          ? json['nutritionalInfo'] as Map<String, dynamic>
          : <String, dynamic>{},
      brand: (json['brand'] ?? '').toString(),
      barcode: (json['barcode'] ?? '').toString(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'description': description,
      'price': price,
      'imageUrl': imageUrl,
      'category': category,
      'merchantId': merchantId,
      'merchantName': merchantName,
      'isAvailable': isAvailable,
      'stockQuantity': stockQuantity,
      'variants': variants.map((e) => e.toJson()).toList(),
      'options': options.map((e) => e.toJson()).toList(),
      'rating': rating,
      'reviewCount': reviewCount,
      'unit': unit,
      'discountPrice': discountPrice,
      'discountPercentage': discountPercentage,
      'tags': tags,
      'nutritionalInfo': nutritionalInfo,
      'brand': brand,
      'barcode': barcode,
    };
  }

  Product toDomain() {
    return Product(
      id: id,
      name: name,
      description: description,
      price: price,
      imageUrl: imageUrl,
      category: category,
      merchantId: merchantId,
      merchantName: merchantName,
      isAvailable: isAvailable,
      stockQuantity: stockQuantity,
      variants: variants.map((v) => v.toDomain()).toList(),
      options: options.map((o) => o.toDomain()).toList(),
      rating: rating,
      reviewCount: reviewCount,
      unit: unit,
      discountPrice: discountPrice,
      discountPercentage: discountPercentage,
      tags: tags,
      nutritionalInfo: nutritionalInfo,
      brand: brand,
      barcode: barcode,
    );
  }
}

class ProductVariantDto {
  final String id;
  final String name;
  final double price;
  final bool isAvailable;
  final int stockQuantity;

  const ProductVariantDto({
    required this.id,
    required this.name,
    required this.price,
    required this.isAvailable,
    required this.stockQuantity,
  });

  factory ProductVariantDto.fromJson(Map<String, dynamic> json) {
    return ProductVariantDto(
      id: (json['id'] ?? '').toString(),
      name: (json['name'] ?? '').toString(),
      price: json['price'] is num ? (json['price'] as num).toDouble() : 0.0,
      isAvailable: json['isAvailable'] == true,
      stockQuantity: json['stockQuantity'] is int
          ? json['stockQuantity'] as int
          : 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'price': price,
      'isAvailable': isAvailable,
      'stockQuantity': stockQuantity,
    };
  }

  ProductVariant toDomain() {
    return ProductVariant(
      id: id,
      name: name,
      price: price,
      isAvailable: isAvailable,
      stockQuantity: stockQuantity,
    );
  }
}

class ProductOptionDto {
  final String id;
  final String name;
  final String type;
  final bool isRequired;
  final List<ProductOptionValueDto> values;

  const ProductOptionDto({
    required this.id,
    required this.name,
    required this.type,
    required this.isRequired,
    required this.values,
  });

  factory ProductOptionDto.fromJson(Map<String, dynamic> json) {
    return ProductOptionDto(
      id: (json['id'] ?? '').toString(),
      name: (json['name'] ?? '').toString(),
      type: (json['type'] ?? 'single').toString(),
      isRequired: json['isRequired'] == true,
      values:
          (json['values'] as List<dynamic>?)
              ?.map(
                (v) =>
                    ProductOptionValueDto.fromJson(v as Map<String, dynamic>),
              )
              .toList() ??
          const <ProductOptionValueDto>[],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'type': type,
      'isRequired': isRequired,
      'values': values.map((e) => e.toJson()).toList(),
    };
  }

  ProductOption toDomain() {
    return ProductOption(
      id: id,
      name: name,
      type: type,
      isRequired: isRequired,
      values: values.map((v) => v.toDomain()).toList(),
    );
  }
}

class ProductOptionValueDto {
  final String id;
  final String name;
  final double price;
  final bool isAvailable;

  const ProductOptionValueDto({
    required this.id,
    required this.name,
    required this.price,
    required this.isAvailable,
  });

  factory ProductOptionValueDto.fromJson(Map<String, dynamic> json) {
    return ProductOptionValueDto(
      id: (json['id'] ?? '').toString(),
      name: (json['name'] ?? '').toString(),
      price: json['price'] is num ? (json['price'] as num).toDouble() : 0.0,
      isAvailable: json['isAvailable'] == true,
    );
  }

  Map<String, dynamic> toJson() {
    return {'id': id, 'name': name, 'price': price, 'isAvailable': isAvailable};
  }

  ProductOptionValue toDomain() {
    return ProductOptionValue(
      id: id,
      name: name,
      price: price,
      isAvailable: isAvailable,
    );
  }
}
