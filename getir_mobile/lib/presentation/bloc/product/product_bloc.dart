import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/entities/product.dart';
import '../../../domain/usecases/product_usecases.dart';

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

  const ProductsLoaded(this.products);

  @override
  List<Object> get props => [products];
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
  final GetProductsUseCase _getProductsUseCase;
  final GetProductByIdUseCase _getProductByIdUseCase;
  final GetProductsByMerchantUseCase _getProductsByMerchantUseCase;
  final SearchProductsUseCase _searchProductsUseCase;
  final GetCategoriesUseCase _getCategoriesUseCase;

  ProductBloc({
    required GetProductsUseCase getProductsUseCase,
    required GetProductByIdUseCase getProductByIdUseCase,
    required GetProductsByMerchantUseCase getProductsByMerchantUseCase,
    required SearchProductsUseCase searchProductsUseCase,
    required GetCategoriesUseCase getCategoriesUseCase,
  }) : _getProductsUseCase = getProductsUseCase,
       _getProductByIdUseCase = getProductByIdUseCase,
       _getProductsByMerchantUseCase = getProductsByMerchantUseCase,
       _searchProductsUseCase = searchProductsUseCase,
       _getCategoriesUseCase = getCategoriesUseCase,
       super(ProductInitial()) {
    on<LoadProducts>(_onLoadProducts);
    on<LoadProductById>(_onLoadProductById);
    on<LoadProductsByMerchant>(_onLoadProductsByMerchant);
    on<SearchProducts>(_onSearchProducts);
    on<LoadProductsByCategory>(_onLoadProductsByCategory);
    on<LoadCategories>(_onLoadCategories);
  }

  Future<void> _onLoadProducts(
    LoadProducts event,
    Emitter<ProductState> emit,
  ) async {
    emit(ProductLoading());

    final result = await _getProductsUseCase(
      page: event.page,
      limit: event.limit,
      merchantId: event.merchantId,
      category: event.category,
      search: event.search,
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

    final result = await _getProductByIdUseCase(event.id);

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

    final result = await _getProductsByMerchantUseCase(event.merchantId);

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

    final result = await _searchProductsUseCase(event.query);

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

    final result = await _getProductsUseCase(category: event.category);

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
    final result = await _getCategoriesUseCase();

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
}
