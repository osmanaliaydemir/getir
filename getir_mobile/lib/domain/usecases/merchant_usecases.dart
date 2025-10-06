import '../entities/merchant.dart';
import '../repositories/merchant_repository.dart';

class GetMerchantsUseCase {
  final MerchantRepository _repository;

  GetMerchantsUseCase(this._repository);

  Future<List<Merchant>> call({
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

class GetMerchantByIdUseCase {
  final MerchantRepository _repository;

  GetMerchantByIdUseCase(this._repository);

  Future<Merchant> call(String id) async {
    return await _repository.getMerchantById(id);
  }
}

class SearchMerchantsUseCase {
  final MerchantRepository _repository;

  SearchMerchantsUseCase(this._repository);

  Future<List<Merchant>> call(String query) async {
    return await _repository.searchMerchants(query);
  }
}

class GetNearbyMerchantsUseCase {
  final MerchantRepository _repository;

  GetNearbyMerchantsUseCase(this._repository);

  Future<List<Merchant>> call({
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
