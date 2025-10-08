import '../../core/errors/result.dart';
import '../entities/product.dart';

abstract class ProductRepository {
  Future<Result<List<Product>>> getProducts({
    int page = 1,
    int limit = 20,
    String? merchantId,
    String? category,
    String? search,
  });

  Future<Result<Product>> getProductById(String id);
  Future<Result<List<Product>>> getProductsByMerchant(String merchantId);
  Future<Result<List<Product>>> searchProducts(String query);
  Future<Result<List<String>>> getCategories();
}
