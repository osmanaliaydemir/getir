import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../domain/entities/service_category.dart';
import '../../domain/entities/service_category_type.dart';
import '../../domain/repositories/service_category_repository.dart';
import '../datasources/service_category_datasource.dart';

/// ServiceCategory repository implementasyonu - Data layer
class ServiceCategoryRepositoryImpl implements IServiceCategoryRepository {
  final ServiceCategoryDataSource _dataSource;

  ServiceCategoryRepositoryImpl(this._dataSource);

  @override
  Future<Result<List<ServiceCategory>>> getServiceCategories({
    int page = 1,
    int pageSize = 20,
  }) async {
    try {
      final dtos = await _dataSource.getServiceCategories(
        page: page,
        pageSize: pageSize,
      );
      final entities = dtos.map((dto) => dto.toEntity()).toList();
      return Result.success(entities);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message: 'Failed to get service categories: ${e.toString()}',
        ),
      );
    }
  }

  @override
  Future<Result<ServiceCategory>> getServiceCategoryById(String id) async {
    try {
      final dto = await _dataSource.getServiceCategoryById(id);
      return Result.success(dto.toEntity());
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message: 'Failed to get service category: ${e.toString()}',
        ),
      );
    }
  }

  @override
  Future<Result<List<ServiceCategory>>> getActiveServiceCategoriesByType(
    ServiceCategoryType type,
  ) async {
    try {
      final dtos = await _dataSource.getActiveServiceCategoriesByType(type);
      final entities = dtos.map((dto) => dto.toEntity()).toList();
      return Result.success(entities);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message: 'Failed to get service categories by type: ${e.toString()}',
        ),
      );
    }
  }

  @override
  Future<Result<List<ServiceCategory>>> getAllActiveCategories() async {
    try {
      final dtos = await _dataSource.getAllActiveCategories();
      final entities = dtos.map((dto) => dto.toEntity()).toList();
      return Result.success(entities);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message: 'Failed to get all active categories: ${e.toString()}',
        ),
      );
    }
  }
}
