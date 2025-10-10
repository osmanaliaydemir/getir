import '../../../domain/entities/address.dart';

class UserAddressDto {
  final String id;
  final String userId;
  final String title;
  final String fullAddress;
  final String? buildingNumber;
  final String? floor;
  final String? apartment;
  final String? landmark;
  final double latitude;
  final double longitude;
  final String type;
  final bool isDefault;
  final DateTime createdAt;
  final DateTime updatedAt;

  const UserAddressDto({
    required this.id,
    required this.userId,
    required this.title,
    required this.fullAddress,
    this.buildingNumber,
    this.floor,
    this.apartment,
    this.landmark,
    required this.latitude,
    required this.longitude,
    required this.type,
    required this.isDefault,
    required this.createdAt,
    required this.updatedAt,
  });

  factory UserAddressDto.fromJson(Map<String, dynamic> json) {
    return UserAddressDto(
      id: (json['id'] ?? '').toString(),
      userId: (json['userId'] ?? '').toString(),
      title: (json['title'] ?? '').toString(),
      fullAddress: (json['fullAddress'] ?? '').toString(),
      buildingNumber: (json['buildingNumber'] as String?),
      floor: (json['floor'] as String?),
      apartment: (json['apartment'] as String?),
      landmark: (json['landmark'] as String?),
      latitude: json['latitude'] is num
          ? (json['latitude'] as num).toDouble()
          : 0.0,
      longitude: json['longitude'] is num
          ? (json['longitude'] as num).toDouble()
          : 0.0,
      type: (json['type'] ?? 'other').toString(),
      isDefault: json['isDefault'] == true,
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
      'title': title,
      'fullAddress': fullAddress,
      'buildingNumber': buildingNumber,
      'floor': floor,
      'apartment': apartment,
      'landmark': landmark,
      'latitude': latitude,
      'longitude': longitude,
      'type': type,
      'isDefault': isDefault,
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt.toIso8601String(),
    };
  }

  UserAddress toDomain() {
    return UserAddress(
      id: id,
      userId: userId,
      title: title,
      fullAddress: fullAddress,
      buildingNumber: buildingNumber,
      floor: floor,
      apartment: apartment,
      landmark: landmark,
      latitude: latitude,
      longitude: longitude,
      type: AddressType.fromString(type),
      isDefault: isDefault,
      createdAt: createdAt,
      updatedAt: updatedAt,
    );
  }
  
  /// Convert from Domain Entity (for API requests)
  factory UserAddressDto.fromDomain(UserAddress address) {
    return UserAddressDto(
      id: address.id,
      userId: address.userId,
      title: address.title,
      fullAddress: address.fullAddress,
      buildingNumber: address.buildingNumber,
      floor: address.floor,
      apartment: address.apartment,
      landmark: address.landmark,
      latitude: address.latitude,
      longitude: address.longitude,
      type: address.type.value,
      isDefault: address.isDefault,
      createdAt: address.createdAt,
      updatedAt: address.updatedAt,
    );
  }
}
