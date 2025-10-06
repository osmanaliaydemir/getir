import '../../domain/entities/merchant.dart';
import '../../domain/repositories/merchant_repository.dart';
import '../datasources/merchant_datasource.dart';

class MerchantRepositoryImpl implements MerchantRepository {
  final MerchantDataSource _dataSource;

  MerchantRepositoryImpl(this._dataSource);

  @override
  Future<List<Merchant>> getMerchants({
    int page = 1,
    int limit = 20,
    String? search,
    String? category,
    double? latitude,
    double? longitude,
    double? radius,
  }) async {
    return await _dataSource.getMerchants(
      page: page,
      limit: limit,
      search: search,
      category: category,
      latitude: latitude,
      longitude: longitude,
      radius: radius,
    );
  }

  @override
  Future<Merchant> getMerchantById(String id) async {
    return await _dataSource.getMerchantById(id);
  }

  @override
  Future<List<Merchant>> searchMerchants(String query) async {
    return await _dataSource.searchMerchants(query);
  }

  @override
  Future<List<Merchant>> getNearbyMerchants({
    required double latitude,
    required double longitude,
    double radius = 5.0,
  }) async {
    return await _dataSource.getNearbyMerchants(
      latitude: latitude,
      longitude: longitude,
      radius: radius,
    );
  }
}
