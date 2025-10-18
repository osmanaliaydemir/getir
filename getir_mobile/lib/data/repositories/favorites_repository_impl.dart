import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../domain/entities/favorite_product.dart';
import '../../domain/repositories/favorites_repository.dart';
import '../datasources/favorites_datasource.dart';

class FavoritesRepositoryImpl implements IFavoritesRepository {
  final FavoritesDataSource _dataSource;

  FavoritesRepositoryImpl(this._dataSource);

  @override
  Future<Result<List<FavoriteProduct>>> getFavorites({
    int page = 1,
    int limit = 20,
  }) async {
    try {
      final favorites = await _dataSource.getFavorites(
        page: page,
        limit: limit,
      );
      return Result.success(favorites);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get favorites: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<void>> addToFavorites(String productId) async {
    try {
      await _dataSource.addToFavorites(productId);
      return Result.success(null);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to add to favorites: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<void>> removeFromFavorites(String productId) async {
    try {
      await _dataSource.removeFromFavorites(productId);
      return Result.success(null);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message: 'Failed to remove from favorites: ${e.toString()}',
        ),
      );
    }
  }

  @override
  Future<Result<bool>> isFavorite(String productId) async {
    try {
      final isFavorite = await _dataSource.isFavorite(productId);
      return Result.success(isFavorite);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message: 'Failed to check favorite status: ${e.toString()}',
        ),
      );
    }
  }
}
