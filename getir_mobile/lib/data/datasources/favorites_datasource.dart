import 'package:dio/dio.dart';
import '../../domain/entities/favorite_product.dart';

abstract class FavoritesDataSource {
  Future<List<FavoriteProduct>> getFavorites({int page = 1, int limit = 20});
  Future<void> addToFavorites(String productId);
  Future<void> removeFromFavorites(String productId);
  Future<bool> isFavorite(String productId);
}

class FavoritesDataSourceImpl implements FavoritesDataSource {
  final Dio _dio;
  FavoritesDataSourceImpl(this._dio);

  @override
  Future<List<FavoriteProduct>> getFavorites({
    int page = 1,
    int limit = 20,
  }) async {
    final response = await _dio.get(
      '/api/v1/user/favorites',
      queryParameters: {'page': page, 'pageSize': limit},
    );

    // Handle ApiResponse format
    final responseData = response.data;
    if (responseData is Map<String, dynamic>) {
      if (responseData['success'] == true && responseData['value'] != null) {
        final favoritesList = responseData['value'] as List;
        return favoritesList
            .map(
              (json) => FavoriteProduct.fromJson(json as Map<String, dynamic>),
            )
            .toList();
      }
    }

    // Fallback for direct data
    if (responseData is List) {
      return responseData
          .map((json) => FavoriteProduct.fromJson(json as Map<String, dynamic>))
          .toList();
    }

    throw Exception('Invalid response format');
  }

  @override
  Future<void> addToFavorites(String productId) async {
    await _dio.post('/api/v1/user/favorites', data: {'productId': productId});
  }

  @override
  Future<void> removeFromFavorites(String productId) async {
    await _dio.delete('/api/v1/user/favorites/$productId');
  }

  @override
  Future<bool> isFavorite(String productId) async {
    final response = await _dio.get('/api/v1/user/favorites/$productId/status');

    // Handle ApiResponse format
    final responseData = response.data;
    if (responseData is Map<String, dynamic>) {
      if (responseData['success'] == true && responseData['value'] != null) {
        return responseData['value'] as bool;
      }
    }

    // Fallback for direct data
    return responseData as bool;
  }
}
