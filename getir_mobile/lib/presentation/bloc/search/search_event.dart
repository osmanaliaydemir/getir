import 'package:equatable/equatable.dart';

abstract class SearchEvent extends Equatable {
  const SearchEvent();

  @override
  List<Object?> get props => [];
}

class SearchQueryChanged extends SearchEvent {
  final String query;

  const SearchQueryChanged(this.query);

  @override
  List<Object?> get props => [query];
}

class SearchTypeChanged extends SearchEvent {
  final SearchType searchType;

  const SearchTypeChanged(this.searchType);

  @override
  List<Object?> get props => [searchType];
}

class SearchSubmitted extends SearchEvent {
  final String query;

  const SearchSubmitted(this.query);

  @override
  List<Object?> get props => [query];
}

class SearchHistoryLoaded extends SearchEvent {
  const SearchHistoryLoaded();
}

class SearchHistoryCleared extends SearchEvent {
  const SearchHistoryCleared();
}

class SearchHistoryItemRemoved extends SearchEvent {
  final String query;

  const SearchHistoryItemRemoved(this.query);

  @override
  List<Object?> get props => [query];
}

// 🔄 Pagination Events
class LoadMoreSearchResults extends SearchEvent {
  const LoadMoreSearchResults();
}

class RefreshSearchResults extends SearchEvent {
  const RefreshSearchResults();
}

enum SearchType {
  all,
  merchants,
  products,
}

