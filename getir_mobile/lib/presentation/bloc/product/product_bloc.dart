import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../core/models/pagination_model.dart';
import '../../../domain/entities/product.dart';
import '../../../domain/services/product_service.dart';

// Events
abstract class ProductEvent extends Equatable {
  const ProductEvent();

  @override
  List<Object?> get props => [];
}

class LoadProducts extends ProductEvent {
  final int page;
  final int limit;
  final String? merchantId;
  final String? category;
  final String? search;

  const LoadProducts({
    this.page = 1,
    this.limit = 20,
    this.merchantId,
    this.category,
    this.search,
  });

  @override
  List<Object?> get props => [page, limit, merchantId, category, search];
}

class LoadProductById extends ProductEvent {
  final String id;

  const LoadProductById(this.id);

  @override
  List<Object> get props => [id];
}

class LoadProductsByMerchant extends ProductEvent {
  final String merchantId;

  const LoadProductsByMerchant(this.merchantId);

  @override
  List<Object> get props => [merchantId];
}

class SearchProducts extends ProductEvent {
  final String query;

  const SearchProducts(this.query);

  @override
  List<Object> get props => [query];
}

class LoadProductsByCategory extends ProductEvent {
  final String category;

  const LoadProductsByCategory(this.category);

  @override
  List<Object> get props => [category];
}

class LoadCategories extends ProductEvent {}

// Popular Products Event
class LoadPopularProducts extends ProductEvent {
  final int limit;

  const LoadPopularProducts({this.limit = 10});

  @override
  List<Object?> get props => [limit];
}

// 🔄 Pagination Events
class LoadMoreProducts extends ProductEvent {
  const LoadMoreProducts();
}

class RefreshProducts extends ProductEvent {
  final String? merchantId;
  final String? category;
  final String? search;

  const RefreshProducts({this.merchantId, this.category, this.search});

  @override
  List<Object?> get props => [merchantId, category, search];
}

// States
abstract class ProductState extends Equatable {
  const ProductState();

  @override
  List<Object?> get props => [];
}

class ProductInitial extends ProductState {}

class ProductLoading extends ProductState {}

class ProductLoaded extends ProductState {
  final Product product;

  const ProductLoaded(this.product);

  @override
  List<Object> get props => [product];
}

class ProductsLoaded extends ProductState {
  final List<Product> products;
  final PaginationModel<Product>? pagination; // 🔄 Pagination support

  const ProductsLoaded(this.products, {this.pagination});

  @override
  List<Object?> get props => [products, pagination];

  // ✅ Check if pagination is active
  bool get hasPagination => pagination != null;

  // ✅ Check if can load more
  bool get canLoadMore => pagination?.hasNextPage ?? false;
}

class ProductCategoriesLoaded extends ProductState {
  final List<String> categories;

  const ProductCategoriesLoaded(this.categories);

  @override
  List<Object> get props => [categories];
}

class ProductError extends ProductState {
  final String message;

  const ProductError(this.message);

  @override
  List<Object> get props => [message];
}

// BLoC
class ProductBloc extends Bloc<ProductEvent, ProductState> {
  final ProductService _productService;

  ProductBloc(this._productService) : super(ProductInitial()) {
    on<LoadProducts>(_onLoadProducts);
    on<LoadProductById>(_onLoadProductById);
    on<LoadProductsByMerchant>(_onLoadProductsByMerchant);
    on<SearchProducts>(_onSearchProducts);
    on<LoadProductsByCategory>(_onLoadProductsByCategory);
    on<LoadCategories>(_onLoadCategories);
    on<LoadPopularProducts>(_onLoadPopularProducts);
    on<LoadMoreProducts>(_onLoadMoreProducts); // 🔄 Pagination
    on<RefreshProducts>(_onRefreshProducts); // 🔄 Pagination
  }

  Future<void> _onLoadProducts(
    LoadProducts event,
    Emitter<ProductState> emit,
  ) async {
    emit(ProductLoading());

    final result = await _productService.getProducts(
      page: event.page,
      limit: event.limit,
      merchantId: event.merchantId,
      category: event.category,
    );

    result.when(
      success: (products) => emit(ProductsLoaded(products)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(ProductError(message));
      },
    );
  }

  Future<void> _onLoadProductById(
    LoadProductById event,
    Emitter<ProductState> emit,
  ) async {
    emit(ProductLoading());

    final result = await _productService.getProductById(event.id);

    result.when(
      success: (product) => emit(ProductLoaded(product)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(ProductError(message));
      },
    );
  }

  Future<void> _onLoadProductsByMerchant(
    LoadProductsByMerchant event,
    Emitter<ProductState> emit,
  ) async {
    emit(ProductLoading());

    final result = await _productService.getProductsByMerchant(
      event.merchantId,
    );

    result.when(
      success: (products) => emit(ProductsLoaded(products)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(ProductError(message));
      },
    );
  }

  Future<void> _onSearchProducts(
    SearchProducts event,
    Emitter<ProductState> emit,
  ) async {
    emit(ProductLoading());

    final result = await _productService.searchProducts(event.query);

    result.when(
      success: (products) => emit(ProductsLoaded(products)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(ProductError(message));
      },
    );
  }

  Future<void> _onLoadProductsByCategory(
    LoadProductsByCategory event,
    Emitter<ProductState> emit,
  ) async {
    emit(ProductLoading());

    final result = await _productService.getProducts(category: event.category);

    result.when(
      success: (products) => emit(ProductsLoaded(products)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(ProductError(message));
      },
    );
  }

  Future<void> _onLoadCategories(
    LoadCategories event,
    Emitter<ProductState> emit,
  ) async {
    final result = await _productService.getCategories();

    result.when(
      success: (categories) => emit(ProductCategoriesLoaded(categories)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(ProductError(message));
      },
    );
  }

  /// Extract user-friendly error message from exception
  String _getErrorMessage(Exception exception) {
    if (exception is AppException) {
      return exception.message;
    }
    return 'An unexpected error occurred';
  }

  // 🔄 ============= PAGINATION HANDLERS =============

  /// Load more products (infinite scroll)
  Future<void> _onLoadMoreProducts(
    LoadMoreProducts event,
    Emitter<ProductState> emit,
  ) async {
    // Only load more if current state is ProductsLoaded with pagination
    if (state is! ProductsLoaded) return;

    final currentState = state as ProductsLoaded;

    // Check if pagination exists and has next page
    if (currentState.pagination == null ||
        !currentState.pagination!.hasNextPage ||
        currentState.pagination!.isLoading) {
      return;
    }

    // Set loading state
    final loadingPagination = currentState.pagination!.setLoading(true);
    emit(ProductsLoaded(currentState.products, pagination: loadingPagination));

    // Calculate next page
    final nextPage = currentState.pagination!.nextPage;

    // Load next page
    final result = await _productService.getProducts(page: nextPage, limit: 20);

    result.when(
      success: (newProducts) {
        // Add new products to existing list
        final updatedProducts = [...currentState.products, ...newProducts];
        final updatedPagination = currentState.pagination!
            .addItems(newProducts)
            .copyWith(
              currentPage: nextPage,
              hasNextPage: newProducts.length >= 20, // Has more if full page
              isLoading: false,
            );

        emit(ProductsLoaded(updatedProducts, pagination: updatedPagination));
      },
      failure: (exception) {
        // Reset loading state on error
        final errorPagination = currentState.pagination!.setLoading(false);
        emit(
          ProductsLoaded(currentState.products, pagination: errorPagination),
        );

        // Optionally emit error state (or handle silently)
        final message = _getErrorMessage(exception);
        emit(ProductError(message));
      },
    );
  }

  /// Refresh products (pull-to-refresh)
  Future<void> _onRefreshProducts(
    RefreshProducts event,
    Emitter<ProductState> emit,
  ) async {
    // Set refreshing state
    if (state is ProductsLoaded) {
      final currentState = state as ProductsLoaded;
      if (currentState.pagination != null) {
        final refreshingPagination = currentState.pagination!.setRefreshing(
          true,
        );
        emit(
          ProductsLoaded(
            currentState.products,
            pagination: refreshingPagination,
          ),
        );
      }
    }

    // Load first page
    final result = await _productService.getProducts(
      page: 1,
      limit: 20,
      merchantId: event.merchantId,
      category: event.category,
    );

    result.when(
      success: (products) {
        // Replace all products
        final pagination = PaginationModel<Product>(
          items: products,
          currentPage: 1,
          totalPages: 999, // Unknown, will update later
          totalItems: products.length,
          hasNextPage: products.length >= 20,
          hasPreviousPage: false,
          isLoading: false,
          isRefreshing: false,
        );

        emit(ProductsLoaded(products, pagination: pagination));
      },
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(ProductError(message));
      },
    );
  }

  Future<void> _onLoadPopularProducts(
    LoadPopularProducts event,
    Emitter<ProductState> emit,
  ) async {
    emit(ProductLoading());

    final result = await _productService.getPopularProducts(limit: event.limit);

    result.when(
      success: (products) {
        emit(ProductsLoaded(products));
      },
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(ProductError(message));
      },
    );
  }
}
