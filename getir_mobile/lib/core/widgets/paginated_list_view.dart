import 'package:flutter/material.dart';

/// Paginated List View Widget
/// Automatically loads more data when scrolling to the bottom
/// Supports pull-to-refresh and error handling
class PaginatedListView<T> extends StatefulWidget {
  final Future<List<T>> Function(int page) onLoadMore;
  final Widget Function(BuildContext context, T item, int index) itemBuilder;
  final Widget Function(BuildContext context)? loadingBuilder;
  final Widget Function(BuildContext context, String error)? errorBuilder;
  final Widget Function(BuildContext context)? emptyBuilder;
  final int itemsPerPage;
  final bool enablePullToRefresh;
  final ScrollPhysics? physics;
  final EdgeInsets? padding;
  final Widget? separator;

  const PaginatedListView({
    super.key,
    required this.onLoadMore,
    required this.itemBuilder,
    this.loadingBuilder,
    this.errorBuilder,
    this.emptyBuilder,
    this.itemsPerPage = 20,
    this.enablePullToRefresh = true,
    this.physics,
    this.padding,
    this.separator,
  });

  @override
  State<PaginatedListView<T>> createState() => _PaginatedListViewState<T>();
}

class _PaginatedListViewState<T> extends State<PaginatedListView<T>> {
  final ScrollController _scrollController = ScrollController();
  final List<T> _items = [];
  int _currentPage = 1;
  bool _isLoading = false;
  bool _hasMore = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _scrollController.addListener(_onScroll);
    _loadInitialData();
  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  /// Load initial data
  Future<void> _loadInitialData() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final newItems = await widget.onLoadMore(1);
      if (mounted) {
        setState(() {
          _items.clear();
          _items.addAll(newItems);
          _currentPage = 1;
          _hasMore = newItems.length >= widget.itemsPerPage;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _error = e.toString();
          _isLoading = false;
        });
      }
    }
  }

  /// Load more data
  Future<void> _loadMore() async {
    if (_isLoading || !_hasMore) return;

    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final newItems = await widget.onLoadMore(_currentPage + 1);
      if (mounted) {
        setState(() {
          _items.addAll(newItems);
          _currentPage++;
          _hasMore = newItems.length >= widget.itemsPerPage;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _error = e.toString();
          _isLoading = false;
        });
      }
    }
  }

  /// Handle scroll events
  void _onScroll() {
    if (_scrollController.position.pixels >=
        _scrollController.position.maxScrollExtent - 200) {
      _loadMore();
    }
  }

  /// Handle pull to refresh
  Future<void> _onRefresh() async {
    await _loadInitialData();
  }

  @override
  Widget build(BuildContext context) {
    // Show error widget
    if (_error != null && _items.isEmpty) {
      return widget.errorBuilder?.call(context, _error!) ??
          _buildDefaultError(_error!);
    }

    // Show loading widget for initial load
    if (_isLoading && _items.isEmpty) {
      return widget.loadingBuilder?.call(context) ?? _buildDefaultLoading();
    }

    // Show empty widget
    if (_items.isEmpty) {
      return widget.emptyBuilder?.call(context) ?? _buildDefaultEmpty();
    }

    // Build list
    final listView = ListView.separated(
      controller: _scrollController,
      physics: widget.physics,
      padding: widget.padding,
      itemCount: _items.length + (_hasMore ? 1 : 0),
      separatorBuilder: (context, index) {
        if (index >= _items.length) {
          return const SizedBox.shrink();
        }
        return widget.separator ?? const SizedBox.shrink();
      },
      itemBuilder: (context, index) {
        // Loading indicator at the end
        if (index >= _items.length) {
          return _buildLoadingIndicator();
        }

        // Item builder
        return widget.itemBuilder(context, _items[index], index);
      },
    );

    // Wrap with RefreshIndicator if enabled
    if (widget.enablePullToRefresh) {
      return RefreshIndicator(
        onRefresh: _onRefresh,
        child: listView,
      );
    }

    return listView;
  }

  /// Default loading widget
  Widget _buildDefaultLoading() {
    return const Center(
      child: CircularProgressIndicator(),
    );
  }

  /// Default error widget
  Widget _buildDefaultError(String error) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.error_outline, size: 64, color: Colors.red),
          const SizedBox(height: 16),
          Text(
            'Error: $error',
            textAlign: TextAlign.center,
            style: const TextStyle(color: Colors.red),
          ),
          const SizedBox(height: 16),
          ElevatedButton(
            onPressed: _loadInitialData,
            child: const Text('Retry'),
          ),
        ],
      ),
    );
  }

  /// Default empty widget
  Widget _buildDefaultEmpty() {
    return const Center(
      child: Text('No items found'),
    );
  }

  /// Loading indicator at the bottom
  Widget _buildLoadingIndicator() {
    return const Padding(
      padding: EdgeInsets.all(16.0),
      child: Center(
        child: SizedBox(
          width: 24,
          height: 24,
          child: CircularProgressIndicator(strokeWidth: 2),
        ),
      ),
    );
  }
}

/// Paginated Grid View Widget
/// Similar to PaginatedListView but for grid layouts
class PaginatedGridView<T> extends StatefulWidget {
  final Future<List<T>> Function(int page) onLoadMore;
  final Widget Function(BuildContext context, T item, int index) itemBuilder;
  final int crossAxisCount;
  final double mainAxisSpacing;
  final double crossAxisSpacing;
  final double childAspectRatio;
  final int itemsPerPage;
  final bool enablePullToRefresh;
  final EdgeInsets? padding;

  const PaginatedGridView({
    super.key,
    required this.onLoadMore,
    required this.itemBuilder,
    this.crossAxisCount = 2,
    this.mainAxisSpacing = 8.0,
    this.crossAxisSpacing = 8.0,
    this.childAspectRatio = 1.0,
    this.itemsPerPage = 20,
    this.enablePullToRefresh = true,
    this.padding,
  });

  @override
  State<PaginatedGridView<T>> createState() => _PaginatedGridViewState<T>();
}

class _PaginatedGridViewState<T> extends State<PaginatedGridView<T>> {
  final ScrollController _scrollController = ScrollController();
  final List<T> _items = [];
  int _currentPage = 1;
  bool _isLoading = false;
  bool _hasMore = true;

  @override
  void initState() {
    super.initState();
    _scrollController.addListener(_onScroll);
    _loadInitialData();
  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  Future<void> _loadInitialData() async {
    setState(() => _isLoading = true);

    try {
      final newItems = await widget.onLoadMore(1);
      if (mounted) {
        setState(() {
          _items.clear();
          _items.addAll(newItems);
          _currentPage = 1;
          _hasMore = newItems.length >= widget.itemsPerPage;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  Future<void> _loadMore() async {
    if (_isLoading || !_hasMore) return;

    setState(() => _isLoading = true);

    try {
      final newItems = await widget.onLoadMore(_currentPage + 1);
      if (mounted) {
        setState(() {
          _items.addAll(newItems);
          _currentPage++;
          _hasMore = newItems.length >= widget.itemsPerPage;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  void _onScroll() {
    if (_scrollController.position.pixels >=
        _scrollController.position.maxScrollExtent - 200) {
      _loadMore();
    }
  }

  Future<void> _onRefresh() async {
    await _loadInitialData();
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading && _items.isEmpty) {
      return const Center(child: CircularProgressIndicator());
    }

    final gridView = GridView.builder(
      controller: _scrollController,
      padding: widget.padding,
      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: widget.crossAxisCount,
        mainAxisSpacing: widget.mainAxisSpacing,
        crossAxisSpacing: widget.crossAxisSpacing,
        childAspectRatio: widget.childAspectRatio,
      ),
      itemCount: _items.length + (_hasMore && _isLoading ? 1 : 0),
      itemBuilder: (context, index) {
        if (index >= _items.length) {
          return const Center(
            child: CircularProgressIndicator(),
          );
        }
        return widget.itemBuilder(context, _items[index], index);
      },
    );

    if (widget.enablePullToRefresh) {
      return RefreshIndicator(
        onRefresh: _onRefresh,
        child: gridView,
      );
    }

    return gridView;
  }
}
