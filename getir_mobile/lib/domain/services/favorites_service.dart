import '../../core/errors/result.dart';
import '../entities/favorite_product.dart';
import '../repositories/favorites_repository.dart';

/// Favorites Service
///
/// Centralized service for favorite products operations.
class FavoritesService {
  final IFavoritesRepository _repository;

  const FavoritesService(this._repository);

  Future<Result<List<FavoriteProduct>>> getFavorites({int page = 1, int limit = 20}) async {
    return await _repository.getFavorites(page: page, limit: limit);
  }

  Future<Result<void>> addToFavorites(String productId) async {
    return await _repository.addToFavorites(productId);
  }

  Future<Result<void>> removeFromFavorites(String productId) async {
    return await _repository.removeFromFavorites(productId);
  }

  Future<Result<bool>> isFavorite(String productId) async {
    return await _repository.isFavorite(productId);
  }
}
