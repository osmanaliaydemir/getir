import '../entities/merchant.dart';

abstract class MerchantRepository {
  Future<List<Merchant>> getMerchants({
    int page = 1,
    int limit = 20,
    String? search,
    String? category,
    double? latitude,
    double? longitude,
    double? radius,
  });

  Future<Merchant> getMerchantById(String id);
  Future<List<Merchant>> searchMerchants(String query);
  Future<List<Merchant>> getNearbyMerchants({
    required double latitude,
    required double longitude,
    double radius = 5.0,
  });

  Future<List<Merchant>> getNearbyMerchantsByCategory({
    required double latitude,
    required double longitude,
    required int categoryType,
    double radius = 5.0,
  });
}
