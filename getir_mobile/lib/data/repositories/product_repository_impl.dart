import '../../domain/entities/product.dart';
import '../../domain/repositories/product_repository.dart';
import '../datasources/product_datasource.dart';

class ProductRepositoryImpl implements ProductRepository {
  final ProductDataSource _dataSource;

  ProductRepositoryImpl(this._dataSource);

  @override
  Future<List<Product>> getProducts({
    int page = 1,
    int limit = 20,
    String? merchantId,
    String? category,
    String? search,
  }) async {
    return await _dataSource.getProducts(
      page: page,
      limit: limit,
      merchantId: merchantId,
      category: category,
      search: search,
    );
  }

  @override
  Future<Product> getProductById(String id) async {
    return await _dataSource.getProductById(id);
  }

  @override
  Future<List<Product>> getProductsByMerchant(String merchantId) async {
    return await _dataSource.getProductsByMerchant(merchantId);
  }

  @override
  Future<List<Product>> searchProducts(String query) async {
    return await _dataSource.searchProducts(query);
  }

  @override
  Future<List<String>> getCategories() async {
    return await _dataSource.getCategories();
  }
}
