/// Pagination model for handling paginated data
class PaginationModel<T> {
  final List<T> items;
  final int currentPage;
  final int totalPages;
  final int totalItems;
  final bool hasNextPage;
  final bool hasPreviousPage;
  final bool isLoading;
  final bool isRefreshing;

  const PaginationModel({
    required this.items,
    required this.currentPage,
    required this.totalPages,
    required this.totalItems,
    required this.hasNextPage,
    required this.hasPreviousPage,
    this.isLoading = false,
    this.isRefreshing = false,
  });

  /// Create initial empty pagination
  factory PaginationModel.empty() {
    return PaginationModel<T>(
      items: [],
      currentPage: 0,
      totalPages: 0,
      totalItems: 0,
      hasNextPage: false,
      hasPreviousPage: false,
    );
  }

  /// Create pagination with loading state
  factory PaginationModel.loading() {
    return PaginationModel<T>(
      items: [],
      currentPage: 0,
      totalPages: 0,
      totalItems: 0,
      hasNextPage: false,
      hasPreviousPage: false,
      isLoading: true,
    );
  }

  /// Copy with new values
  PaginationModel<T> copyWith({
    List<T>? items,
    int? currentPage,
    int? totalPages,
    int? totalItems,
    bool? hasNextPage,
    bool? hasPreviousPage,
    bool? isLoading,
    bool? isRefreshing,
  }) {
    return PaginationModel<T>(
      items: items ?? this.items,
      currentPage: currentPage ?? this.currentPage,
      totalPages: totalPages ?? this.totalPages,
      totalItems: totalItems ?? this.totalItems,
      hasNextPage: hasNextPage ?? this.hasNextPage,
      hasPreviousPage: hasPreviousPage ?? this.hasPreviousPage,
      isLoading: isLoading ?? this.isLoading,
      isRefreshing: isRefreshing ?? this.isRefreshing,
    );
  }

  /// Add new items to the list
  PaginationModel<T> addItems(List<T> newItems) {
    return copyWith(items: [...items, ...newItems]);
  }

  /// Replace all items (for refresh)
  PaginationModel<T> replaceItems(List<T> newItems) {
    return copyWith(items: newItems, currentPage: 1);
  }

  /// Set loading state
  PaginationModel<T> setLoading(bool loading) {
    return copyWith(isLoading: loading);
  }

  /// Set refreshing state
  PaginationModel<T> setRefreshing(bool refreshing) {
    return copyWith(isRefreshing: refreshing);
  }

  /// Check if pagination is empty
  bool get isEmpty => items.isEmpty && !isLoading;

  /// Check if pagination has data
  bool get hasData => items.isNotEmpty;

  /// Get next page number
  int get nextPage => currentPage + 1;

  /// Get previous page number
  int get previousPage => currentPage - 1;

  @override
  String toString() {
    return 'PaginationModel(items: ${items.length}, currentPage: $currentPage, totalPages: $totalPages, hasNextPage: $hasNextPage, isLoading: $isLoading)';
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;

    return other is PaginationModel<T> &&
        other.items == items &&
        other.currentPage == currentPage &&
        other.totalPages == totalPages &&
        other.totalItems == totalItems &&
        other.hasNextPage == hasNextPage &&
        other.hasPreviousPage == hasPreviousPage &&
        other.isLoading == isLoading &&
        other.isRefreshing == isRefreshing;
  }

  @override
  int get hashCode {
    return items.hashCode ^
        currentPage.hashCode ^
        totalPages.hashCode ^
        totalItems.hashCode ^
        hasNextPage.hashCode ^
        hasPreviousPage.hashCode ^
        isLoading.hashCode ^
        isRefreshing.hashCode;
  }
}
