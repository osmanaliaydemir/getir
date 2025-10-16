import '../../core/errors/result.dart';
import '../entities/merchant.dart';
import '../repositories/merchant_repository.dart';

/// Merchant Service
///
/// Centralized service for all merchant-related operations.
/// Replaces 5 separate UseCase classes.
class MerchantService {
  final IMerchantRepository _repository;

  const MerchantService(this._repository);

  Future<Result<List<Merchant>>> getMerchants({
    int page = 1,
    int limit = 20,
    String? search,
    String? category,
  }) async {
    return await _repository.getMerchants(
      page: page,
      limit: limit,
      search: search,
      category: category,
    );
  }

  Future<Result<Merchant>> getMerchantById(String merchantId) async {
    return await _repository.getMerchantById(merchantId);
  }

  Future<Result<List<Merchant>>> searchMerchants(String query) async {
    return await _repository.searchMerchants(query);
  }

  Future<Result<List<Merchant>>> getNearbyMerchants({
    required double latitude,
    required double longitude,
    double radius = 5.0,
  }) async {
    return await _repository.getNearbyMerchants(
      latitude: latitude,
      longitude: longitude,
      radius: radius,
    );
  }

  Future<Result<List<Merchant>>> getNearbyMerchantsByCategory({
    required double latitude,
    required double longitude,
    required int categoryType,
    double radius = 5.0,
  }) async {
    return await _repository.getNearbyMerchantsByCategory(
      latitude: latitude,
      longitude: longitude,
      categoryType: categoryType,
      radius: radius,
    );
  }
}
