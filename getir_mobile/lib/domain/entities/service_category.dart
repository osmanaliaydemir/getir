import 'package:equatable/equatable.dart';
import 'service_category_type.dart';

/// Service category entity - Domain layer
/// Backend'deki ServiceCategory entity'si ile eşleşir
class ServiceCategory extends Equatable {
  final String id;
  final String name;
  final String? description;
  final ServiceCategoryType type;
  final String? imageUrl;
  final String? iconUrl;
  final int displayOrder;
  final bool isActive;
  final int merchantCount;

  const ServiceCategory({
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

  @override
  List<Object?> get props => [
    id,
    name,
    description,
    type,
    imageUrl,
    iconUrl,
    displayOrder,
    isActive,
    merchantCount,
  ];

  ServiceCategory copyWith({
    String? id,
    String? name,
    String? description,
    ServiceCategoryType? type,
    String? imageUrl,
    String? iconUrl,
    int? displayOrder,
    bool? isActive,
    int? merchantCount,
  }) {
    return ServiceCategory(
      id: id ?? this.id,
      name: name ?? this.name,
      description: description ?? this.description,
      type: type ?? this.type,
      imageUrl: imageUrl ?? this.imageUrl,
      iconUrl: iconUrl ?? this.iconUrl,
      displayOrder: displayOrder ?? this.displayOrder,
      isActive: isActive ?? this.isActive,
      merchantCount: merchantCount ?? this.merchantCount,
    );
  }
}
