import '../entities/product.dart';
import '../repositories/product_repository.dart';

/// Product Use Cases
///
/// **Note:** These are currently simple repository wrappers.
/// Future enhancements should include:
/// - Stock validation
/// - Price history tracking
/// - Recommendation engine
/// - Analytics (product views, searches)
/// - Inventory alerts

class GetProductsUseCase {
  final ProductRepository _repository;

  GetProductsUseCase(this._repository);

  Future<List<Product>> call({
    int page = 1,
    int limit = 20,
    String? merchantId,
    String? category,
    String? search,
  }) async {
    return await _repository.getProducts(
      page: page,
      limit: limit,
      merchantId: merchantId,
      category: category,
      search: search,
    );
  }
}

class GetProductByIdUseCase {
  final ProductRepository _repository;

  GetProductByIdUseCase(this._repository);

  Future<Product> call(String id) async {
    return await _repository.getProductById(id);
  }
}

class GetProductsByMerchantUseCase {
  final ProductRepository _repository;

  GetProductsByMerchantUseCase(this._repository);

  Future<List<Product>> call(String merchantId) async {
    return await _repository.getProductsByMerchant(merchantId);
  }
}

class SearchProductsUseCase {
  final ProductRepository _repository;

  SearchProductsUseCase(this._repository);

  Future<List<Product>> call(String query) async {
    return await _repository.searchProducts(query);
  }
}

class GetCategoriesUseCase {
  final ProductRepository _repository;

  GetCategoriesUseCase(this._repository);

  Future<List<String>> call() async {
    return await _repository.getCategories();
  }
}
