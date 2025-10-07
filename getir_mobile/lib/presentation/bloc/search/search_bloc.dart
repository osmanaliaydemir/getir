import 'dart:async';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'search_event.dart';
import 'search_state.dart';
import '../../../domain/usecases/merchant_usecases.dart';
import '../../../domain/usecases/product_usecases.dart';
import '../../../core/services/search_history_service.dart';

class SearchBloc extends Bloc<SearchEvent, SearchState> {
  final SearchMerchantsUseCase _searchMerchantsUseCase;
  final SearchProductsUseCase _searchProductsUseCase;
  final SearchHistoryService _searchHistoryService;

  Timer? _debounceTimer;

  SearchBloc({
    required SearchMerchantsUseCase searchMerchantsUseCase,
    required SearchProductsUseCase searchProductsUseCase,
    required SearchHistoryService searchHistoryService,
  })  : _searchMerchantsUseCase = searchMerchantsUseCase,
        _searchProductsUseCase = searchProductsUseCase,
        _searchHistoryService = searchHistoryService,
        super(const SearchInitial()) {
    on<SearchQueryChanged>(_onSearchQueryChanged);
    on<SearchTypeChanged>(_onSearchTypeChanged);
    on<SearchSubmitted>(_onSearchSubmitted);
    on<SearchHistoryLoaded>(_onSearchHistoryLoaded);
    on<SearchHistoryCleared>(_onSearchHistoryCleared);
    on<SearchHistoryItemRemoved>(_onSearchHistoryItemRemoved);
  }

  Future<void> _onSearchQueryChanged(
    SearchQueryChanged event,
    Emitter<SearchState> emit,
  ) async {
    // Cancel previous debounce timer
    _debounceTimer?.cancel();

    if (event.query.isEmpty) {
      final history = await _searchHistoryService.getSearchHistory();
      emit(SearchInitial(
        searchHistory: history,
        currentSearchType: state is SearchInitial
            ? (state as SearchInitial).currentSearchType
            : SearchType.all,
      ));
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
      emit(SearchInitial(
        searchHistory: history,
        currentSearchType: event.searchType,
      ));
    } else if (state is SearchSuccess) {
      final currentState = state as SearchSuccess;
      emit(SearchSuccess(
        merchants: currentState.merchants,
        products: currentState.products,
        query: currentState.query,
        searchType: event.searchType,
      ));
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

    try {
      // Save to search history
      await _searchHistoryService.addSearchQuery(event.query.trim());

      switch (searchType) {
        case SearchType.all:
          final merchants = await _searchMerchantsUseCase(event.query.trim());
          final products = await _searchProductsUseCase(event.query.trim());
          emit(SearchSuccess(
            merchants: merchants,
            products: products,
            query: event.query.trim(),
            searchType: searchType,
          ));
          break;

        case SearchType.merchants:
          final merchants = await _searchMerchantsUseCase(event.query.trim());
          emit(SearchSuccess(
            merchants: merchants,
            products: const [],
            query: event.query.trim(),
            searchType: searchType,
          ));
          break;

        case SearchType.products:
          final products = await _searchProductsUseCase(event.query.trim());
          emit(SearchSuccess(
            merchants: const [],
            products: products,
            query: event.query.trim(),
            searchType: searchType,
          ));
          break;
      }
    } catch (e) {
      emit(SearchError(e.toString()));
    }
  }

  Future<void> _onSearchHistoryLoaded(
    SearchHistoryLoaded event,
    Emitter<SearchState> emit,
  ) async {
    try {
      final history = await _searchHistoryService.getSearchHistory();
      emit(SearchInitial(
        searchHistory: history,
        currentSearchType: state is SearchInitial
            ? (state as SearchInitial).currentSearchType
            : SearchType.all,
      ));
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
      emit(SearchInitial(
        searchHistory: history,
        currentSearchType: state is SearchInitial
            ? (state as SearchInitial).currentSearchType
            : SearchType.all,
      ));
    } catch (e) {
      emit(SearchError(e.toString()));
    }
  }

  @override
  Future<void> close() {
    _debounceTimer?.cancel();
    return super.close();
  }
}

