import 'package:equatable/equatable.dart';
import '../../../domain/entities/merchant.dart';
import '../../../domain/entities/product.dart';
import 'search_event.dart';

abstract class SearchState extends Equatable {
  const SearchState();

  @override
  List<Object?> get props => [];
}

class SearchInitial extends SearchState {
  final List<String> searchHistory;
  final SearchType currentSearchType;

  const SearchInitial({
    this.searchHistory = const [],
    this.currentSearchType = SearchType.all,
  });

  @override
  List<Object?> get props => [searchHistory, currentSearchType];
}

class SearchLoading extends SearchState {
  final SearchType searchType;

  const SearchLoading({this.searchType = SearchType.all});

  @override
  List<Object?> get props => [searchType];
}

class SearchSuccess extends SearchState {
  final List<Merchant> merchants;
  final List<Product> products;
  final String query;
  final SearchType searchType;

  const SearchSuccess({
    required this.merchants,
    required this.products,
    required this.query,
    required this.searchType,
  });

  bool get isEmpty => merchants.isEmpty && products.isEmpty;

  @override
  List<Object?> get props => [merchants, products, query, searchType];
}

class SearchError extends SearchState {
  final String message;

  const SearchError(this.message);

  @override
  List<Object?> get props => [message];
}

class SearchHistoryUpdated extends SearchState {
  final List<String> searchHistory;
  final SearchType currentSearchType;

  const SearchHistoryUpdated({
    required this.searchHistory,
    this.currentSearchType = SearchType.all,
  });

  @override
  List<Object?> get props => [searchHistory, currentSearchType];
}

