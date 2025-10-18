import 'dart:async';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../core/models/pagination_model.dart';
import 'search_event.dart';
import 'search_state.dart';
import '../../../domain/services/merchant_service.dart';
import '../../../domain/services/product_service.dart';
import '../../../core/services/search_history_service.dart';

class SearchBloc extends Bloc<SearchEvent, SearchState> {
  final MerchantService _merchantService;
  final ProductService _productService;
  final SearchHistoryService _searchHistoryService;

  Timer? _debounceTimer;

  SearchBloc(
    this._merchantService,
    this._productService,
    this._searchHistoryService,
  ) : super(const SearchInitial()) {
    on<SearchQueryChanged>(_onSearchQueryChanged);
    on<SearchTypeChanged>(_onSearchTypeChanged);
    on<SearchSubmitted>(_onSearchSubmitted);
    on<SearchHistoryLoaded>(_onSearchHistoryLoaded);
    on<SearchHistoryCleared>(_onSearchHistoryCleared);
    on<SearchHistoryItemRemoved>(_onSearchHistoryItemRemoved);
    on<LoadMoreSearchResults>(_onLoadMoreSearchResults);
    on<RefreshSearchResults>(_onRefreshSearchResults);
  }

  Future<void> _onSearchQueryChanged(
    SearchQueryChanged event,
    Emitter<SearchState> emit,
  ) async {
    // Cancel previous debounce timer
    _debounceTimer?.cancel();

    if (event.query.isEmpty) {
      final history = await _searchHistoryService.getSearchHistory();
      emit(
        SearchInitial(
          searchHistory: history,
          currentSearchType: state is SearchInitial
              ? (state as SearchInitial).currentSearchType
              : SearchType.all,
        ),
      );
      return;
    }

    // Debounce search for 500ms
    _debounceTimer = Timer(const Duration(milliseconds: 500), () {
      add(SearchSubmitted(event.query));
    });
  }

  Future<void> _onSearchTypeChanged(
    SearchTypeChanged event,
    Emitter<SearchState> emit,
  ) async {
    if (state is SearchInitial) {
      final history = await _searchHistoryService.getSearchHistory();
      emit(
        SearchInitial(
          searchHistory: history,
          currentSearchType: event.searchType,
        ),
      );
    } else if (state is SearchSuccess) {
      final currentState = state as SearchSuccess;
      emit(
        SearchSuccess(
          merchants: currentState.merchants,
          products: currentState.products,
          query: currentState.query,
          searchType: event.searchType,
        ),
      );
    }
  }

  Future<void> _onSearchSubmitted(
    SearchSubmitted event,
    Emitter<SearchState> emit,
  ) async {
    if (event.query.trim().isEmpty) return;

    final searchType = state is SearchInitial
        ? (state as SearchInitial).currentSearchType
        : state is SearchSuccess
        ? (state as SearchSuccess).searchType
        : SearchType.all;

    emit(SearchLoading(searchType: searchType));

    // Save to search history
    await _searchHistoryService.addSearchQuery(event.query.trim());

    switch (searchType) {
      case SearchType.all:
        final merchantsResult = await _merchantService.searchMerchants(
          event.query.trim(),
        );
        final productsResult = await _productService.searchProducts(
          event.query.trim(),
        );

        // Check if both succeeded
        if (merchantsResult.isSuccess && productsResult.isSuccess) {
          emit(
            SearchSuccess(
              merchants: merchantsResult.data,
              products: productsResult.data,
              query: event.query.trim(),
              searchType: searchType,
            ),
          );
        } else {
          // If either failed, show error from the first failure
          final exception =
              merchantsResult.exceptionOrNull ?? productsResult.exceptionOrNull;
          final message = _getErrorMessage(exception!);
          emit(SearchError(message));
        }
        break;

      case SearchType.merchants:
        final result = await _merchantService.searchMerchants(
          event.query.trim(),
        );

        result.when(
          success: (merchants) => emit(
            SearchSuccess(
              merchants: merchants,
              products: const [],
              query: event.query.trim(),
              searchType: searchType,
            ),
          ),
          failure: (exception) {
            final message = _getErrorMessage(exception);
            emit(SearchError(message));
          },
        );
        break;

      case SearchType.products:
        final result = await _productService.searchProducts(event.query.trim());

        result.when(
          success: (products) => emit(
            SearchSuccess(
              merchants: const [],
              products: products,
              query: event.query.trim(),
              searchType: searchType,
            ),
          ),
          failure: (exception) {
            final message = _getErrorMessage(exception);
            emit(SearchError(message));
          },
        );
        break;
    }
  }

  /// Extract user-friendly error message from exception
  String _getErrorMessage(Exception exception) {
    if (exception is AppException) {
      return exception.message;
    }
    return 'An unexpected error occurred';
  }

  Future<void> _onSearchHistoryLoaded(
    SearchHistoryLoaded event,
    Emitter<SearchState> emit,
  ) async {
    try {
      final history = await _searchHistoryService.getSearchHistory();
      emit(
        SearchInitial(
          searchHistory: history,
          currentSearchType: state is SearchInitial
              ? (state as SearchInitial).currentSearchType
              : SearchType.all,
        ),
      );
    } catch (e) {
      emit(SearchError(e.toString()));
    }
  }

  Future<void> _onSearchHistoryCleared(
    SearchHistoryCleared event,
    Emitter<SearchState> emit,
  ) async {
    try {
      await _searchHistoryService.clearSearchHistory();
      emit(const SearchInitial(searchHistory: []));
    } catch (e) {
      emit(SearchError(e.toString()));
    }
  }

  Future<void> _onSearchHistoryItemRemoved(
    SearchHistoryItemRemoved event,
    Emitter<SearchState> emit,
  ) async {
    try {
      await _searchHistoryService.removeSearchQuery(event.query);
      final history = await _searchHistoryService.getSearchHistory();
      emit(
        SearchInitial(
          searchHistory: history,
          currentSearchType: state is SearchInitial
              ? (state as SearchInitial).currentSearchType
              : SearchType.all,
        ),
      );
    } catch (e) {
      emit(SearchError(e.toString()));
    }
  }

  // 🔄 ============= PAGINATION HANDLERS =============

  Future<void> _onLoadMoreSearchResults(
    LoadMoreSearchResults event,
    Emitter<SearchState> emit,
  ) async {
    if (state is! SearchSuccess) return;
    final currentState = state as SearchSuccess;

    if (currentState.searchType == SearchType.products &&
        currentState.productPagination != null &&
        currentState.productPagination!.hasNextPage) {
      final nextPage = currentState.productPagination!.nextPage;
      final result = await _productService.searchProducts(
        currentState.query,
        page: nextPage,
      );

      result.when(
        success: (newProducts) {
          final updatedProducts = [...currentState.products, ...newProducts];
          final updatedPagination = currentState.productPagination!
              .addItems(newProducts)
              .copyWith(
                currentPage: nextPage,
                hasNextPage: newProducts.length >= 20,
              );
          emit(
            SearchSuccess(
              merchants: currentState.merchants,
              products: updatedProducts,
              query: currentState.query,
              searchType: currentState.searchType,
              productPagination: updatedPagination,
              merchantPagination: currentState.merchantPagination,
            ),
          );
        },
        failure: (_) {},
      );
    } else if (currentState.searchType == SearchType.merchants &&
        currentState.merchantPagination != null &&
        currentState.merchantPagination!.hasNextPage) {
      final nextPage = currentState.merchantPagination!.nextPage;
      final result = await _merchantService.searchMerchants(
        currentState.query,
        page: nextPage,
      );

      result.when(
        success: (newMerchants) {
          final updatedMerchants = [...currentState.merchants, ...newMerchants];
          final updatedPagination = currentState.merchantPagination!
              .addItems(newMerchants)
              .copyWith(
                currentPage: nextPage,
                hasNextPage: newMerchants.length >= 20,
              );
          emit(
            SearchSuccess(
              merchants: updatedMerchants,
              products: currentState.products,
              query: currentState.query,
              searchType: currentState.searchType,
              merchantPagination: updatedPagination,
              productPagination: currentState.productPagination,
            ),
          );
        },
        failure: (_) {},
      );
    }
  }

  Future<void> _onRefreshSearchResults(
    RefreshSearchResults event,
    Emitter<SearchState> emit,
  ) async {
    if (state is! SearchSuccess) return;
    final currentState = state as SearchSuccess;
    add(SearchSubmitted(currentState.query));
  }

  @override
  Future<void> close() {
    _debounceTimer?.cancel();
    return super.close();
  }
}
