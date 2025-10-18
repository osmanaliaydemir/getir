import '../../core/errors/result.dart';
import '../entities/favorite_product.dart';

abstract class IFavoritesRepository {
  Future<Result<List<FavoriteProduct>>> getFavorites({
    int page = 1,
    int limit = 20,
  });
  Future<Result<void>> addToFavorites(String productId);
  Future<Result<void>> removeFromFavorites(String productId);
  Future<Result<bool>> isFavorite(String productId);
}
