import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../domain/entities/product.dart';
import '../../domain/repositories/product_repository.dart';
import '../datasources/product_datasource.dart';

class ProductRepositoryImpl implements ProductRepository {
  final ProductDataSource _dataSource;

  ProductRepositoryImpl(this._dataSource);

  @override
  Future<Result<List<Product>>> getProducts({
    int page = 1,
    int limit = 20,
    String? merchantId,
    String? category,
    String? search,
  }) async {
    try {
      final products = await _dataSource.getProducts(
        page: page,
        limit: limit,
        merchantId: merchantId,
        category: category,
        search: search,
      );
      return Result.success(products);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get products: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<Product>> getProductById(String id) async {
    try {
      final product = await _dataSource.getProductById(id);
      return Result.success(product);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get product: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<List<Product>>> getProductsByMerchant(String merchantId) async {
    try {
      final products = await _dataSource.getProductsByMerchant(merchantId);
      return Result.success(products);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message: 'Failed to get products by merchant: ${e.toString()}',
        ),
      );
    }
  }

  @override
  Future<Result<List<Product>>> searchProducts(String query) async {
    try {
      final products = await _dataSource.searchProducts(query);
      return Result.success(products);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to search products: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<List<String>>> getCategories() async {
    try {
      final categories = await _dataSource.getCategories();
      return Result.success(categories);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get categories: ${e.toString()}'),
      );
    }
  }
}
