import '../../../domain/entities/merchant.dart';

class MerchantDto {
  final String id;
  final String name;
  final String description;
  final String logoUrl;
  final String coverImageUrl;
  final double rating;
  final int reviewCount;
  final double deliveryFee;
  final int estimatedDeliveryTime;
  final double distance;
  final bool isOpen;
  final String address;
  final String phoneNumber;
  final List<String> categories;
  final Map<String, String> workingHours;
  final double minimumOrderAmount;
  final bool isDeliveryAvailable;
  final bool isPickupAvailable;

  const MerchantDto({
    required this.id,
    required this.name,
    required this.description,
    required this.logoUrl,
    required this.coverImageUrl,
    required this.rating,
    required this.reviewCount,
    required this.deliveryFee,
    required this.estimatedDeliveryTime,
    required this.distance,
    required this.isOpen,
    required this.address,
    required this.phoneNumber,
    required this.categories,
    required this.workingHours,
    required this.minimumOrderAmount,
    required this.isDeliveryAvailable,
    required this.isPickupAvailable,
  });

  factory MerchantDto.fromJson(Map<String, dynamic> json) {
    return MerchantDto(
      id: (json['id'] ?? '').toString(),
      name: (json['name'] ?? '').toString(),
      description: (json['description'] ?? '').toString(),
      logoUrl: (json['logoUrl'] ?? '').toString(),
      coverImageUrl: (json['coverImageUrl'] ?? '').toString(),
      rating: json['rating'] is num ? (json['rating'] as num).toDouble() : 0.0,
      reviewCount: json['reviewCount'] is int ? json['reviewCount'] as int : 0,
      deliveryFee: json['deliveryFee'] is num
          ? (json['deliveryFee'] as num).toDouble()
          : 0.0,
      estimatedDeliveryTime: json['estimatedDeliveryTime'] is int
          ? json['estimatedDeliveryTime'] as int
          : 30,
      distance: json['distance'] is num
          ? (json['distance'] as num).toDouble()
          : 0.0,
      isOpen: json['isOpen'] == true,
      address: (json['address'] ?? '').toString(),
      phoneNumber: (json['phoneNumber'] ?? '').toString(),
      categories:
          (json['categories'] as List<dynamic>?)
              ?.map((e) => e.toString())
              .toList() ??
          const <String>[],
      workingHours: (json['workingHours'] is Map<String, dynamic>)
          ? (json['workingHours'] as Map<String, dynamic>).map(
              (k, v) => MapEntry(k.toString(), v.toString()),
            )
          : <String, String>{},
      minimumOrderAmount: json['minimumOrderAmount'] is num
          ? (json['minimumOrderAmount'] as num).toDouble()
          : 0.0,
      isDeliveryAvailable: json['isDeliveryAvailable'] != false,
      isPickupAvailable: json['isPickupAvailable'] == true,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'description': description,
      'logoUrl': logoUrl,
      'coverImageUrl': coverImageUrl,
      'rating': rating,
      'reviewCount': reviewCount,
      'deliveryFee': deliveryFee,
      'estimatedDeliveryTime': estimatedDeliveryTime,
      'distance': distance,
      'isOpen': isOpen,
      'address': address,
      'phoneNumber': phoneNumber,
      'categories': categories,
      'workingHours': workingHours,
      'minimumOrderAmount': minimumOrderAmount,
      'isDeliveryAvailable': isDeliveryAvailable,
      'isPickupAvailable': isPickupAvailable,
    };
  }

  Merchant toDomain() {
    return Merchant(
      id: id,
      name: name,
      description: description,
      logoUrl: logoUrl,
      coverImageUrl: coverImageUrl,
      rating: rating,
      reviewCount: reviewCount,
      deliveryFee: deliveryFee,
      estimatedDeliveryTime: estimatedDeliveryTime,
      distance: distance,
      isOpen: isOpen,
      address: address,
      phoneNumber: phoneNumber,
      categories: categories,
      workingHours: workingHours,
      minimumOrderAmount: minimumOrderAmount,
      isDeliveryAvailable: isDeliveryAvailable,
      isPickupAvailable: isPickupAvailable,
    );
  }
  
  /// Convert from Domain Entity (for API requests)
  factory MerchantDto.fromDomain(Merchant merchant) {
    return MerchantDto(
      id: merchant.id,
      name: merchant.name,
      description: merchant.description,
      logoUrl: merchant.logoUrl,
      coverImageUrl: merchant.coverImageUrl,
      rating: merchant.rating,
      reviewCount: merchant.reviewCount,
      deliveryFee: merchant.deliveryFee,
      estimatedDeliveryTime: merchant.estimatedDeliveryTime,
      distance: merchant.distance,
      isOpen: merchant.isOpen,
      address: merchant.address,
      phoneNumber: merchant.phoneNumber,
      categories: merchant.categories,
      workingHours: merchant.workingHours,
      minimumOrderAmount: merchant.minimumOrderAmount,
      isDeliveryAvailable: merchant.isDeliveryAvailable,
      isPickupAvailable: merchant.isPickupAvailable,
    );
  }
}
