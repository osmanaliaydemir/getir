import '../../core/errors/result.dart';
import '../entities/product.dart';
import '../repositories/product_repository.dart';

/// Product Service
///
/// Centralized service for all product-related operations.
/// Replaces 5 separate UseCase classes.
class ProductService {
  final IProductRepository _repository;

  const ProductService(this._repository);

  Future<Result<List<Product>>> getProducts({
    int page = 1,
    int limit = 20,
    String? merchantId,
    String? category,
  }) async {
    return await _repository.getProducts(
      page: page,
      limit: limit,
      merchantId: merchantId,
      category: category,
    );
  }

  Future<Result<Product>> getProductById(String productId) async {
    return await _repository.getProductById(productId);
  }

  Future<Result<List<Product>>> getProductsByMerchant(
    String merchantId, {
    int page = 1,
    int limit = 20,
  }) async {
    return await _repository.getProductsByMerchant(
      merchantId,
      page: page,
      limit: limit,
    );
  }

  Future<Result<List<Product>>> searchProducts(
    String query, {
    int page = 1,
    int limit = 20,
  }) async {
    return await _repository.searchProducts(query, page: page, limit: limit);
  }

  Future<Result<List<String>>> getCategories() async {
    return await _repository.getCategories();
  }

  Future<Result<List<Product>>> getPopularProducts({int limit = 10}) async {
    return await _repository.getPopularProducts(limit: limit);
  }
}
