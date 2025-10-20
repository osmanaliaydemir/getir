import '../../core/errors/result.dart';
import '../entities/product.dart';

abstract class IProductRepository {
  Future<Result<List<Product>>> getProducts({
    int page = 1,
    int limit = 20,
    String? merchantId,
    String? category,
    String? search,
  });

  Future<Result<Product>> getProductById(String id);
  Future<Result<List<Product>>> getProductsByMerchant(
    String merchantId, {
    int page = 1,
    int limit = 20,
  });
  Future<Result<List<Product>>> searchProducts(
    String query, {
    int page = 1,
    int limit = 20,
  });
  Future<Result<List<String>>> getCategories();
  
  /// Popüler ürünleri getir (en çok satılan ve yüksek ratingli)
  Future<Result<List<Product>>> getPopularProducts({int limit = 10});
}
