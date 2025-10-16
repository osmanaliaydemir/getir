import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../domain/entities/merchant.dart';
import '../../domain/repositories/merchant_repository.dart';
import '../datasources/merchant_datasource.dart';

class MerchantRepositoryImpl implements IMerchantRepository {
  final MerchantDataSource _dataSource;

  MerchantRepositoryImpl(this._dataSource);

  @override
  Future<Result<List<Merchant>>> getMerchants({
    int page = 1,
    int limit = 20,
    String? search,
    String? category,
    double? latitude,
    double? longitude,
    double? radius,
  }) async {
    try {
      final merchants = await _dataSource.getMerchants(
        page: page,
        limit: limit,
        search: search,
        category: category,
        latitude: latitude,
        longitude: longitude,
        radius: radius,
      );
      return Result.success(merchants);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get merchants: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<Merchant>> getMerchantById(String id) async {
    try {
      final merchant = await _dataSource.getMerchantById(id);
      return Result.success(merchant);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get merchant: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<List<Merchant>>> searchMerchants(String query) async {
    try {
      final merchants = await _dataSource.searchMerchants(query);
      return Result.success(merchants);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to search merchants: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<List<Merchant>>> getNearbyMerchants({
    required double latitude,
    required double longitude,
    double radius = 5.0,
  }) async {
    try {
      final merchants = await _dataSource.getNearbyMerchants(
        latitude: latitude,
        longitude: longitude,
        radius: radius,
      );
      return Result.success(merchants);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message: 'Failed to get nearby merchants: ${e.toString()}',
        ),
      );
    }
  }

  @override
  Future<Result<List<Merchant>>> getNearbyMerchantsByCategory({
    required double latitude,
    required double longitude,
    required int categoryType,
    double radius = 5.0,
  }) async {
    try {
      final merchants = await _dataSource.getNearbyMerchantsByCategory(
        latitude: latitude,
        longitude: longitude,
        categoryType: categoryType,
        radius: radius,
      );
      return Result.success(merchants);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message:
              'Failed to get nearby merchants by category: ${e.toString()}',
        ),
      );
    }
  }
}
