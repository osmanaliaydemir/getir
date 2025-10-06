import '../entities/product.dart';

abstract class ProductRepository {
  Future<List<Product>> getProducts({
    int page = 1,
    int limit = 20,
    String? merchantId,
    String? category,
    String? search,
  });

  Future<Product> getProductById(String id);
  Future<List<Product>> getProductsByMerchant(String merchantId);
  Future<List<Product>> searchProducts(String query);
  Future<List<String>> getCategories();
}
