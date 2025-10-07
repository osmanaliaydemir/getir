/// Service category types for merchants
/// Backend'deki ServiceCategoryType enum'ı ile eşleşir
enum ServiceCategoryType {
  restaurant(1, 'Restoran', 'Yemek siparişi ve teslimatı'),
  market(2, 'Market', 'Gıda ve temizlik ürünleri'),
  pharmacy(3, 'Eczane', 'İlaç ve sağlık ürünleri'),
  water(4, 'Su', 'Su teslimatı'),
  cafe(5, 'Kafe', 'Kahve ve atıştırmalık'),
  bakery(6, 'Pastane', 'Tatlı ve hamur işi'),
  other(99, 'Diğer', 'Diğer hizmetler');

  final int value;
  final String displayName;
  final String description;

  const ServiceCategoryType(this.value, this.displayName, this.description);

  /// Backend'e gönderilecek int değer
  int toInt() => value;

  /// Backend'den gelen int değerinden enum oluştur
  static ServiceCategoryType fromInt(int value) {
    return ServiceCategoryType.values.firstWhere(
      (e) => e.value == value,
      orElse: () => ServiceCategoryType.other,
    );
  }

  /// JSON serialization için
  static ServiceCategoryType? fromJson(int? json) {
    if (json == null) return null;
    return fromInt(json);
  }

  /// JSON deserialization için
  int toJson() => value;

  /// Yemek ile ilgili kategoriler mi?
  bool get isFoodRelated =>
      this == ServiceCategoryType.restaurant ||
      this == ServiceCategoryType.cafe ||
      this == ServiceCategoryType.bakery;

  /// Ürün ile ilgili kategoriler mi?
  bool get isProductRelated =>
      this == ServiceCategoryType.market ||
      this == ServiceCategoryType.pharmacy ||
      this == ServiceCategoryType.water;
}
