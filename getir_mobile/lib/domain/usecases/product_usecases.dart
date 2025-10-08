import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../entities/product.dart';
import '../repositories/product_repository.dart';

/// Get Products Use Case
class GetProductsUseCase {
  final ProductRepository _repository;

  GetProductsUseCase(this._repository);

  Future<Result<List<Product>>> call({
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

/// Get Product By ID Use Case
class GetProductByIdUseCase {
  final ProductRepository _repository;

  GetProductByIdUseCase(this._repository);

  Future<Result<Product>> call(String id) async {
    if (id.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Product ID cannot be empty',
          code: 'EMPTY_PRODUCT_ID',
        ),
      );
    }

    return await _repository.getProductById(id);
  }
}

/// Get Products By Merchant Use Case
class GetProductsByMerchantUseCase {
  final ProductRepository _repository;

  GetProductsByMerchantUseCase(this._repository);

  Future<Result<List<Product>>> call(String merchantId) async {
    if (merchantId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Merchant ID cannot be empty',
          code: 'EMPTY_MERCHANT_ID',
        ),
      );
    }

    return await _repository.getProductsByMerchant(merchantId);
  }
}

/// Search Products Use Case
class SearchProductsUseCase {
  final ProductRepository _repository;

  SearchProductsUseCase(this._repository);

  Future<Result<List<Product>>> call(String query) async {
    if (query.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Search query cannot be empty',
          code: 'EMPTY_SEARCH_QUERY',
        ),
      );
    }

    return await _repository.searchProducts(query);
  }
}

/// Get Categories Use Case
class GetCategoriesUseCase {
  final ProductRepository _repository;

  GetCategoriesUseCase(this._repository);

  Future<Result<List<String>>> call() async {
    return await _repository.getCategories();
  }
}