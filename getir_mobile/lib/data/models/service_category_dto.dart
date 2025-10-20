import '../../domain/entities/service_category.dart';
import '../../domain/entities/service_category_type.dart';

/// ServiceCategory Data Transfer Object
/// Backend'deki ServiceCategoryResponse ile eşleşir
class ServiceCategoryDto {
  final String id;
  final String name;
  final String? description;
  final int type;
  final String? imageUrl;
  final String? iconUrl;
  final int displayOrder;
  final bool isActive;
  final int merchantCount;

  const ServiceCategoryDto({
    required this.id,
    required this.name,
    this.description,
    required this.type,
    this.imageUrl,
    this.iconUrl,
    required this.displayOrder,
    required this.isActive,
    required this.merchantCount,
  });

  /// JSON'dan DTO oluştur
  factory ServiceCategoryDto.fromJson(Map<String, dynamic> json) {
    return ServiceCategoryDto(
      id: (json['id'] ?? '').toString(),
      name: (json['name'] ?? '').toString(),
      description: json['description']?.toString(),
      type: json['type'] as int? ?? 99, // 99 = Other
      imageUrl: json['imageUrl']?.toString(),
      iconUrl: json['iconUrl']?.toString(),
      displayOrder: json['displayOrder'] as int? ?? 0,
      isActive: json['isActive'] as bool? ?? true,
      merchantCount: json['merchantCount'] as int? ?? 0,
    );
  }

  /// DTO'yu JSON'a dönüştür
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'description': description,
      'type': type,
      'imageUrl': imageUrl,
      'iconUrl': iconUrl,
      'displayOrder': displayOrder,
      'isActive': isActive,
      'merchantCount': merchantCount,
    };
  }

  /// DTO'yu domain entity'sine dönüştür
  ServiceCategory toEntity() {
    return ServiceCategory(
      id: id,
      name: name,
      description: description,
      type: ServiceCategoryType.fromInt(type),
      imageUrl: imageUrl,
      iconUrl: iconUrl,
      displayOrder: displayOrder,
      isActive: isActive,
      merchantCount: merchantCount,
    );
  }

  /// Domain entity'sinden DTO oluştur
  factory ServiceCategoryDto.fromEntity(ServiceCategory entity) {
    return ServiceCategoryDto(
      id: entity.id,
      name: entity.name,
      description: entity.description,
      type: entity.type.value,
      imageUrl: entity.imageUrl,
      iconUrl: entity.iconUrl,
      displayOrder: entity.displayOrder,
      isActive: entity.isActive,
      merchantCount: entity.merchantCount,
    );
  }
}
