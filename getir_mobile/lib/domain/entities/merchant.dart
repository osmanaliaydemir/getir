import 'package:equatable/equatable.dart';
import 'service_category_type.dart';

class Merchant extends Equatable {
  final String id;
  final String name;
  final String description;
  final String logoUrl;
  final String coverImageUrl;
  final double rating;
  final int reviewCount;
  final double deliveryFee;
  final int estimatedDeliveryTime; // minutes
  final double distance; // kilometers
  final bool isOpen;
  final String address;
  final String phoneNumber;
  final List<String> categories;
  final Map<String, String> workingHours;
  final double minimumOrderAmount;
  final bool isDeliveryAvailable;
  final bool isPickupAvailable;
  final ServiceCategoryType?
  categoryType; // Servis kategori tipi (Market, Restaurant, vb.)

  const Merchant({
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
    this.categoryType,
  });

  @override
  List<Object?> get props => [
    id,
    name,
    description,
    logoUrl,
    coverImageUrl,
    rating,
    reviewCount,
    deliveryFee,
    estimatedDeliveryTime,
    distance,
    isOpen,
    address,
    phoneNumber,
    categories,
    workingHours,
    minimumOrderAmount,
    isDeliveryAvailable,
    isPickupAvailable,
    categoryType,
  ];

  Merchant copyWith({
    String? id,
    String? name,
    String? description,
    String? logoUrl,
    String? coverImageUrl,
    double? rating,
    int? reviewCount,
    double? deliveryFee,
    int? estimatedDeliveryTime,
    double? distance,
    bool? isOpen,
    String? address,
    String? phoneNumber,
    List<String>? categories,
    Map<String, String>? workingHours,
    double? minimumOrderAmount,
    bool? isDeliveryAvailable,
    bool? isPickupAvailable,
    ServiceCategoryType? categoryType,
  }) {
    return Merchant(
      id: id ?? this.id,
      name: name ?? this.name,
      description: description ?? this.description,
      logoUrl: logoUrl ?? this.logoUrl,
      coverImageUrl: coverImageUrl ?? this.coverImageUrl,
      rating: rating ?? this.rating,
      reviewCount: reviewCount ?? this.reviewCount,
      deliveryFee: deliveryFee ?? this.deliveryFee,
      estimatedDeliveryTime:
          estimatedDeliveryTime ?? this.estimatedDeliveryTime,
      distance: distance ?? this.distance,
      isOpen: isOpen ?? this.isOpen,
      address: address ?? this.address,
      phoneNumber: phoneNumber ?? this.phoneNumber,
      categories: categories ?? this.categories,
      workingHours: workingHours ?? this.workingHours,
      minimumOrderAmount: minimumOrderAmount ?? this.minimumOrderAmount,
      isDeliveryAvailable: isDeliveryAvailable ?? this.isDeliveryAvailable,
      isPickupAvailable: isPickupAvailable ?? this.isPickupAvailable,
      categoryType: categoryType ?? this.categoryType,
    );
  }
}
