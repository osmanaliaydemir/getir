import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../entities/merchant.dart';
import '../repositories/merchant_repository.dart';

/// Get Merchants Use Case
class GetMerchantsUseCase {
  final MerchantRepository _repository;

  GetMerchantsUseCase(this._repository);

  Future<Result<List<Merchant>>> call({
    int page = 1,
    int limit = 20,
    String? search,
    String? category,
    double? latitude,
    double? longitude,
    double? radius,
  }) async {
    return await _repository.getMerchants(
      page: page,
      limit: limit,
      search: search,
      category: category,
      latitude: latitude,
      longitude: longitude,
      radius: radius,
    );
  }
}

/// Get Merchant By ID Use Case
class GetMerchantByIdUseCase {
  final MerchantRepository _repository;

  GetMerchantByIdUseCase(this._repository);

  Future<Result<Merchant>> call(String id) async {
    if (id.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Merchant ID cannot be empty',
          code: 'EMPTY_MERCHANT_ID',
        ),
      );
    }

    return await _repository.getMerchantById(id);
  }
}

/// Search Merchants Use Case
class SearchMerchantsUseCase {
  final MerchantRepository _repository;

  SearchMerchantsUseCase(this._repository);

  Future<Result<List<Merchant>>> call(String query) async {
    if (query.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Search query cannot be empty',
          code: 'EMPTY_SEARCH_QUERY',
        ),
      );
    }

    return await _repository.searchMerchants(query);
  }
}

/// Get Nearby Merchants Use Case
class GetNearbyMerchantsUseCase {
  final MerchantRepository _repository;

  GetNearbyMerchantsUseCase(this._repository);

  Future<Result<List<Merchant>>> call({
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
}

/// Get Nearby Merchants By Category Use Case
class GetNearbyMerchantsByCategoryUseCase {
  final MerchantRepository _repository;

  GetNearbyMerchantsByCategoryUseCase(this._repository);

  Future<Result<List<Merchant>>> call({
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
