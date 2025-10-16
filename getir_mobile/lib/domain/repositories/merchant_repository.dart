import '../../core/errors/result.dart';
import '../entities/merchant.dart';

abstract class IMerchantRepository {
  Future<Result<List<Merchant>>> getMerchants({
    int page = 1,
    int limit = 20,
    String? search,
    String? category,
    double? latitude,
    double? longitude,
    double? radius,
  });

  Future<Result<Merchant>> getMerchantById(String id);
  Future<Result<List<Merchant>>> searchMerchants(String query);
  Future<Result<List<Merchant>>> getNearbyMerchants({
    required double latitude,
    required double longitude,
    double radius = 5.0,
  });

  Future<Result<List<Merchant>>> getNearbyMerchantsByCategory({
    required double latitude,
    required double longitude,
    required int categoryType,
    double radius = 5.0,
  });
}
